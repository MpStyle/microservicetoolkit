using Microsoft.CSharp.RuntimeBinder;

using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace microservice.toolkit.connectionmanager.objectmapper
{
    internal static class ObjectMapperCache
    {
        private static readonly Hashtable Setters = new();

        internal static void SetValue(string name, object target, object value)
        {
            var callSite = (CallSite<Func<CallSite, object, object, object>>)Setters[name];
            if (callSite == null)
            {
                var newSite = CallSite<Func<CallSite, object, object, object>>.Create(Binder.SetMember(
                    CSharpBinderFlags.None, name, typeof(ObjectMapperCache),
                    new[]
                    {
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                        CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                    }));
                lock (Setters)
                {
                    callSite = (CallSite<Func<CallSite, object, object, object>>)Setters[name];
                    if (callSite == null)
                    {
                        Setters[name] = callSite = newSite;
                    }
                }
            }

            callSite.Target(callSite, target, value);
        }
    }
}