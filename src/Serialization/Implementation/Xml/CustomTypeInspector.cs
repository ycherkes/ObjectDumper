using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using YAXLib;
using YAXLib.Options;

namespace YellowFlavor.Serialization.Implementation.Xml;


internal class CustomTypeInspector : DefaultTypeInspector
{
    public override IEnumerable<IMemberDescriptor> GetMembers(Type type, SerializerOptions options, bool includePrivateMembersFromBaseTypes)
    {
        var members = base.GetMembers(type, options, includePrivateMembersFromBaseTypes);

        var filteredMembers = members.Where(member =>
            !string.Equals("Avro.Schema", member.Type.FullName, StringComparison.OrdinalIgnoreCase));

        if (NamingPolicy != null)
        {
            filteredMembers = filteredMembers.Select(WrapAndApplyNamingPolicy);
        }

        return filteredMembers;
    }

    public override string GetTypeName(Type type, SerializerOptions options)
    {
        var typeName = base.GetTypeName(type, options);
        return NamingPolicy?.Invoke(typeName) ?? typeName;
    }

    private IMemberDescriptor WrapAndApplyNamingPolicy(IMemberDescriptor filteredMember)
    {
        return new MemberWrapper(filteredMember)
        {
            Name = NamingPolicy(filteredMember.Name),
            DateTimeZoneHandling = DateTimeZoneHandling
        };
    }

    public Func<string, string> NamingPolicy { get; set; }
    public DateTimeZoneHandling DateTimeZoneHandling { get; set; }
}