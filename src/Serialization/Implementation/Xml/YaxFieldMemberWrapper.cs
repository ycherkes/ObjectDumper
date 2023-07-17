using System;
using System.Reflection;
using Newtonsoft.Json;
using YAXLib;

namespace YellowFlavor.Serialization.Implementation.Xml
{
    internal class YaxFieldMemberWrapper : IYaxFieldInfo
    {
        private readonly IYaxFieldInfo _wrappedFieldInfo;

        public string Name { get; set; }
        public MemberTypes MemberType { get; }
        public bool IsPublic { get; }
        public Type Type { get; }
        public DateTimeZoneHandling DateTimeZoneHandling { get; set; }

        public YaxFieldMemberWrapper(IYaxFieldInfo fieldInfo)
        {
            _wrappedFieldInfo = fieldInfo;
            Name = fieldInfo.Name;
            MemberType = fieldInfo.MemberType;
            IsPublic = fieldInfo.IsPublic;
            Type = fieldInfo.Type;
        }

        public Attribute[] GetCustomAttributes(Type attrType, bool inherit)
        {
            return _wrappedFieldInfo.GetCustomAttributes(attrType, inherit);
        }

        public Attribute[] GetCustomAttributes(bool inherit)
        {
            return _wrappedFieldInfo.GetCustomAttributes(inherit);
        }

        public object GetValue(object obj)
        {
            var value = _wrappedFieldInfo.GetValue(obj);
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

        public void SetValue(object obj, object value)
        {
            _wrappedFieldInfo.SetValue(obj, value);
        }
    }
}
