using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using YAXLib;
using YAXLib.Enums;
using YAXLib.KnownTypes;
using YAXLib.Options;
using YellowFlavor.Serialization.Extensions;
using YellowFlavor.Serialization.Implementation.Settings;
using YellowFlavor.Serialization.Implementation.Xml;

namespace YellowFlavor.Serialization.Implementation
{
    internal class XmlSerializer : ISerializer
    {
        private static SerializerOptions SerializerOptions => new()
        {
            ExceptionHandlingPolicies = YAXExceptionHandlingPolicies.ThrowErrorsOnly,
            ExceptionBehavior = YAXExceptionTypes.Ignore,
            SerializationOptions = YAXSerializationOptions.SuppressMetadataAttributes | YAXSerializationOptions.DontSerializeNullObjects | YAXSerializationOptions.DontSerializeDefaultValues,
            MaxRecursion = 25,
            TypeResolver = new CustomResolver()
        };

        private static readonly string XmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine;

        public string Serialize(object obj, string settings)
        {
            if (obj == null)
            {
                return XmlHeader;
            }

            if (!WellKnownTypes.TryGetKnownType(typeof(IPAddress), out _))
            {
                WellKnownTypes.Add(new IpAddressKnownType());
            }
            
            var xmlSettings = GetXmlSettings(settings);
            var serializer = new YAXSerializer(obj.GetType(), xmlSettings);
            var serializedValue = serializer.Serialize(obj);
            return XmlHeader + serializedValue;
        }

        private static SerializerOptions GetXmlSettings(string settings)
        {
            var newSettings = SerializerOptions;
            if (settings == null) return newSettings;

            var xmlSettings = JsonConvert.DeserializeObject<XmlSettings>(settings);
            newSettings.MaxRecursion = xmlSettings.MaxDepth > 0 ? xmlSettings.MaxDepth : 1;

            newSettings.SerializationOptions |= xmlSettings.IgnoreNullValues
                ? YAXSerializationOptions.DontSerializeNullObjects
                : YAXSerializationOptions.SerializeNullObjects;

            newSettings.SerializationOptions ^= xmlSettings.IgnoreNullValues
                ? YAXSerializationOptions.SerializeNullObjects
                : YAXSerializationOptions.DontSerializeNullObjects;

            if (xmlSettings.IgnoreDefaultValues)
            {
                newSettings.SerializationOptions |= YAXSerializationOptions.DontSerializeDefaultValues;
            }
            else
            {
                newSettings.SerializationOptions ^= YAXSerializationOptions.DontSerializeDefaultValues;
            }

            var namingStrategy = xmlSettings.NamingStrategy.ToPascalCase();

            if (namingStrategy == "Default")
            {
                return newSettings;
            }

            Func<string, string> namingStrategyType = namingStrategy switch
            {
                "CamelCase" => name => name.ToCamelCase(),
                "KebabCase" => name => name.ToKebabCase(),
                "SnakeCase" => name => name.ToSnakeCase(),
                _ => throw new InvalidOperationException($"Invalid naming strategy: {namingStrategy}")
            };

            newSettings.TypeResolver = new CustomResolver
            {
                NamingPolicy = namingStrategyType
            };

            return newSettings;
        }

        private class CustomResolver : ITypeResolver
        {
            public IList<IYaxMemberInfo> ResolveMembers(IList<IYaxMemberInfo> sourceMembers, Type underlyingType, SerializerOptions options)
            {
                var filteredMembers = sourceMembers.Where(member =>
                    !string.Equals("Avro.Schema", member.Type.FullName, StringComparison.OrdinalIgnoreCase));

                if (NamingPolicy != null)
                {
                    filteredMembers = ApplyNamingPolicy(filteredMembers);
                }

                return filteredMembers.ToList();
            }

            public string GetTypeName(Type udtType, string proposedAlias, SerializerOptions serializerOptions)
            {
                return NamingPolicy?.Invoke(proposedAlias) ?? proposedAlias;
            }

            private IEnumerable<IYaxMemberInfo> ApplyNamingPolicy(IEnumerable<IYaxMemberInfo> filteredMembers)
            {
                var newMembers = new List<IYaxMemberInfo>();

                foreach (var filteredMember in filteredMembers)
                {
                    switch (filteredMember.MemberType)
                    {
                        case MemberTypes.Field:
                        {
                            var newFieldMember = new YaxFieldMemberWrapper((IYaxFieldInfo)filteredMember)
                            {
                                Name = NamingPolicy(filteredMember.Name)
                            };
                            newMembers.Add(newFieldMember);
                            break;
                        }
                        case MemberTypes.Property:
                        {
                            var newFieldMember = new YaxPropertyMemberWrapper((IYaxPropertyInfo)filteredMember)
                            {
                                Name = NamingPolicy(filteredMember.Name)
                            };
                            newMembers.Add(newFieldMember);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                filteredMembers = newMembers;
                return filteredMembers;
            }

            public Func<string, string> NamingPolicy { get; set; }
        }
    }
}
