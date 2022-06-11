using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json.Embedded.Utilities;
using ObjectFormatter.CodeDom.Embedded.ms.CodeDom.Microsoft.VisualBasic;
using ObjectFormatter.CodeDom.Embedded.ms.Common.src.Sys;
using ObjectFormatter.YamlDotNet.Embedded.Serialization.Utilities;

namespace ObjectFormatter.CodeDom.Embedded
{
    internal static class ReflectionUtils
    {
        public static string ComposeVariableName(Type type)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(GetFormattedTypeName(type).ToCamelCase());
            var innerType = GetCollectionItemType(type);

            while (innerType != type)
            {
                stringBuilder.Append("Of");
                if (type.Name.Contains("Dictionary")) 
                {
                    innerType = type.GetGenericArguments().LastOrDefault() ?? typeof(object);
                }
                stringBuilder.Append(GetFormattedTypeName(innerType).ToPascalCase());
                if(innerType == typeof(string))
                {
                    break;
                }
                type = innerType;
                innerType = GetCollectionItemType(type);
            }

            var result = stringBuilder.ToString();

            if(CSharpHelpers.IsKeyword(result) || VBCodeGenerator.IsKeyword(result))
            {
                result += "Value";
            }

            return result;
        }

        private static string GetFormattedTypeName(Type type)
        {
            var result = type.IsArray 
                ? "array" 
                : type.IsAnonymousType() 
                    ? "AnonymousType" 
                    : type.Name.Split('`')[0];

            if(type.IsInterface() && result.StartsWith("I", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Substring(1);
            }
            return result;
        }

        public static Type GetCollectionItemType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            if (ImplementsGenericDefinition(type, typeof(IEnumerable<>), out Type? genericListType))
            {
                if (genericListType!.IsGenericTypeDefinition())
                {
                    throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, type));
                }

                return genericListType!.GetGenericArguments()[0];
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return typeof(object);
            }

            return type;
        }

        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, [NotNullWhen(true)] out Type? implementingType)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));
            ValidationUtils.ArgumentNotNull(genericInterfaceDefinition, nameof(genericInterfaceDefinition));

            if (!genericInterfaceDefinition.IsInterface() || !genericInterfaceDefinition.IsGenericTypeDefinition())
            {
                throw new ArgumentNullException("'{0}' is not a generic interface definition.".FormatWith(CultureInfo.InvariantCulture, genericInterfaceDefinition));
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
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
            var nameContainsAnonymousType = (type.FullName ?? "").Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

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
            var elementType = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))?.GenericTypeArguments.FirstOrDefault() ?? typeof(object);

            return elementType;
        }

        public static bool IsGenericCollection(Type type)
        {
            var hasICollection = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>)) != null;

            return hasICollection;
        }

        public static bool IsImmutableCollection(Type type)
        {
            var typeFullName = type.FullName ?? "";

            return type.IsValueType() && typeFullName.StartsWith("System.Collections.Immutable");
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

            // possibly use IL initobj for perf here?
            return Activator.CreateInstance(type);
        }

        public static bool IsNullableType(Type t)
        {
            return t.IsGenericType() && t.GetGenericTypeDefinition() == typeof(Nullable<>);
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

        public static bool IsNullable(Type t)
        {
            if (t.IsValueType())
            {
                return IsNullableType(t);
            }

            return true;
        }
    }
}
