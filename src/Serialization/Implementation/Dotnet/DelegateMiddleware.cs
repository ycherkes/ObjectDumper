using System;
using VarDump.Visitor.Descriptors;

namespace YellowFlavor.Serialization.Implementation.Dotnet;

internal class DelegateMiddleware : IObjectDescriptorMiddleware
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
    {
        if (typeof(Delegate).IsAssignableFrom(objectType))
        {
            return new ObjectDescription { Type = objectType };
        }

        return prev();
    }
}