using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ObjectFormatter.CodeDom.Embedded.ms.CodeDom.System.CodeDom;
using ObjectFormatter.CodeDom.Embedded.ms.Common.src.Sys.CodeDom;

namespace ObjectFormatter.CodeDom.Embedded
{
    internal class ObjectVisitor
    {
        private readonly int _maxDepth;
        private readonly bool _ignoreDefaultValues;
        private readonly bool _ignoreNullValues;
        private readonly CodeTypeReferenceOptions _typeReferenceOptions;
        private readonly Stack<object> _visitedObjects;
        private int _depth;
        private readonly ICollection<string> _excludeTypes;

        public ObjectVisitor(VisitorOptions visitorOptions)
        {
            _maxDepth = visitorOptions.MaxDepth;
            _ignoreDefaultValues = visitorOptions.IgnoreDefaultValues;
            _ignoreNullValues = visitorOptions.IgnoreNullValues;
            _typeReferenceOptions = visitorOptions.UseTypeFullName ? CodeTypeReferenceOptions.FullTypeName : CodeTypeReferenceOptions.ShortTypeName;
            _excludeTypes = visitorOptions.ExcludeTypes ?? new List<string>();

            _visitedObjects = new Stack<object>();
        }

        public CodeExpression Visit(object @object)
        {
            if (IsMaxDepth())
            {
                return new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Max depth") { IsBlock = true }));
            }

            try
            {
                _depth++;

                if (@object == null || IsPrimitive(@object))
                {
                    return new CodePrimitiveExpression(@object);
                }

                if (@object is TimeSpan timeSpan)
                {
                    return VisitTimeSpan(timeSpan);
                }

                if (@object is DateTime dateTime)
                {
                    return VisitDateTime(dateTime);
                }

                if (@object is DateTimeOffset dateTimeOffset)
                {
                    return VisitDateTimeOffset(dateTimeOffset);
                }

                if (IsKeyValuePair(@object))
                {
                    return VisitKeyValuePair(@object);
                }

                if (IsTuple(@object))
                {
                    return VisitTuple(@object);
                }

                if (IsValueTuple(@object))
                {
                    return VisitValueTuple(@object);
                }

                if (@object is Enum)
                {
                    return VisitEnum(@object);
                }

                if (@object is Guid guid)
                {
                    return VisitGuid(guid);
                }

                if (@object is CultureInfo cultureInfo)
                {
                    return VisitCultureInfo(cultureInfo);
                }

                if (@object is Type type)
                {
                    return VisitType(type);
                }

                if (@object is IDictionary dict)
                {
                    return VisitDictionary(dict);
                }

                if (@object is IEnumerable enumerable)
                {
                    return VisitCollection(enumerable);
                }

                return VisitObject(@object);
            }
            finally
            {
                _depth--;
            }
        }

        private CodeExpression VisitType(Type type)
        {
            return new CodeTypeOfExpression(new CodeTypeReference(type, _typeReferenceOptions));
        }

        private CodeExpression VisitGuid(Guid guid)
        {
            return new CodeObjectCreateExpression(new CodeTypeReference(typeof(Guid), _typeReferenceOptions), new CodePrimitiveExpression(guid.ToString("D")));
        }

        private CodeExpression VisitCultureInfo(CultureInfo cultureInfo)
        {
            return new CodeObjectCreateExpression(new CodeTypeReference(typeof(CultureInfo), _typeReferenceOptions), new CodePrimitiveExpression(cultureInfo.ToString()));
        }

        private CodeExpression VisitEnum(object o)
        {
            return new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(o.GetType(), _typeReferenceOptions)), o.ToString());
        }

        private CodeExpression VisitKeyValuePair(object o)
        {
            var objectType = o.GetType();
            var propertyValues = objectType.GetProperties().Select(p => p.GetValue(o)).Select(Visit);
            return new CodeObjectCreateExpression(new CodeTypeReference(objectType, _typeReferenceOptions), propertyValues.ToArray());
        }

        private CodeExpression VisitTuple(object o)
        {
            if (IsVisited(o))
                return new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { IsBlock = true }));
            PushVisited(o);
            var objectType = o.GetType();
            var propertyValues = objectType.GetProperties().Select(p => p.GetValue(o)).Select(Visit);
            var result = new CodeObjectCreateExpression(new CodeTypeReference(objectType, _typeReferenceOptions), propertyValues.ToArray());
            PopVisited();
            return result;
        }

        private CodeExpression VisitKeyValuePairGenerateTuple(object o)
        {
            var objectType = o.GetType();
            var propertyValues = objectType.GetProperties().Select(p => p.GetValue(o)).Select(Visit);
            return new CodeValueTupleCreateExpression(propertyValues.ToArray());
        }

        private CodeExpression VisitKeyValuePairGenerateImplicitly(object o)
        {
            var objectType = o.GetType();
            var propertyValues = objectType.GetProperties().Select(p => p.GetValue(o)).Select(Visit).Take(2).ToArray();
            return new CodeImplicitKeyValuePairCreateExpression(propertyValues.First(), propertyValues.Last());
        }

        private CodeExpression VisitObject(object o)
        {
            if (IsVisited(o))
                return new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { IsBlock = true }));

            PushVisited(o);
            var objectType = o.GetType();

            var result = new CodeObjectCreateAndInitializeExpression(IsAnonymousType(o.GetType()) ? new CodeAnonymousTypeReference() : new CodeTypeReference(objectType, _typeReferenceOptions))
            {
                InitializeExpressions = new CodeExpressionCollection(objectType.GetRuntimeProperties()
                .Select(p => new { PropertyName = p.Name, Value = p.GetValue(o), PropertyType = p.PropertyType })
                .Where(pv => !_excludeTypes.Contains(pv.PropertyType.FullName) &&
                (!_ignoreNullValues || _ignoreNullValues && pv.Value != null) &&
                (!_ignoreDefaultValues || !pv.PropertyType.IsValueType || _ignoreDefaultValues && !IsDefault(pv.PropertyType, pv.Value)))
                .Select(pv => (CodeExpression)new CodeAssignExpression(new CodePropertyReferenceExpression(null, pv.PropertyName), Visit(pv.Value)))
                .ToArray())
            };

            PopVisited();

            return result;
        }


        private static bool IsDefault(Type t, object value)
        {
            var defaultValue = typeof(ObjectVisitor).GetMethod(nameof(IsDefaultGeneric), BindingFlags.Static | BindingFlags.NonPublic)?.MakeGenericMethod(t).Invoke(null, new[]{ value });
            return (bool)defaultValue;
        }

        //private static bool IsNullable(Type t)
        //{
        //    var defaultValue = typeof(ObjectVisitor).GetMethod(nameof(IsNullableGeneric), BindingFlags.Static | BindingFlags.NonPublic)?.MakeGenericMethod(t).Invoke(null, null);
        //    return (bool)defaultValue;
        //}

        //private static bool IsNullableGeneric<T>()
        //{
        //    return default(T) == null;
        //}

        private static bool IsDefaultGeneric<T>(T obj)
        {
            return Equals(default(T), obj);
        }

        
        private static bool IsTuple(object o)
        {
            var typeFullName = o.GetType().FullName ?? "";

            return typeFullName.StartsWith("System.Tuple");
        }

        private CodeExpression VisitValueTuple(object o)
        {
            var objectType = o.GetType();
            var propertyValues = objectType.GetFields().Select(p => p.GetValue(o)).Select(Visit);

            return new CodeValueTupleCreateExpression(propertyValues.ToArray());
        }

        private static bool IsKeyValuePair(object o)
        {
            var type = o.GetType();
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
            }
            return false;
        }

        private static bool IsValueTuple(object o)
        {
            var objectType = o.GetType();
            var typeFullName = objectType.FullName ?? "";

            return objectType.IsValueType && typeFullName.StartsWith("System.ValueTuple");
        }

        private CodeExpression VisitDictionary(IDictionary dict)
        {
            if (IsVisited(dict))
                return new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { IsBlock = true }));
            PushVisited(dict);
            var valuesType = dict.Values.GetType();
            var keysType = dict.Keys.GetType();
            var result = ContainsAnonymousType(keysType) || ContainsAnonymousType(valuesType) ? VisitAnonymousDictionary(dict) : VisitSimpleDictionary(dict);
            PopVisited();
            return result;
        }

        private CodeExpression VisitSimpleDictionary(IDictionary dict)
        {
            var items = dict.Cast<object>().Select(VisitKeyValuePairGenerateImplicitly);

            var type = dict.GetType();
            var typeFullName = type.FullName ?? "";
            var isImmutable = typeFullName.StartsWith("System.Collections.Immutable");



            if (isImmutable)
            {
                var keyType = GetInnerElementType(dict.Keys.GetType());
                var valueType = GetInnerElementType(dict.Values.GetType());

                var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

                CodeExpression dictionaryCreateExpression = new CodeObjectCreateAndInitializeExpression(
                 new CodeTypeReference(dictionaryType, _typeReferenceOptions),
                 items);

                dictionaryCreateExpression = new CodeMethodInvokeExpression(dictionaryCreateExpression, $"To{type.Name.Split('`')[0]}");

                return dictionaryCreateExpression;
            }
            else
            {
                CodeExpression dictionaryCreateExpression = new CodeObjectCreateAndInitializeExpression(
                new CodeTypeReference(type, _typeReferenceOptions), items);
                return dictionaryCreateExpression;
            }
        }

        private CodeExpression VisitAnonymousDictionary(IEnumerable dict)
        {
            var items = dict.Cast<object>().Select(VisitKeyValuePairGenerateTuple);
            var type = dict.GetType();

            var typeFullName = type.FullName ?? "";

            var isImmutable = typeFullName.StartsWith("System.Collections.Immutable");


            CodeExpression expr = new CodeArrayCreateExpression(
            new CodeAnonymousTypeReference(),
            items.ToArray());

            var variableReferenceExpression = new CodeVariableReferenceExpression("kvp");
            var keyLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Item1"), variableReferenceExpression);
            var valueLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Item2"), variableReferenceExpression);

            expr = isImmutable
                ? new CodeMethodInvokeExpression(expr, $"To{type.Name.Split('`')[0]}", keyLambdaExpression, valueLambdaExpression)
                : new CodeMethodInvokeExpression(expr, "ToDictionary", keyLambdaExpression, valueLambdaExpression);

            return expr;
        }

        private CodeExpression VisitCollection(IEnumerable enumerable)
        {
            if (IsVisited(enumerable))
                return new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { IsBlock = true }));

            PushVisited(enumerable);
            var collectionType = enumerable.GetType();

            var elementType = GetInnerElementType(collectionType);
            var result = ContainsAnonymousType(collectionType) ? VisitAnonymousCollection(enumerable) : VisitSimpleCollection(enumerable, elementType);

            PopVisited();

            return result;
        }

        private CodeExpression VisitSimpleCollection(IEnumerable enumerable, Type elementType)
        {
            var items = enumerable.Cast<object>().Select(Visit);

            var type = enumerable.GetType();

            var typeFullName = type.FullName ?? "";

            var isImmutable = typeFullName.StartsWith("System.Collections.Immutable");

            if (!IsCollection(enumerable) || type.IsArray || isImmutable)
            {
                CodeExpression expr = new CodeArrayCreateExpression(
                new CodeTypeReference(elementType, _typeReferenceOptions),
                items.ToArray());

                if (isImmutable)
                {
                    expr = new CodeMethodInvokeExpression(expr, $"To{type.Name.Split('`')[0]}");
                }

                return expr;
            }

            if (typeFullName.StartsWith("System.Collections.ObjectModel.ReadOnlyCollection"))
            {
                var expression = new CodeObjectCreateExpression(
                new CodeTypeReference(type, _typeReferenceOptions),
                new CodeArrayCreateExpression(
                new CodeTypeReference(elementType, _typeReferenceOptions),
                items.ToArray()));

                return expression;

            }

            var initializeExpression = new CodeObjectCreateAndInitializeExpression(
                new CodeTypeReference(type, _typeReferenceOptions),
                items.ToArray()
            );

            return initializeExpression;
        }

        private CodeExpression VisitAnonymousCollection(IEnumerable enumerable)
        {
            var items = enumerable.Cast<object>().Select(Visit);
            var type = enumerable.GetType();

            var typeFullName = type.FullName ?? "";

            var isImmutable = typeFullName.StartsWith("System.Collections.Immutable");


            CodeExpression expr = new CodeArrayCreateExpression(
            new CodeAnonymousTypeReference(),
            items.ToArray());

            if (isImmutable || !type.IsArray)
            {
                expr = new CodeMethodInvokeExpression(expr, $"To{type.Name.Split('`')[0]}");
            }

            return expr;
        }

        private CodeExpression VisitDateTimeOffset(DateTimeOffset dateTimeOffset)
        {
            if (dateTimeOffset == DateTimeOffset.MaxValue)
            {
                return new CodeFieldReferenceExpression
                (
                    new CodeTypeReferenceExpression(new CodeTypeReference(typeof(DateTimeOffset), _typeReferenceOptions)),
                    nameof(DateTimeOffset.MaxValue)
                );
            }

            if (dateTimeOffset == DateTimeOffset.MinValue)
            {
                return new CodeFieldReferenceExpression
                (
                    new CodeTypeReferenceExpression(new CodeTypeReference(typeof(DateTimeOffset), _typeReferenceOptions)),
                    nameof(DateTimeOffset.MinValue)
                );
            }

            var year = new CodePrimitiveExpression(dateTimeOffset.Year);
            var month = new CodePrimitiveExpression(dateTimeOffset.Month);
            var day = new CodePrimitiveExpression(dateTimeOffset.Day);
            var hour = new CodePrimitiveExpression(dateTimeOffset.Hour);
            var minute = new CodePrimitiveExpression(dateTimeOffset.Minute);
            var second = new CodePrimitiveExpression(dateTimeOffset.Second);
            var millisecond = new CodePrimitiveExpression(dateTimeOffset.Millisecond);

            var offsetExpression = VisitTimeSpan(dateTimeOffset.Offset);

            return new CodeObjectCreateExpression(new CodeTypeReference(typeof(DateTimeOffset), _typeReferenceOptions), year, month, day, hour, minute, second, millisecond, offsetExpression);
        }

        private CodeExpression VisitTimeSpan(TimeSpan timeSpan)
        {
            var specialValuesDictionary = new Dictionary<TimeSpan, string>
            {
                { TimeSpan.MaxValue, nameof(TimeSpan.MaxValue) },
                { TimeSpan.MinValue, nameof(TimeSpan.MinValue) },
                { TimeSpan.Zero, nameof(TimeSpan.Zero) }
            };

            if (specialValuesDictionary.TryGetValue(timeSpan, out var name))
            {
                return new CodeFieldReferenceExpression
                (
                    new CodeTypeReferenceExpression(new CodeTypeReference(typeof(TimeSpan), _typeReferenceOptions)),
                    name
                );
            }

            var valuesCollection = new Dictionary<string, long>
            {
                {nameof(TimeSpan.FromDays), timeSpan.Days },
                {nameof(TimeSpan.FromHours), timeSpan.Hours },
                {nameof(TimeSpan.FromMinutes), timeSpan.Minutes},
                {nameof(TimeSpan.FromSeconds), timeSpan.Seconds },
                {nameof(TimeSpan.FromMilliseconds),  timeSpan.Milliseconds }
            };

            var nonZeroValues = valuesCollection.Where(v => v.Value > 0).ToArray();

            if (nonZeroValues.Length == 1)
            {
                return new CodeMethodInvokeExpression
                (
                    new CodeMethodReferenceExpression(
                        new CodeTypeReferenceExpression(new CodeTypeReference(typeof(TimeSpan), _typeReferenceOptions)),
                        nonZeroValues[0].Key),
                    new CodePrimitiveExpression(nonZeroValues[0].Value)
                );
            }

            if (timeSpan.TotalMilliseconds % TimeSpan.TicksPerMillisecond == 0)
            {
                var days = new CodePrimitiveExpression(timeSpan.Days);
                var hours = new CodePrimitiveExpression(timeSpan.Hours);
                var minutes = new CodePrimitiveExpression(timeSpan.Minutes);
                var seconds = new CodePrimitiveExpression(timeSpan.Seconds);
                var milliseconds = new CodePrimitiveExpression(timeSpan.Milliseconds);

                if (timeSpan.Days == 0 && timeSpan.Milliseconds == 0)
                {
                    return new CodeObjectCreateExpression(new CodeTypeReference(typeof(TimeSpan), _typeReferenceOptions), hours, minutes, seconds);
                }

                if (timeSpan.Milliseconds == 0)
                {
                    return new CodeObjectCreateExpression(new CodeTypeReference(typeof(TimeSpan), _typeReferenceOptions), days, hours, minutes, seconds);
                }

                return new CodeObjectCreateExpression(new CodeTypeReference(typeof(TimeSpan), _typeReferenceOptions), days, hours, minutes, seconds, milliseconds);
            }

            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(new CodeTypeReference(typeof(TimeSpan), _typeReferenceOptions)),
                    nameof(TimeSpan.FromTicks)),
                new CodePrimitiveExpression(timeSpan.Ticks)
            );
        }

        private static bool IsCollection(object obj)
        {
            return obj is ICollection || obj.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>)) != null;
        }

        private static Type GetInnerElementType(Type type)
        {
            var elementType = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))?.GenericTypeArguments.FirstOrDefault() ?? typeof(object);

            return elementType;
        }

        private static bool IsAnonymousType(Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
            var nameContainsAnonymousType = (type.FullName ?? "").Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        private static bool ContainsAnonymousType(Type type)
        {
            var elementType = GetInnerElementType(type);

            while (elementType != null && elementType != typeof(object) && type != typeof(string))
            {
                type = elementType;
                elementType = GetInnerElementType(type);
            }

            return IsAnonymousType(type);
        }

        private static bool IsPrimitive(object @object)
        {
            return @object is char
                    || @object is sbyte
                    || @object is ushort
                    || @object is uint
                    || @object is ulong
                    || @object is string
                    || @object is byte
                    || @object is short
                    || @object is int
                    || @object is long
                    || @object is float
                    || @object is double
                    || @object is decimal
                    || @object is bool;
        }

        private CodeExpression VisitDateTime(DateTime dateTime)
        {
            var year = new CodePrimitiveExpression(dateTime.Year);
            var month = new CodePrimitiveExpression(dateTime.Month);
            var day = new CodePrimitiveExpression(dateTime.Day);
            var hour = new CodePrimitiveExpression(dateTime.Hour);
            var minute = new CodePrimitiveExpression(dateTime.Minute);
            var second = new CodePrimitiveExpression(dateTime.Second);
            var millisecond = new CodePrimitiveExpression(dateTime.Millisecond);


            var kind = new CodeFieldReferenceExpression
                (
                    new CodeTypeReferenceExpression(new CodeTypeReference(typeof(DateTimeKind), _typeReferenceOptions)),
                    dateTime.Kind.ToString()
                );

            return new CodeObjectCreateExpression(new CodeTypeReference(typeof(DateTime), _typeReferenceOptions), year, month, day, hour, minute, second, millisecond, kind);
        }

        private void PushVisited(object value)
        {
            _visitedObjects.Push(value);
        }

        private void PopVisited()
        {
            _visitedObjects.Pop();
        }

        private bool IsVisited(object value)
        {
            return value != null && _visitedObjects.Contains(value);
        }

        private bool IsMaxDepth()
        {
            return _depth > _maxDepth;
        }
    }
}
