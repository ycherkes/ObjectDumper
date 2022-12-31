using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using YellowFlavor.Serialization.Embedded.CodeDom.ms;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.VisualBasic;
using YellowFlavor.Serialization.Extensions;

namespace YellowFlavor.Serialization.Embedded.CodeDom
{
    internal static class ReflectionUtils
    {
        public static string ComposeCsharpVariableName(Type type)
        {
            var result = ComposeVariableName(type, GetFormattedCsharpTypeName);

            if (CSharpHelpers.IsKeyword(result) || string.Equals(type.Name, result, StringComparison.Ordinal))
            {
                result += "Value";
            }

            return result;
        }

        public static string ComposeVisualBasicVariableName(Type type)
        {
            var result = ComposeVariableName(type, GetFormattedVisualBasicTypeName);

            if (VBCodeGenerator.IsKeyword(result) || string.Equals(type.Name, result, StringComparison.OrdinalIgnoreCase))
            {
                result += "Value";
            }

            return result;
        }
        private static string ComposeVariableName(Type type, Func<Type, string> typeNameFormatter)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(typeNameFormatter(type).ToCamelCase());
            var innerType = GetCollectionItemType(type);

            while (innerType != type)
            {
                if (type == typeof(string) && innerType == typeof(char))
                {
                    break;
                }
                stringBuilder.Append("Of");
                if (type.Name.Contains("Dictionary"))
                {
                    innerType = type.GetGenericArguments().LastOrDefault() ?? typeof(object);
                }
                stringBuilder.Append(typeNameFormatter(innerType).ToPascalCase());
                type = innerType;
                innerType = GetCollectionItemType(type);
            }

            return stringBuilder.ToString();
        }

        private static readonly Dictionary<string, string> VisualBasicReplacements = new(StringComparer.OrdinalIgnoreCase)
        {
            { "int16", "Short" },
            { "int32", "Integer" },
            { "int64", "Long" },
            { "uint16", "UShort" },
            { "uint32", "UInteger" },
            { "uint64", "ULong" },
            { "datetime", "Date" },
        };

        private static string GetFormattedVisualBasicTypeName(Type type)
        {
            var result = GetFormattedTypeName(type);

            return VisualBasicReplacements.TryGetValue(result, out string replacement)
                ? replacement
                : result;
        }

        private static readonly Dictionary<string, string> CSharpReplacements = new(StringComparer.OrdinalIgnoreCase)
        {
            { "int16", "short" },
            { "int32", "int" },
            { "int64", "long" },
            { "boolean", "bool" },
            { "uint16", "ushort" },
            { "uint32", "uint" },
            { "uint64", "ulong" },
            { "single", "float" }
        };

        private static string GetFormattedCsharpTypeName(Type type)
        {
            var result = GetFormattedTypeName(type);

            return CSharpReplacements.TryGetValue(result, out string replacement)
                ? replacement
                : result;
        }

        private static string GetFormattedTypeName(Type type)
        {
            var typeName = type.Name;

            var result = type.IsArray
                ? "array"
                : type.IsAnonymousType()
                    ? "AnonymousType"
                      : typeName.StartsWith("<")
                        ? typeName.Substring(1).Split('>')[0]
                        : typeName.Split('`')[0];

            if (type.IsInterface() && result.StartsWith("I", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Substring(1);
            }
            return result;
        }

        private static Type GetCollectionItemType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            if (ImplementsGenericDefinition(type, typeof(IEnumerable<>), out Type genericListType))
            {
                if (genericListType.IsGenericTypeDefinition())
                {
                    throw new Exception($"Type {type} is not a collection.");
                }

                return genericListType!.GetGenericArguments()[0];
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return typeof(object);
            }

            return type;
        }

        private static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, out Type implementingType)
        {
            if (!genericInterfaceDefinition.IsInterface() || !genericInterfaceDefinition.IsGenericTypeDefinition())
            {
                throw new ArgumentNullException($"'{genericInterfaceDefinition}' is not a generic interface definition.");
            }

            if (type.IsInterface())
            {
                if (type.IsGenericType())
                {
                    Type interfaceDefinition = type.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = type;
                        return true;
                    }
                }
            }

            foreach (Type i in type.GetInterfaces())
            {
                if (i.IsGenericType())
                {
                    Type interfaceDefinition = i.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = i;
                        return true;
                    }
                }
            }

            implementingType = null;
            return false;
        }

        public static bool IsAnonymousType(this Type type)
        {
            var isAnonymousType = Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                                    && type.IsGenericType
                                    && type.Name.Contains("AnonymousType")
                                    && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) ||
                                        type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase));

            return isAnonymousType;
        }

        public static bool ContainsAnonymousType(Type type)
        {
            var elementType = GetInnerElementType(type);

            while (elementType != null && elementType != typeof(object) && type != typeof(string))
            {
                type = elementType;
                elementType = GetInnerElementType(type);
            }

            return IsAnonymousType(type);
        }

        public static Type GetInnerElementType(Type type)
        {
            var elementType = type.IsArray ? type.GetElementType() : type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))?.GenericTypeArguments.FirstOrDefault() ?? typeof(object);

            return elementType;
        }

        public static bool IsGenericCollection(Type type)
        {
            var hasICollection = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>)) != null;

            return hasICollection;
        }

        public static bool IsPublicImmutableCollection(Type type)
        {
            var typeFullName = type.FullName ?? "";

            return type.IsPublic && typeFullName.StartsWith("System.Collections.Immutable");
        }

        public static bool IsReadonlyCollection(Type type)
        {
            var typeFullName = type.FullName ?? "";

            return type.IsValueType() && typeFullName.StartsWith("System.Collections.ObjectModel.ReadOnlyCollection");
        }

        public static bool IsTuple(Type type)
        {
            var typeFullName = type.FullName ?? "";

            return typeFullName.StartsWith("System.Tuple");
        }

        public static bool IsValueTuple(Type type)
        {
            var typeFullName = type.FullName ?? "";

            return type.IsValueType() && typeFullName.StartsWith("System.ValueTuple");
        }

        public static bool IsKeyValuePair(Type type)
        {
            if (type.IsGenericType())
            {
                return type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
            }
            return false;
        }

        public static object GetDefaultValue(Type type)
        {
            if (!type.IsValueType())
            {
                return null;
            }

            switch (type.FullName)
            {
                case "System.Boolean":
                    return false;
                case "System.Char":
                case "System.SByte":
                case "System.Byte":
                case "System.Int16":
                case "System.UInt16":
                case "System.Int32":
                case "System.UInt32":
                    return 0;
                case "System.Int64":
                case "System.UInt64":
                    return 0L;
                case "System.Single":
                    return 0f;
                case "System.Double":
                    return 0.0;
                case "System.Decimal":
                    return 0m;
                case "System.DateTime":
                    return new DateTime();
#if HAVE_BIG_INTEGER
                case "System.Numerics.BigInteger":
                    return new System.Numerics.BigInteger();
#endif
                case "System.Guid":
                    return new Guid();
#if HAVE_DATE_TIME_OFFSET
                case "System.DateTimeOffset":
                    return new DateTimeOffset();
#endif
            }

            if (IsNullable(type))
            {
                return null;
            }

            try
            {
                // possibly use IL initobj for perf here?
                return Activator.CreateInstance(type);
            }
            catch
            {
                return 0;
            }
        }

        public static bool IsNullableType(Type t)
        {
            return t.IsGenericType() && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsInterface(this Type type)
        {
#if HAVE_FULL_REFLECTION
            return type.IsInterface;
#else
            return type.GetTypeInfo().IsInterface;
#endif
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
#if HAVE_FULL_REFLECTION
            return type.IsGenericTypeDefinition;
#else
            return type.GetTypeInfo().IsGenericTypeDefinition;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if HAVE_FULL_REFLECTION
            return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if HAVE_FULL_REFLECTION
            return type.IsValueType;
#else
            return type.GetTypeInfo().IsValueType;
#endif
        }

        private static bool IsNullable(Type t)
        {
            if (t.IsValueType())
            {
                return IsNullableType(t);
            }

            return true;
        }

        public static bool IsGrouping(Type type)
        {
            var hasIGrouping = type.GetInterfaces().Concat(new[] { type }).FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IGrouping<,>)) != null;

            return hasIGrouping;
        }

        public static bool IsLookup(Type type)
        {
            var hasILookup = type.GetInterfaces().Concat(new[] { type }).FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ILookup<,>)) != null;

            return hasILookup;
        }
    }
}
