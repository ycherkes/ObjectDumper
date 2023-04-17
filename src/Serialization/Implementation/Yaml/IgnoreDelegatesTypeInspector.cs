using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace YellowFlavor.Serialization.Implementation.Yaml
{
    internal class IgnoreDelegatesTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _innerTypeDescriptor;

        public IgnoreDelegatesTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            _innerTypeDescriptor = innerTypeDescriptor;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            var props = _innerTypeDescriptor.GetProperties(type, container);
            props = props.Where(p => !typeof(Delegate).IsAssignableFrom(p.Type));
            return props;
        }
    }
}
