using System;
using System.Reflection;
using YAXLib;

namespace YellowFlavor.Serialization.Implementation.Xml
{
    internal class YaxPropertyMemberWrapper : IYaxPropertyInfo
    {
        private readonly IYaxPropertyInfo _wrappedPropertyInfo;

        public string Name { get; set; }
        public MemberTypes MemberType { get; }
        public bool IsPublic { get; }
        public Type Type { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }

        public YaxPropertyMemberWrapper(IYaxPropertyInfo propertyInfo)
        {
            _wrappedPropertyInfo = propertyInfo;
            Name = propertyInfo.Name;
            MemberType = propertyInfo.MemberType;
            CanRead = propertyInfo.CanRead;
            CanWrite = propertyInfo.CanWrite;
            IsPublic = propertyInfo.IsPublic;
            Type = propertyInfo.Type;
        }

        public Attribute[] GetCustomAttributesByType(Type attrType, bool inherit)
        {
            return _wrappedPropertyInfo.GetCustomAttributesByType(attrType, inherit);
        }

        public Attribute[] GetCustomAttributes(bool inherit)
        {
            return _wrappedPropertyInfo.GetCustomAttributes(inherit);
        }

        public object GetValue(object obj, object[] index)
        {
            return _wrappedPropertyInfo.GetValue(obj, index);
        }

        public void SetValue(object obj, object value, object[] index)
        {
            _wrappedPropertyInfo.SetValue(obj, value, index);
        }
    }
}
