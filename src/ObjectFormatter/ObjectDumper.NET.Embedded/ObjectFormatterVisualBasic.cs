using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectFormatter.ObjectDumper.NET.Embedded
{
    internal class ObjectFormatterVisualBasic : DumperBase
    {
        private ObjectFormatterVisualBasic(DumpOptions dumpOptions) : base(dumpOptions)
        {
        }

        public static string Dump(object element, DumpOptions dumpOptions = null)
        {
            dumpOptions ??= new DumpOptions();

            using var instance = new ObjectFormatterVisualBasic(dumpOptions);
            instance.Write($"Dim {GetVariableName(element)} = ");
            instance.FormatValue(element);

            return instance.ToString();
        }

        private void CreateObject(object o)
        {
            PushAlreadyTouched(o);

            Write($"new {GetClassName(o)}");
            Write(" With {");
            LineBreak();
            Level++;

            var properties = o.GetType().GetRuntimeProperties()
                .Where(p => p.GetMethod != null && p.GetMethod.IsPublic && p.GetMethod.IsStatic == false)
                .ToList();

            if (DumpOptions.ExcludeProperties?.Any() == true)
            {
                properties = properties
                    .Where(p => !DumpOptions.ExcludeProperties.Contains(p.Name))
                    .ToList();
            }

            if (DumpOptions.ExcludeTypes?.Any() == true)
            {
                properties = properties
                    .Where(p => !DumpOptions.ExcludeTypes.Contains(p.PropertyType.FullName))
                    .ToList();
            }

            if (DumpOptions.SetPropertiesOnly)
            {
                properties = properties
                    .Where(p => p.SetMethod != null && p.SetMethod.IsPublic && p.SetMethod.IsStatic == false)
                    .ToList();
            }

            if (DumpOptions.IgnoreDefaultValues || DumpOptions.IgnoreNullValues)
            {
                properties = properties
                    .Where(p =>
                    {
                        var value = p.GetValue(o);
                        var defaultValue = p.PropertyType.GetDefault();
                        var isDefaultValue = Equals(value, defaultValue);
                        return (DumpOptions.IgnoreDefaultValues && !isDefaultValue) || (DumpOptions.IgnoreNullValues && value != null);
                    })
                    .ToList();
            }

            if (DumpOptions.PropertyOrderBy != null)
            {
                properties = properties.OrderBy(DumpOptions.PropertyOrderBy.Compile())
                    .ToList();
            }

            var last = properties.LastOrDefault();

            foreach (var property in properties)
            {
                var value = property.TryGetValue(o);

                if (AlreadyTouched(value))
                {
                    Write($".{property.Name} = ");
                    FormatValue(property.PropertyType.GetDefault());
                    if (!Equals(property, last))
                    {
                        Write(",");
                    }
                    Write(" Rem Circular reference detected");
                    LineBreak();
                    continue;
                }

                Write($".{property.Name} = ");
                FormatValue(value);
                if (!Equals(property, last))
                {
                    Write(",");
                }

                LineBreak();
            }

            Level--;
            Write("}");

            PopAlreadyTouched();
        }

        protected override void FormatValue(object o)
        {
            if (IsMaxLevel())
            {
                return;
            }

            if (o == null)
            {
                Write("Nothing");
                return;
            }

            if (o is bool)
            {
                Write($"{o.ToString().ToLower()}");
                return;
            }

            if (o is string)
            {
                var str = $@"{o}".Escape();
                Write($"\"{str}\"");
                return;
            }

            if (o is char)
            {
                var c = o.ToString().Replace("\0", "").Trim();
                Write($"\"{c}c\"");
                return;
            }

            if (o is double)
            {
                Write($"{o}d");
                return;
            }

            if (o is decimal)
            {
                Write($"{o}D");
                return;
            }

            if (o is byte or sbyte)
            {
                Write($"{o}");
                return;
            }

            if (o is float)
            {
                Write($"{o}f");
                return;
            }

            if (o is int or uint)
            {
                Write($"{o}");
                return;
            }

            if (o is long || o is ulong)
            {
                Write($"{o}L");
                return;
            }

            if (o is short or ushort)
            {
                Write($"{o}");
                return;
            }

            if (o is DateTime dateTime)
            {
                if (dateTime == DateTime.MinValue)
                {
                    Write("DateTime.MinValue");
                }
                else if (dateTime == DateTime.MaxValue)
                {
                    Write("DateTime.MaxValue");
                }
                else
                {
                    Write($"DateTime.ParseExact(\"{dateTime:O}\", \"O\", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)");
                }

                return;
            }

            if (o is DateTimeOffset dateTimeOffset)
            {
                if (dateTimeOffset == DateTimeOffset.MinValue)
                {
                    Write("DateTimeOffset.MinValue");
                }
                else if (dateTimeOffset == DateTimeOffset.MaxValue)
                {
                    Write("DateTimeOffset.MaxValue");
                }
                else
                {
                    Write($"DateTimeOffset.ParseExact(\"{dateTimeOffset:O}\", \"O\", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)");
                }

                return;
            }

            var type = o.GetType();

            if (o is Enum)
            {
                Write($"{type.FullName}.{o}");
                return;
            }

            if (o is Guid guid)
            {
                Write($"new Guid(\"{guid:D}\")");
                return;
            }

            if (DumpOptions.CustomInstanceFormatters.TryGetValue(type, out var func) ||
                (type.BaseType != typeof(object) && DumpOptions.CustomInstanceFormatters.TryGetValue(type.BaseType, out func)))

            {
                Write(func(o));
                return;
            }

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var kvpKey = type.GetRuntimeProperty(nameof(KeyValuePair<object, object>.Key)).GetValue(o, null);
                var kvpValue = type.GetRuntimeProperty(nameof(KeyValuePair<object, object>.Value)).GetValue(o, null);

                Write("{ ");
                FormatValue(kvpKey);
                Write(", ");
                FormatValue(kvpValue);
                Write(" }");
                return;
            }

            if (o is IEnumerable)
            {

                //fixme array here? from/not from
                Write($"new {GetClassName(o)}");
                //this.LineBreak();
                Write(o is Array ? " {" : " From {");

                //this.Write(" {"); //fixme
                LineBreak();
                WriteItems((IEnumerable)o);
                Write("}");
                return;
            }

            CreateObject(o);
        }

        private void WriteItems(IEnumerable items)
        {
            Level++;
            if (IsMaxLevel())
            {
                ////this.StartLine("// Omitted code");
                ////this.LineBreak();
                Level--;
                return;
            }

            var e = items.GetEnumerator();
            if (e.MoveNext())
            {
                FormatValue(e.Current);

                while (e.MoveNext())
                {
                    Write(",");
                    LineBreak();

                    FormatValue(e.Current);
                }

                LineBreak();
            }

            Level--;
        }

        private static string GetClassName(object o)
        {
            var type = o.GetType();
            var className = type.GetFormattedNameVB();
            return className;
        }

        private static string GetVariableName(object element)
        {
            if (element == null)
            {
                return "[x]";
            }

            var type = element.GetType();
            var variableName = type.Name;

            if (element is IEnumerable)
            {
                variableName = GetClassName(element)
                .Replace("(Of ", "")
                .Replace(")", "")
                .Replace(" ", "")
                .Replace(",", "")
                .Replace("(", "");


            }
            else if (type.GetTypeInfo().IsGenericType)
            {
                variableName = $"{type.Name.Substring(0, type.Name.IndexOf('`'))}";
            }

            return $"[{variableName.ToLowerFirst()}]";
        }
    }
}
