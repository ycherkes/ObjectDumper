using System;
using System.Reflection;
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
            return _wrappedFieldInfo.GetValue(obj);
        }

        public void SetValue(object obj, object value)
        {
            _wrappedFieldInfo.SetValue(obj, value);
        }
    }
}
