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
            IgnoreNullValues = false;
            IgnoreIndexers = true;
            CustomTypeFormatter = new Dictionary<Type, Func<Type, string>>();
            CustomInstanceFormatters = new Dictionary<Type, Func<object, string>>();
            TrimInitialVariableName = false;
            UseTypeFullName = false;
        }

        public bool IgnoreNullValues { get; set; }

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

        public Dictionary<Type, Func<object, string>> CustomInstanceFormatters { get; }

        public bool UseTypeFullName { get; set; }
    }

}