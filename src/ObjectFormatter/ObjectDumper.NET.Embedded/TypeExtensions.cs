using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ObjectFormatter.ObjectDumper.NET.Embedded
{
    internal static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> TypeToKeywordMappings = new()
        {
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(sbyte), "sbyte" },
            { typeof(float), "float" },
            { typeof(ushort), "ushort" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(void), "void" }
        };

        internal static string GetFormattedNameVB(this Type type, bool useFullName = false)
        {
            var typeName = useFullName ? type.FullName : type.Name;
            
            var typeInfo = type.GetTypeInfo();

            if (!typeInfo.IsGenericType)
            {
                return typeName.Replace("[]", "()");
            }

            return $"{typeName?.Substring(0, typeName.IndexOf('`'))}(Of {string.Join(", ", typeInfo.GenericTypeArguments.Select(t => t.GetFormattedName(useFullName)))})";

        }

        internal static bool TryGetBuiltInTypeName(this Type type, out string value)
        {
            return TypeToKeywordMappings.TryGetValue(type, out value);
        }

        internal static bool IsKeyword(string value)
        {
            return TypeToKeywordMappings.Values.Contains(value);
        }

        internal static string GetFormattedName(this Type type, bool useFullName = false, bool useValueTupleFormatting = true)
        {
            type = TryGetInnerElementType(type, out var arrayBrackets);

            var typeName = GetTypeName(type, useFullName, useValueTupleFormatting);

            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType || type.IsAnonymous())
            {
                return $"{typeName}{arrayBrackets}";
            }

            if (useValueTupleFormatting && type.IsValueTuple())
            {
                return typeName.RemoveGenericBackTick();
            }

            string genericTypeParametersString;
            if (typeInfo.IsGenericTypeDefinition)
            {
                // Used for open generic types
                genericTypeParametersString = $"{string.Join(",", typeInfo.GenericTypeParameters.Select(t => string.Empty))}";
            }
            else
            {
                // Used for regular generic types
                genericTypeParametersString = $"{string.Join(", ", typeInfo.GenericTypeArguments.Select(t => t.GetFormattedName(useFullName, useValueTupleFormatting)))}";
            }

            typeName = typeName.RemoveGenericBackTick();

            return $"{typeName}<{genericTypeParametersString}>{arrayBrackets}";
        }

        private static string RemoveGenericBackTick(this string typeName)
        {
            int iBacktick = typeName.IndexOf('`');
            if (iBacktick > 0)
            {
                typeName = typeName.Remove(iBacktick);
            }

            return typeName;
        }

        private static string GetTypeName(Type type, bool useFullName, bool useValueTupleFormatting)
        {
            if (type.IsAnonymous())
            {
                return string.Empty;
            }
            
            string typeName;

            if (useFullName == false && type.TryGetBuiltInTypeName(out var keyword))
            {
                return keyword;
            }
            else
            if (useValueTupleFormatting && type.IsValueTuple())
            {
                typeName = $"({string.Join(", ", type.GenericTypeArguments.Select(t => GetTypeName(t, useFullName, useValueTupleFormatting)))})";
            }
            else
            {
                typeName = useFullName ? type.FullName : type.Name;
            }

            return typeName;
        }

        private static Type TryGetInnerElementType(Type type, out string arrayBrackets)
        {
            arrayBrackets = null;
            if (!type.IsArray)
            {
                return type;
            }

            do
            {
                arrayBrackets += "[" + new string(',', type.GetArrayRank() - 1) + "]";
                type = type.GetElementType();
            }
            while (type.IsArray);

            return type;
        }

        public static object GetDefault(this Type t)
        {
            var defaultValue = typeof(TypeExtensions).GetRuntimeMethod(nameof(GetDefaultGeneric), new Type[] { }).MakeGenericMethod(t).Invoke(null, null);
            return defaultValue;
        }

        public static object TryGetDefault(this Type t)
        {
            object value;

            try
            {
                value = t.GetDefault();
            }
            catch (Exception ex)
            {
                value = $"{{{ex.GetType().Name}: {ex.Message}}}";
            }

            return value;
        }

        public static T GetDefaultGeneric<T>()
        {
            return default;
        }

        public static bool IsAnonymous(this Type type)
        {
            return type.GetTypeInfo().IsAnonymous();
        }

        public static bool IsAnonymous(this TypeInfo typeInfo)
        {
            if (typeInfo.IsGenericType)
            {
                var genericTypeDefinition = typeInfo.GetGenericTypeDefinition().GetTypeInfo();
                if (genericTypeDefinition.IsClass && genericTypeDefinition.IsSealed && genericTypeDefinition.Attributes.HasFlag(TypeAttributes.NotPublic))
                {
                    var attributes = genericTypeDefinition.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
                    if (attributes != null && attributes.Any())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsValueTuple(this Type type)
        {
            return
                type.IsValueType &&
                type.FullName is string fullName &&
                (fullName.StartsWith("System.ValueTuple") || fullName.StartsWith("System.ValueTuple`"));
        }
    }
}
