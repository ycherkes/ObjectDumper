using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.Visitor.Descriptors;

namespace YellowFlavor.Serialization.Implementation.Dotnet;

internal class MemberInfoMiddleware : IObjectDescriptorMiddleware
{
    private readonly HashSet<string> _includeProperties =
    [
        "Name",
        "DeclaringType",
        "ReflectedType",
        "MemberType",
        "Attributes"
    ];

    public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
    {
        var description = prev();

        if (typeof(MemberInfo).IsAssignableFrom(objectType))
        {
            description.Properties = description.Properties.Where(p => _includeProperties.Contains(p.Name));
            description.Fields = [];
        }

        return description;
    }
}