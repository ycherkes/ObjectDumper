using System;
using System.Reflection;
using Newtonsoft.Json;
using YAXLib;

namespace YellowFlavor.Serialization.Implementation.Xml
{
    internal class PropertyMemberWrapper : IPropertyInfo
    {
        private readonly IPropertyInfo _wrappedPropertyInfo;

        public string Name { get; set; }
        public MemberTypes MemberType { get; }
        public bool IsPublic { get; }
        public Type Type { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }
        public DateTimeZoneHandling DateTimeZoneHandling { get; set; }

        public PropertyMemberWrapper(IPropertyInfo propertyInfo)
        {
            _wrappedPropertyInfo = propertyInfo;
            Name = propertyInfo.Name;
            MemberType = propertyInfo.MemberType;
            CanRead = propertyInfo.CanRead;
            CanWrite = propertyInfo.CanWrite;
            IsPublic = propertyInfo.IsPublic;
            Type = propertyInfo.Type;
        }

        public Attribute[] GetCustomAttributes(Type attrType, bool inherit)
        {
            return _wrappedPropertyInfo.GetCustomAttributes(attrType, inherit);
        }

        public Attribute[] GetCustomAttributes(bool inherit)
        {
            return _wrappedPropertyInfo.GetCustomAttributes(inherit);
        }

        public object GetValue(object obj, object[] index)
        {
            var value = _wrappedPropertyInfo.GetValue(obj, index);
            if (Type != typeof(DateTime) && Type != typeof(DateTime?))
            {
                return value;
            }

            if (Type == typeof(DateTime?) && value == null)
            {
                return null;
            }

            value = JsonConvert.ToString((DateTime)value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling).Trim('"');

            return value;
        }

        public void SetValue(object obj, object value, object[] index)
        {
            _wrappedPropertyInfo.SetValue(obj, value, index);
        }
    }
}
