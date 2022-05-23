using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectFormatter.ObjectDumper.NET.Embedded
{
    // ReSharper disable once CheckNamespace
    public class DumpOptions
    {
        public DumpOptions()
        {
            IndentSize = 2;
            IndentChar = ' ';
            LineBreakChar = Environment.NewLine;
            SetPropertiesOnly = false;
            MaxLevel = int.MaxValue;
            ExcludeProperties = new HashSet<string>();
            PropertyOrderBy = null;
            IgnoreDefaultValues = false;
            IgnoreIndexers = true;
            CustomTypeFormatter = new Dictionary<Type, Func<Type, string>>();
            CustomInstanceFormatters = new CustomInstanceFormatters();
            TrimInitialVariableName = false;
            UseTypeFullName = false;
        }

        public int IndentSize { get; set; }

        public char IndentChar { get; set; }

        public string LineBreakChar { get; set; }

        public bool SetPropertiesOnly { get; set; }

        public int MaxLevel { get; set; }

        public ICollection<string> ExcludeProperties { get; set; }

        public IDictionary<Type, Func<Type, string>> CustomTypeFormatter { get; set; }

        public Expression<Func<PropertyInfo, object>> PropertyOrderBy { get; set; }

        /// <summary>
        /// Ignores default values if set to <c>true</c>.
        /// Default: <c>false</c>
        /// </summary>
        public bool IgnoreDefaultValues { get; set; }

        /// <summary>
        /// Ignores index properties if set to <c>true</c>.
        /// Default: <c>true</c>
        /// </summary>
        public bool IgnoreIndexers { get; set; }

        public bool TrimInitialVariableName { get; set; }

        public bool TrimTrailingColonName { get; set; }

        public CustomInstanceFormatters CustomInstanceFormatters { get; }

        public bool UseTypeFullName { get; set; }
    }

    public class CustomInstanceFormatters
    {
        private readonly Dictionary<Type, CustomInstanceFormatter> customFormatters = new Dictionary<Type, CustomInstanceFormatter>();

        public void AddFormatter<T>(Func<T, string> formatInstance)
        {
            customFormatters.Add(typeof(T), new CustomInstanceFormatter(typeof(T), o => formatInstance((T)o)));
        }

        public bool HasFormatterFor<T>()
        {
            return customFormatters.ContainsKey(typeof(T));
        }

        public bool HasFormatterFor(object obj)
        {
            return customFormatters.ContainsKey(obj.GetType());
        }

        public bool TryGetFormatter(Type type, out Func<object, string> formatter)
        {
            if (customFormatters.TryGetValue(type, out var customInstanceFormatter))
            {
                formatter = customInstanceFormatter.Formatter;
                return true;
            }

            formatter = null;
            return false;
        }

        public void Clear()
        {
            customFormatters.Clear();
        }

        public void RemoveFormatter<T>()
        {
            RemoveFormatter(typeof(T));
        }

        public void RemoveFormatter(Type type)
        {
            customFormatters.Remove(type);
        }

        private class CustomInstanceFormatter
        {
            public CustomInstanceFormatter(Type type, Func<object, string> formatter)
            {
                InstanceType = type;
                Formatter = formatter;
            }

            public Func<object, string> Formatter { get; }

            public Type InstanceType { get; }
        }
    }
}