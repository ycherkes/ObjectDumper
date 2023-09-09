using System;
using System.Collections.Generic;
using System.Reflection;
using VarDumpExtended.CodeDom.Common;
using VarDumpExtended.Visitor;
using VarDumpExtended.Visitor.KnownTypes;

namespace YellowFlavor.Serialization.Implementation.Dotnet
{
    internal sealed class ServiceDescriptorKnownObject : IKnownObjectVisitor
    {
        private readonly IObjectVisitor _rootObjectVisitor;
        private readonly CodeTypeReferenceOptions _typeReferenceOptions;

        public ServiceDescriptorKnownObject(IObjectVisitor rootObjectVisitor, DumpOptions options)
        {
            _typeReferenceOptions = options.UseTypeFullName
                ? CodeTypeReferenceOptions.FullTypeName
                : CodeTypeReferenceOptions.ShortTypeName;

            _rootObjectVisitor = rootObjectVisitor;
        }

        public bool IsSuitableFor(object obj, Type objectType)
        {
            return string.Equals(objectType?.FullName, "Microsoft.Extensions.DependencyInjection.ServiceDescriptor", StringComparison.InvariantCulture);
        }

        public CodeExpression Visit(object obj, Type objectType)
        {
            var serviceType = objectType.GetProperty("ServiceType", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj) as Type;

            var typeParameters = new List<CodeTypeReference>
            {
                new(serviceType, _typeReferenceOptions)
            };

            var implementationType = objectType.GetProperty("ImplementationType", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj) as Type;

            if (implementationType != null)
            {
                typeParameters.Add(new CodeTypeReference(implementationType, _typeReferenceOptions));
            }

            var parameters = new List<CodeExpression>(1);

            var implementationInstance = objectType.GetProperty("ImplementationInstance", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj);

            if (implementationInstance != null)
            {
                parameters.Add(_rootObjectVisitor.Visit(implementationInstance));
            }

            var implementationFactory = objectType.GetProperty("ImplementationFactory", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj);

            if (implementationFactory != null)
            {
                parameters.Add(new CodeLambdaExpression(new CodeDefaultValueExpression(new CodeTypeReference(
                        implementationType ?? serviceType, _typeReferenceOptions)),
                    new CodeVariableReferenceExpression("serviceProvider")));
            }

            var lifetime = objectType.GetProperty("Lifetime", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj)?.ToString();

            return new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(new CodeTypeReference(objectType, _typeReferenceOptions)),
                    lifetime, typeParameters.ToArray()),
                parameters.ToArray());
        }

        public string Id => "ServiceDescriptor";
    }
}