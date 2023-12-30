using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VarDump.Visitor.Descriptors;

namespace YellowFlavor.Serialization.Implementation.Dotnet;

internal class MemberInfoMiddleware : IObjectDescriptorMiddleware
{
    private readonly HashSet<string> _includeProperties = new()
    {
        "Name",
        "DeclaringType",
        "ReflectedType",
        "MemberType",
        "Attributes"
    };

    public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType, Func<IEnumerable<IReflectionDescriptor>> prev)
    {
        var members = prev();

        if (typeof(MemberInfo).IsAssignableFrom(objectType))
        {
            members = members.Where(m => _includeProperties.Contains(m.Name));
        }

        return members;
    }
}