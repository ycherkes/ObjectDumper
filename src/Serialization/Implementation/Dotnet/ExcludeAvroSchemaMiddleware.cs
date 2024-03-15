using System;
using System.Linq;
using VarDump.Visitor.Descriptors;

namespace YellowFlavor.Serialization.Implementation.Dotnet
{
    internal class ExcludeAvroSchemaMiddleware : IObjectDescriptorMiddleware
    {
        public IObjectDescription GetObjectDescription(object @object, Type objectType, Func<IObjectDescription> prev)
        {
            var objectDescription = prev();

            return new ObjectDescription
            {
                Type = objectDescription.Type,
                ConstructorArguments = objectDescription.ConstructorArguments,
                Properties = objectDescription.Properties.Where(FilterByTypeName),
                Fields = objectDescription.Fields.Where(FilterByTypeName)
            };
        }

        private static bool FilterByTypeName<T>(T member) where T : ReflectionDescription
        {
            return !string.Equals(member.Type.FullName, "Avro.Schema", StringComparison.InvariantCulture);
        }
    }
}
