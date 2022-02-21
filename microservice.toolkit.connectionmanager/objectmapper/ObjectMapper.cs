using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace microservice.toolkit.connectionmanager.objectmapper;

/// <summary>
/// Provides by-name member-access to objects of a given type
/// </summary>
internal abstract class ObjectMapper
{
    private static AssemblyBuilder assembly;
    private static ModuleBuilder module;
    private static int counter;

    private static readonly MethodInfo TryGetValue = typeof(Dictionary<string, int>).GetMethod("TryGetValue");

    // hash-table has better read-without-locking semantics than dictionary
    private static readonly Hashtable PublicAccessors = new();

    /// <summary>
    /// Get or set the value of a named member on the target instance
    /// </summary>
    internal abstract object this[object target, string name]
    {
        set;
    }

    /// <summary>
    /// Query the members available for this type
    /// </summary>
    internal abstract string[] GetMemberNames();

    /// <summary>
    /// Provides a type-specific accessor, allowing by-name access for all objects of that type
    /// </summary>
    /// <remarks>The accessor is cached internally; a pre-existing accessor may be returned</remarks>
    internal static ObjectMapper Create(Type type)
    {
        var obj = (ObjectMapper)PublicAccessors[type];
        if (obj != null)
        {
            return obj;
        }

        lock (PublicAccessors)
        {
            // double-check
            obj = (ObjectMapper)PublicAccessors[type];
            if (obj != null)
            {
                return obj;
            }

            obj = CreateNew(type);

            PublicAccessors[type] = obj;
            return obj;
        }
    }

    private static int GetNextCounterValue()
    {
        return Interlocked.Increment(ref counter);
    }

    private static void WriteMapImpl(ILGenerator il, Type type, IReadOnlyList<MemberInfo> members,
        FieldInfo mapField, bool isGet)
    {
        OpCode obj, index, value;

        var fail = il.DefineLabel();
        if (mapField == null)
        {
            index = OpCodes.Ldarg_0;
            obj = OpCodes.Ldarg_1;
            value = OpCodes.Ldarg_2;
        }
        else
        {
            il.DeclareLocal(typeof(int));
            index = OpCodes.Ldloc_0;
            obj = OpCodes.Ldarg_1;
            value = OpCodes.Ldarg_3;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, mapField);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldloca_S, (byte)0);
            il.EmitCall(OpCodes.Callvirt, TryGetValue, null);
            il.Emit(OpCodes.Brfalse, fail);
        }

        var labels = new Label[members.Count];
        for (var i = 0; i < labels.Length; i++)
        {
            labels[i] = il.DefineLabel();
        }

        il.Emit(index);
        il.Emit(OpCodes.Switch, labels);
        il.MarkLabel(fail);
        il.Emit(OpCodes.Ldstr, "name");
        il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new[] {typeof(string)}));
        il.Emit(OpCodes.Throw);
        for (var i = 0; i < labels.Length; i++)
        {
            il.MarkLabel(labels[i]);
            var member = members[i];
            var isFail = true;

            switch (member)
            {
                case PropertyInfo prop:
                    var propType = prop.PropertyType;
                    bool isByRef = propType.IsByRef, isValid = true;
                    if (isByRef)
                    {
                        if (!isGet && prop.CustomAttributes.Any(x =>
                                x.AttributeType.FullName ==
                                "System.Runtime.CompilerServices.IsReadOnlyAttribute"))
                        {
                            isValid = false; // can't assign indirectly to ref-readonly
                        }

                        propType = propType.GetElementType(); // from "ref Foo" to "Foo"
                    }

                    var accessor = (isGet | isByRef)
                        ? prop.GetGetMethod(false)
                        : prop.GetSetMethod(false);
                    if (isValid && prop.CanRead && accessor != null)
                    {
                        il.Emit(obj);
                        Cast(il, type, true); // cast the input object to the right target type

                        if (isGet)
                        {
                            il.EmitCall(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, accessor, null);
                            if (isByRef)
                            {
                                il.Emit(OpCodes.Ldobj, propType); // defererence if needed
                            }

                            if (propType.IsValueType)
                            {
                                il.Emit(OpCodes.Box, propType); // box the value if needed
                            }
                        }
                        else
                        {
                            // when by-ref, we get the target managed pointer *first*, i.e. put obj.TheRef on the stack
                            if (isByRef)
                            {
                                il.EmitCall(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, accessor, null);
                            }

                            // load the new value, and type it
                            il.Emit(value);
                            Cast(il, propType, false);

                            if (isByRef)
                            {
                                // assign to the managed pointer
                                il.Emit(OpCodes.Stobj, propType);
                            }
                            else
                            {
                                // call the setter
                                il.EmitCall(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, accessor, null);
                            }
                        }

                        il.Emit(OpCodes.Ret);
                        isFail = false;
                    }

                    break;
            }

            if (isFail)
            {
                il.Emit(OpCodes.Br, fail);
            }
        }
    }

    private static bool IsFullyPublic(Type type)
    {
        while (type.IsNestedPublic)
        {
            type = type.DeclaringType;
        }

        return type.IsPublic;
    }

    private static ObjectMapper CreateNew(Type type)
    {
        if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
        {
            return DynamicAccessor.Instance;
        }

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var map = new Dictionary<string, int>();
        var members = new List<MemberInfo>(props.Length);
        var i = 0;
        foreach (var prop in props)
        {
            if (!map.ContainsKey(prop.Name) && prop.GetIndexParameters().Length == 0)
            {
                map.Add(prop.Name, i++);
                members.Add(prop);
            }
        }

        var ctor = type.GetConstructor(Type.EmptyTypes);

        ILGenerator il;
        if (!IsFullyPublic(type))
        {
            DynamicMethod dynGetter = new(type.FullName + "_get", typeof(object),
                    new[] {typeof(int), typeof(object)}, type, true),
                dynSetter = new(type.FullName + "_set", null, new[] {typeof(int), typeof(object), typeof(object)},
                    type, true);
            WriteMapImpl(dynGetter.GetILGenerator(), type, members, null, true);
            WriteMapImpl(dynSetter.GetILGenerator(), type, members, null, false);

            var dynCtor = new DynamicMethod(type.FullName + "_ctor", typeof(object), Type.EmptyTypes, type, true);
            il = dynCtor.GetILGenerator();
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);

            return new DelegateAccessor(
                map,
                (Action<int, object, object>)dynSetter.CreateDelegate(typeof(Action<int, object, object>)), type);
        }

        // note this region is synchronized; only one is being created at a time so we don't need to stress about the builders
        if (assembly == null)
        {
            var name = new AssemblyName("FastMember_dynamic");
            assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            module = assembly.DefineDynamicModule(name.Name);
        }

        var attribs = typeof(ObjectMapper).Attributes;
        var tb = module.DefineType("FastMember_dynamic." + type.Name + "_" + GetNextCounterValue(),
            (attribs | TypeAttributes.Sealed | TypeAttributes.Public) &
            ~(TypeAttributes.Abstract | TypeAttributes.NotPublic), typeof(RuntimeObjectMapper));

        il = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
            new[] {typeof(Dictionary<string, int>)}).GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        var mapField = tb.DefineField("_map", typeof(Dictionary<string, int>),
            FieldAttributes.InitOnly | FieldAttributes.Private);
        il.Emit(OpCodes.Stfld, mapField);
        il.Emit(OpCodes.Ret);

        var indexer = typeof(ObjectMapper).GetProperty("Item");
        MethodInfo baseGetter = indexer.GetGetMethod(), baseSetter = indexer.GetSetMethod();
        var body = tb.DefineMethod(baseGetter.Name, baseGetter.Attributes & ~MethodAttributes.Abstract,
            typeof(object), new[] {typeof(object), typeof(string)});
        il = body.GetILGenerator();
        WriteMapImpl(il, type, members, mapField, true);
        tb.DefineMethodOverride(body, baseGetter);

        body = tb.DefineMethod(baseSetter.Name, baseSetter.Attributes & ~MethodAttributes.Abstract, null,
            new[] {typeof(object), typeof(string), typeof(object)});
        il = body.GetILGenerator();
        WriteMapImpl(il, type, members, mapField, false);
        tb.DefineMethodOverride(body, baseSetter);

        var baseMethod = typeof(ObjectMapper).GetProperty("CreateNewSupported").GetGetMethod();
        body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes, baseMethod.ReturnType, Type.EmptyTypes);
        il = body.GetILGenerator();
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Ret);
        tb.DefineMethodOverride(body, baseMethod);

        baseMethod = typeof(ObjectMapper).GetMethod("CreateNew");
        body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes, baseMethod.ReturnType, Type.EmptyTypes);
        il = body.GetILGenerator();
        il.Emit(OpCodes.Newobj, ctor);
        il.Emit(OpCodes.Ret);
        tb.DefineMethodOverride(body, baseMethod);

        baseMethod = typeof(RuntimeObjectMapper).GetProperty("Type", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetGetMethod(true);
        body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes & ~MethodAttributes.Abstract,
            baseMethod.ReturnType, Type.EmptyTypes);
        il = body.GetILGenerator();
        il.Emit(OpCodes.Ldtoken, type);
        il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
        il.Emit(OpCodes.Ret);
        tb.DefineMethodOverride(body, baseMethod);

        var accessor = (ObjectMapper)Activator.CreateInstance(tb.CreateTypeInfo().AsType(), map);
        return accessor;
    }

    private static void Cast(ILGenerator il, Type type, bool valueAsPointer)
    {
        if (type == typeof(object)) { }
        else if (type.IsValueType)
        {
            il.Emit(valueAsPointer ? OpCodes.Unbox : OpCodes.Unbox_Any, type);
        }
        else
        {
            il.Emit(OpCodes.Castclass, type);
        }
    }

    private sealed class DynamicAccessor : ObjectMapper
    {
        private static DynamicAccessor instance;
        internal static DynamicAccessor Instance => instance ??= new DynamicAccessor();

        private DynamicAccessor() { }

        internal override string[] GetMemberNames()
        {
            return Array.Empty<string>();
        }

        internal override object this[object target, string name]
        {
            set { ObjectMapperCache.SetValue(name, target, value); }
        }
    }

    /// <summary>
    /// A TypeAccessor based on a Type implementation, with available member metadata
    /// </summary>
    private abstract class RuntimeObjectMapper : ObjectMapper
    {
        /// <summary>
        /// Returns the Type represented by this accessor
        /// </summary>
        protected abstract Type Type { get; }

        /// <summary>
        /// Can this type be queried for member availability?
        /// </summary>
        internal virtual bool GetMembersSupported { get { return true; } }

        private string[] memberNames;

        /// <summary>
        /// Query the members available for this type
        /// </summary>
        internal override string[] GetMemberNames()
        {
            const BindingFlags publicInstance = BindingFlags.Public | BindingFlags.Instance;
            return this.memberNames ??= this.Type.GetProperties(publicInstance)
                .Concat(this.Type.GetFields(publicInstance).Cast<MemberInfo>()).OrderBy(x => x.Name)
                .Select(member => member.Name).ToArray();
        }
    }

    private sealed class DelegateAccessor : RuntimeObjectMapper
    {
        private readonly Dictionary<string, int> map;
        private readonly Action<int, object, object> setter;

        protected override Type Type { get; }

        internal DelegateAccessor(Dictionary<string, int> map,
            Action<int, object, object> setter, Type type)
        {
            this.map = map;
            this.setter = setter;
            this.Type = type;
        }

        internal override object this[object target, string name]
        {
            set
            {
                if (this.map.TryGetValue(name, out var index))
                {
                    this.setter(index, target, value);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(name));
                }
            }
        }
    }
}