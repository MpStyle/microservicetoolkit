using System;
using System.Linq;
using System.Reflection;
using microservice.toolkit.core.extension;

using System.Diagnostics;

namespace microservice.toolkit.messagemediator.extension
{
    public static class AssemblyScannerExtension
    {
        public static Type[] GetServices(this Assembly assembly)
        {
            return assembly
                .GetExportedTypes()
                .Where(y => y.IsClass && !y.IsAbstract && !y.IsGenericType && !y.IsNested && y.IsService())
                .ToArray();
        }
        
        public static Type[] GetAssemblyServices(this Type type)
        {
            var assembly = Assembly.GetAssembly(type);

            if (assembly == null)
            {
                return Array.Empty<Type>();
            }

            return assembly
                .GetExportedTypes()
                .Where(y => y.IsClass && !y.IsAbstract && !y.IsGenericType && !y.IsNested && y.IsService())
                .ToArray();
        }
        
        private static bool IsService(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            var fullname = typeof(Service<,>).FullName;

            if (fullname.IsNullOrEmpty())
            {
                return false;
            }

            Debug.Assert(fullname != null);
            
            var name = fullname[..fullname.IndexOf('`')];
            var currentType = type;
            while (currentType != null)
            {
                if (currentType.BaseType?.FullName?.StartsWith(name) == true)
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }
    }
}