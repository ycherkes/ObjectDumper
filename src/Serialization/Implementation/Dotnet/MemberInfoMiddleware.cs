using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.Visitor.Descriptors;

namespace YellowFlavor.Serialization.Implementation.Dotnet;

internal class MemberInfoMiddleware : IObjectDescriptorMiddleware
{
    private static readonly HashSet<string> IncludeProperties =
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

        if (!typeof(MemberInfo).IsAssignableFrom(objectType))
        {
            return description;
        }

        description.Properties = description.Properties.Where(p => IncludeProperties.Contains(p.Name));
        description.Fields = [];

        return description;
    }
}