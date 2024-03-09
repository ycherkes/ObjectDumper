using System;
using System.Collections.Generic;
using System.Reflection;
using VarDump.CodeDom.Common;
using VarDump.CodeDom.Compiler;
using VarDump.Visitor;
using VarDump.Visitor.KnownTypes;

namespace YellowFlavor.Serialization.Implementation.Dotnet;

internal sealed class ServiceDescriptorKnownObject(IRootObjectVisitor rootObjectVisitor, ICodeWriter codeWriter)
    : IKnownObjectVisitor
{
    public bool IsSuitableFor(object obj, Type objectType)
    {
        return string.Equals(objectType?.FullName, "Microsoft.Extensions.DependencyInjection.ServiceDescriptor", StringComparison.InvariantCulture);
    }

    public void Visit(object obj, Type objectType, VisitContext context)
    {
        var serviceType = objectType.GetProperty("ServiceType", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj) as Type;

        var typeParameters = new List<CodeTypeInfo>
        {
            new(serviceType)
        };

        var implementationType = objectType.GetProperty("ImplementationType", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj) as Type;

        if (implementationType != null)
        {
            typeParameters.Add(implementationType);
        }

        var parameters = new List<Action>(1);

        var implementationInstance = objectType.GetProperty("ImplementationInstance", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj);

        if (implementationInstance != null)
        {
            parameters.Add(() => rootObjectVisitor.Visit(implementationInstance, context));
        }

        var implementationFactory = objectType.GetProperty("ImplementationFactory", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj);

        if (implementationFactory != null)
        {
            var typeInfo = new CodeTypeInfo(implementationType ?? serviceType);

            parameters.Add(() => codeWriter.WriteLambdaExpression(() => codeWriter.WriteDefaultValue(typeInfo), [() => codeWriter.WriteVariableReference("serviceProvider")]));
        }

        var lifetime = objectType.GetProperty("Lifetime", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj)?.ToString();

        codeWriter.WriteMethodInvoke(() =>
            codeWriter.WriteMethodReference(
                () => codeWriter.WriteType(objectType),
                lifetime, typeParameters.ToArray()
            ), parameters);
    }

    public string Id => "ServiceDescriptor";
}