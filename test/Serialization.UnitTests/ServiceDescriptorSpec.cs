using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serialization.UnitTests.TestModel;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class KnownTypesSpec
    {
        [Fact]
        public void DumpServiceDescriptorSpecCsharp()
        {
            var serviceCollection = new ServiceCollection
            {
                ServiceDescriptor.Transient<IPerson>(serviceProvider => new Person()), // It's not possible to reconstruct the expression by existing Func
                ServiceDescriptor.Singleton<IPerson, Person>(),
                ServiceDescriptor.Scoped<IPerson, Person>()
            };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(serviceCollection, null);

            Assert.Equal("""
                var serviceCollectionOfServiceDescriptor = new ServiceCollection
                {
                    ServiceDescriptor.Transient<IPerson>(serviceProvider => default(IPerson)),
                    ServiceDescriptor.Singleton<IPerson, Person>(),
                    ServiceDescriptor.Scoped<IPerson, Person>()
                };

                """, result);
        }


        [Fact]
        public void DumpServiceDescriptorSpecVb()
        {
            var personServiceDescriptor = ServiceDescriptor.Transient<IPerson, Person>();

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(personServiceDescriptor, null);

            Assert.Equal(
                @"Dim serviceDescriptorValue = ServiceDescriptor.Transient(Of IPerson, Person)()
", result);
        }
    }
}