using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ObjectFormatter
{
    /// <summary>
    ///     Source: http://stackoverflow.com/questions/852181/c-printing-all-properties-of-an-object
    /// </summary>
    internal class ObjectFormatterCSharp : DumperBase
    {
        public ObjectFormatterCSharp(DumpOptions dumpOptions) : base(dumpOptions)
        {
        }

        public static string Dump(object element, DumpOptions dumpOptions = null)
        {
            dumpOptions ??= new DumpOptions();

            var instance = new ObjectFormatterCSharp(dumpOptions);
            if (!dumpOptions.TrimInitialVariableName)
            {
                instance.Write($"var {instance.GetVariableName(element)} = ");
            }

            instance.FormatValue(element);
            if (!dumpOptions.TrimTrailingColonName)
            {
                instance.Write(";");
            }

            return instance.ToString();
        }

        private void CreateObject(object o, int intentLevel = 0)
        {
            AddAlreadyTouched(o);

            var type = o.GetType();

            var typeName = type.IsAnonymous() ? "" : type.GetFormattedName(DumpOptions.UseTypeFullName);

            Write($"new {typeName}", intentLevel);
            LineBreak();
            Write("{");
            LineBreak();
            Level++;
            DumpProperties(o);
            Level--;
            Write("}");
        }

        private void DumpProperties(object o)
        {
            var properties = o.GetType().GetRuntimeProperties()
                .Where(p => p.GetMethod != null && p.GetMethod.IsPublic && p.GetMethod.IsStatic == false)
                .ToList();

            if (DumpOptions.ExcludeProperties != null && DumpOptions.ExcludeProperties.Any())
            {
                properties = properties
                    .Where(p => !DumpOptions.ExcludeProperties.Contains(p.Name))
                    .ToList();
            }

            if (DumpOptions.SetPropertiesOnly)
            {
                properties = properties
                    .Where(p => p.SetMethod != null && p.SetMethod.IsPublic && p.SetMethod.IsStatic == false)
                    .ToList();
            }

            if (DumpOptions.PropertyOrderBy != null)
            {
                properties = properties
                    .OrderBy(DumpOptions.PropertyOrderBy.Compile())
                    .ToList();
            }

            var propertiesAndValues = properties
                .Select(p => new PropertyAndValue(o, p))
                .ToList();

            PropertyAndValue lastProperty;
            if (DumpOptions.IgnoreDefaultValues)
            {
                lastProperty = propertiesAndValues.LastOrDefault(pv => !pv.IsDefaultValue);
            }
            else
            {
                lastProperty = propertiesAndValues.LastOrDefault();
            }

            foreach (var propertiesAndValue in propertiesAndValues)
            {
                var value = propertiesAndValue.Value;

                if (AlreadyTouched(value))
                {
                    Write($"{propertiesAndValue.Property.Name} = ");
                    FormatValue(propertiesAndValue.DefaultValue);
                    if (!Equals(propertiesAndValue, lastProperty))
                    {
                        Write(",");
                    }
                    Write(" // Circular reference detected");
                    LineBreak();
                    continue;
                }

                if (DumpOptions.IgnoreDefaultValues)
                {
                    if (propertiesAndValue.IsDefaultValue)
                    {
                        continue;
                    }
                }

                var indexParameters = propertiesAndValue.Property.GetIndexParameters();
                if (indexParameters.Length > 0)
                {
                    if (!DumpOptions.IgnoreIndexers)
                    {
                        DumpIntegerArrayIndexer(o, propertiesAndValue.Property, indexParameters);
                    }
                }
                else
                {
                    Write($"{propertiesAndValue.Property.Name} = ");
                    FormatValue(value);
                    if (!Equals(propertiesAndValue, lastProperty))
                    {
                        Write(",");
                    }

                    LineBreak();
                }
            }
        }

        private void DumpIntegerArrayIndexer(object o, PropertyInfo property, ParameterInfo[] indexParameters)
        {
            if (indexParameters.Length == 1 && indexParameters[0].ParameterType == typeof(int))
            {
                // get an integer count value
                // issues, what if it's not an integer index (Dictionary?), what if it's multi-dimensional?
                // just need to be able to iterate through each value in the indexed property
                // Source: https://stackoverflow.com/questions/4268244/iterating-through-an-indexed-property-reflection

                var arrayValues = new List<object>();
                var index = 0;
                while (true)
                {
                    try
                    {
                        arrayValues.Add(property.GetValue(o, new object[] { index }));
                        index++;
                    }
                    catch (TargetInvocationException) { break; }
                }

                var lastArrayValue = arrayValues.LastOrDefault();

                for (var arrayIndex = 0; arrayIndex < arrayValues.Count; arrayIndex++)
                {
                    var arrayValue = arrayValues[arrayIndex];
                    Write($"[{arrayIndex}] = ");
                    FormatValue(arrayValue);
                    Write(!Equals(arrayValue, lastArrayValue) ? $",{DumpOptions.LineBreakChar}" : ",");
                }

                LineBreak();
            }
        }

        protected override void FormatValue(object o, int intentLevel = 0)
        {
            if (IsMaxLevel())
            {
                return;
            }

            if (o == null)
            {
                Write("null", intentLevel);
                return;
            }

            if (o is bool)
            {
                Write($"{o.ToString().ToLower()}", intentLevel);
                return;
            }

            if (o is string)
            {
                var str = $@"{o}".Escape();
                Write($"\"{str}\"", intentLevel);
                return;
            }

            if (o is char)
            {
                var c = o.ToString().Replace("\0", "").Trim();
                Write($"\'{c}\'", intentLevel);
                return;
            }

            if (o is byte || o is sbyte)
            {
                Write($"{o}", intentLevel);
                return;
            }

            if (o is short @short)
            {
                if (@short == short.MinValue)
                {
                    Write("short.MinValue", intentLevel);
                }
                else if (@short == short.MaxValue)
                {
                    Write("short.MaxValue", intentLevel);
                }
                else
                {
                    Write($"{@short.ToString(CultureInfo.InvariantCulture)}", intentLevel);
                }

                return;
            }

            if (o is ushort @ushort)
            {
                // No special handling for MinValue

                if (@ushort == ushort.MaxValue)
                {
                    Write("ushort.MaxValue", intentLevel);
                }
                else
                {
                    Write($"{@ushort.ToString(CultureInfo.InvariantCulture)}", intentLevel);
                }

                return;
            }

            if (o is int @int)
            {
                if (@int == int.MinValue)
                {
                    Write("int.MinValue", intentLevel);
                }
                else if (@int == int.MaxValue)
                {
                    Write("int.MaxValue", intentLevel);
                }
                else
                {
                    Write($"{@int.ToString(CultureInfo.InvariantCulture)}", intentLevel);
                }

                return;
            }

            if (o is uint @uint)
            {
                // No special handling for MinValue

                if (@uint == uint.MaxValue)
                {
                    Write("uint.MaxValue", intentLevel);
                }
                else
                {
                    Write($"{@uint.ToString(CultureInfo.InvariantCulture)}u", intentLevel);
                }

                return;
            }

            if (o is long @long)
            {
                if (@long == long.MinValue)
                {
                    Write("long.MinValue", intentLevel);
                }
                else if (@long == long.MaxValue)
                {
                    Write("long.MaxValue", intentLevel);
                }
                else
                {
                    Write($"{@long.ToString(CultureInfo.InvariantCulture)}L", intentLevel);
                }

                return;
            }

            if (o is ulong @ulong)
            {
                // No special handling for MinValue

                if (@ulong == ulong.MaxValue)
                {
                    Write("ulong.MaxValue", intentLevel);
                }
                else
                {
                    Write($"{@ulong.ToString(CultureInfo.InvariantCulture)}UL", intentLevel);
                }

                return;
            }

            if (o is double @double)
            {
                if (@double == double.MinValue)
                {
                    Write("double.MinValue", intentLevel);
                }
                else if (@double == double.MaxValue)
                {
                    Write("double.MaxValue", intentLevel);
                }
                else if (double.IsNaN(@double))
                {
                    Write("double.NaN", intentLevel);
                }
                else if (double.IsPositiveInfinity(@double))
                {
                    Write("double.PositiveInfinity", intentLevel);
                }
                else if (double.IsNegativeInfinity(@double))
                {
                    Write("double.NegativeInfinity", intentLevel);
                }
                else
                {
                    Write($"{@double.ToString(CultureInfo.InvariantCulture)}d", intentLevel);
                }

                return;
            }

            if (o is decimal @decimal)
            {
                if (@decimal == decimal.MinValue)
                {
                    Write("decimal.MinValue", intentLevel);
                }
                else if (@decimal == decimal.MaxValue)
                {
                    Write("decimal.MaxValue", intentLevel);
                }
                else
                {
                    Write($"{@decimal.ToString(CultureInfo.InvariantCulture)}m", intentLevel);
                }

                return;
            }

            if (o is float @float)
            {
                if (@float == float.MinValue)
                {
                    Write("float.MinValue", intentLevel);
                }
                else if (@float == float.MaxValue)
                {
                    Write("float.MaxValue", intentLevel);
                }
                else
                {
                    Write($"{@float.ToString(CultureInfo.InvariantCulture)}f", intentLevel);
                }

                return;
            }

            if (o is DateTime dateTime)
            {
                if (dateTime == DateTime.MinValue)
                {
                    Write("DateTime.MinValue", intentLevel);
                }
                else if (dateTime == DateTime.MaxValue)
                {
                    Write("DateTime.MaxValue", intentLevel);
                }
                else
                {
                    Write($"DateTime.ParseExact(\"{dateTime:O}\", \"O\", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)", intentLevel);
                }

                return;
            }

            if (o is DateTimeOffset dateTimeOffset)
            {
                if (dateTimeOffset == DateTimeOffset.MinValue)
                {
                    Write("DateTimeOffset.MinValue", intentLevel);
                }
                else if (dateTimeOffset == DateTimeOffset.MaxValue)
                {
                    Write("DateTimeOffset.MaxValue", intentLevel);
                }
                else
                {
                    Write($"DateTimeOffset.ParseExact(\"{dateTimeOffset:O}\", \"O\", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)", intentLevel);
                }

                return;
            }

            if (o is TimeSpan timeSpan)
            {
                if (timeSpan == TimeSpan.Zero)
                {
                    Write("TimeSpan.Zero", intentLevel);
                }
                else if (timeSpan == TimeSpan.MinValue)
                {
                    Write("TimeSpan.MinValue", intentLevel);
                }
                else if (timeSpan == TimeSpan.MaxValue)
                {
                    Write("TimeSpan.MaxValue", intentLevel);
                }
                else
                {
                    Write($"TimeSpan.ParseExact(\"{timeSpan:c}\", \"c\", CultureInfo.InvariantCulture, TimeSpanStyles.None)", intentLevel);
                }

                return;
            }

            if (o is CultureInfo cultureInfo)
            {
                Write($"new CultureInfo(\"{cultureInfo}\")", intentLevel);
                return;
            }

            var type = o.GetType();

            if (o is Enum)
            {
                var enumTypeName = type.GetFormattedName(DumpOptions.UseTypeFullName);
                var enumFlags = $"{o}".Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var enumValues = string.Join(" | ", enumFlags.Select(f => $"{enumTypeName}.{f.Replace(" ", "")}"));
                Write($"{enumValues}", intentLevel);
                return;
            }

            if (o is Guid guid)
            {
                Write($"new Guid(\"{guid:D}\")", intentLevel);
                return;
            }

            if (DumpOptions.CustomInstanceFormatters.TryGetFormatter(type, out var func))
            {
                Write(func(o));
                return;
            }

            if (o is Type systemType)
            {
                if (DumpOptions.CustomTypeFormatter.TryGetValue(systemType, out var formatter) ||
                    DumpOptions.CustomTypeFormatter.TryGetValue(typeof(Type), out formatter))
                {
                    Write(formatter(systemType));
                    return;
                }

                Write($"typeof({systemType.GetFormattedName(DumpOptions.UseTypeFullName)})", intentLevel);
                return;
            }
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var kvpKey = type.GetRuntimeProperty(nameof(KeyValuePair<object, object>.Key)).GetValue(o, null);
                var kvpValue = type.GetRuntimeProperty(nameof(KeyValuePair<object, object>.Value)).GetValue(o, null);

                Write("{ ", intentLevel);
                FormatValue(kvpKey);
                Write(", ");
                FormatValue(kvpValue);
                Write(" }");
                return;
            }

#if NETSTANDARD2_0_OR_GREATER
            if (type.IsValueTuple())
            {
                WriteValueTuple(o, type);
                return;
            }
#endif

            if (o is IEnumerable enumerable)
            {
                var typeName = type.GetFormattedName(DumpOptions.UseTypeFullName);
                Write($"new {typeName}", intentLevel);
                LineBreak();
                Write("{");
                LineBreak();
                WriteItems(enumerable);
                Write("}");
                return;
            }

            CreateObject(o, intentLevel);
        }

#if NETSTANDARD2_0_OR_GREATER
        private void WriteValueTuple(object o, Type type)
        {
            var fields = type.GetFields().ToList();
            if (fields.Any())
            {
                var last = fields.LastOrDefault();

                Write("(");
                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(o);
                    FormatValue(fieldValue);
                    if (!Equals(field, last))
                    {
                        Write(", ");
                    }
                }
                Write(")");
            }
            else
            {
                Write("ValueTuple.Create()");
            }
        }
#endif

        private void WriteItems(IEnumerable items)
        {
            Level++;
            if (IsMaxLevel())
            {
                Level--;
                return;
            }

            var e = items.GetEnumerator();
            if (e.MoveNext())
            {
                FormatValue(e.Current, Level);

                while (e.MoveNext())
                {
                    Write(",");
                    LineBreak();

                    FormatValue(e.Current, Level);
                }

                LineBreak();
            }

            Level--;
        }

        private string GetVariableName(object element)
        {
            if (element == null)
            {
                return "x";
            }

            var type = element.GetType();
            if (type.IsAnonymous())
            {
                return "x";
            }

            var className = type.GetFormattedName(useFullName: false, useValueTupleFormatting: false);
            string variableName;

            var splitGenerics = className.Split('<');

            if (splitGenerics.Length > 2 || className.Contains(','))
            {
                // Complex generics and multi-dimensional arrays
                // are using simple variable names
                variableName = splitGenerics[0];
            }
            else
            {
                // Simple generics, nullable types and one-dimensional arrays
                // are using more sophisticated variable names
                variableName = className
                    .Replace("Nullable<", "OfNullable")
                    .Replace("<", "Of")
                    .Replace("<", "Of")
                    .Replace(">", "s")
                    .Replace(" ", "")
                    .Replace("[", "Array")
                    .Replace("]", "")
                    ;
            }

            if (TypeExtensions.IsKeyword(variableName))
            {
                variableName += "Value";
            }

            return variableName.ToLowerFirst();
        }
    }
}
