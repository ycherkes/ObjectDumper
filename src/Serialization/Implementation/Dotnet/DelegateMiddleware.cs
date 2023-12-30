using System;
using System.Collections.Generic;
using System.Linq;
using VarDump.Visitor.Descriptors;

namespace YellowFlavor.Serialization.Implementation.Dotnet;

internal class DelegateMiddleware : IObjectDescriptorMiddleware
{
    public IEnumerable<IReflectionDescriptor> Describe(object @object, Type objectType, Func<IEnumerable<IReflectionDescriptor>> prev)
    {
        if (typeof(Delegate).IsAssignableFrom(objectType))
        {
            return Enumerable.Empty<IReflectionDescriptor>();
        }

        return prev();
    }
}