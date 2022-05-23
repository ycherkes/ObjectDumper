using System.Reflection;

namespace ObjectFormatter.ObjectDumper.NET.Embedded
{
    internal class PropertyAndValue
    {
        public PropertyAndValue(object source, PropertyInfo propertyInfo)
        {
            Value = propertyInfo.TryGetValue(source);
            DefaultValue = propertyInfo.PropertyType.TryGetDefault();
            Property = propertyInfo;
        }

        public PropertyInfo Property { get; }

        public object Value { get; }

        public object DefaultValue { get; }

        public bool IsDefaultValue
        {
            get
            {
                return Equals(Value, DefaultValue);
            }
        }
    }
}
