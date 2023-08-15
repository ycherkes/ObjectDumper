using Newtonsoft.Json;
using System;
using System.Reflection;
using YAXLib;

namespace YellowFlavor.Serialization.Implementation.Xml
{
    internal class MemberWrapper : IMemberDescriptor
    {
        private readonly IMemberDescriptor _wrappedMember;

        public string Name { get; set; }
        public MemberTypes MemberType { get; }
        public bool IsPublic { get; }
        public Type Type { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }
        public DateTimeZoneHandling DateTimeZoneHandling { get; set; }

        public MemberWrapper(IMemberDescriptor member)
        {
            _wrappedMember = member;
            Name = member.Name;
            MemberType = member.MemberType;
            CanRead = member.CanRead;
            CanWrite = member.CanWrite;
            IsPublic = member.IsPublic;
            Type = member.Type;
        }

       public Attribute[] GetCustomAttributes()
        {
            return _wrappedMember.GetCustomAttributes();
        }

        public object GetValue(object obj, object[] index)
        {
            var value = _wrappedMember.GetValue(obj);
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
            _wrappedMember.SetValue(obj, value);
        }
    }
}
