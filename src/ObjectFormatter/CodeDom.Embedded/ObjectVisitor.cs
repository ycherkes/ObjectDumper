using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ObjectFormatter.CodeDom.Embedded.ms.CodeDom.System.CodeDom;
using ObjectFormatter.CodeDom.Embedded.ms.Common.src.Sys.CodeDom;

namespace ObjectFormatter.CodeDom.Embedded;

internal class ObjectVisitor
{
    private readonly ICollection<string> _excludeTypes;
    private readonly bool _ignoreDefaultValues;
    private readonly bool _ignoreNullValues;
    private readonly int _maxDepth;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly Stack<object> _visitedObjects;
    private int _depth;

    public ObjectVisitor(VisitorOptions visitorOptions)
    {
        _maxDepth = visitorOptions.MaxDepth;
        _ignoreDefaultValues = visitorOptions.IgnoreDefaultValues;
        _ignoreNullValues = visitorOptions.IgnoreNullValues;
        _typeReferenceOptions = visitorOptions.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
        _excludeTypes = visitorOptions.ExcludeTypes ?? new List<string>();

        _visitedObjects = new Stack<object>();
    }

    public CodeExpression Visit(object @object)
    {
        if (IsMaxDepth())
            return new CodeSeparatedExpressionCollection(new CodeExpression[]
                {
                    new CodePrimitiveExpression(null), 
                    new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Max depth") { NoNewLine = true }))
                }, ",");

        try
        {
            _depth++;

            if (@object == null || IsPrimitive(@object)) 
                return new CodePrimitiveExpression(@object);

            if (@object is TimeSpan timeSpan) 
                return VisitTimeSpan(timeSpan);

            if (@object is DateTime dateTime) 
                return VisitDateTime(dateTime);

            if (@object is DateTimeOffset dateTimeOffset) 
                return VisitDateTimeOffset(dateTimeOffset);

            if (IsKeyValuePair(@object)) 
                return VisitKeyValuePair(@object);

            if (IsTuple(@object)) 
                return VisitTuple(@object);

            if (IsValueTuple(@object)) 
                return VisitValueTuple(@object);

            if (@object is Enum) 
                return VisitEnum(@object);

            if (@object is Guid guid) 
                return VisitGuid(guid);

            if (@object is CultureInfo cultureInfo) 
                return VisitCultureInfo(cultureInfo);

            if (@object is Type type) 
                return VisitType(type);

            if (@object is IDictionary dict)
                return VisitDictionary(dict);

            if (@object is IEnumerable enumerable)
                return VisitCollection(enumerable);

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
        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(Guid), _typeReferenceOptions),
            new CodePrimitiveExpression(guid.ToString("D")));
    }

    private CodeExpression VisitCultureInfo(CultureInfo cultureInfo)
    {
        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(CultureInfo), _typeReferenceOptions),
            new CodePrimitiveExpression(cultureInfo.ToString()));
    }

    private CodeExpression VisitEnum(object o)
    {
        return new CodePropertyReferenceExpression(
            new CodeTypeReferenceExpression(new CodeTypeReference(o.GetType(), _typeReferenceOptions)), o.ToString());
    }

    private CodeExpression VisitKeyValuePair(object o)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => p.GetValue(o)).Select(Visit);
        return new CodeObjectCreateExpression(new CodeTypeReference(objectType, _typeReferenceOptions),
            propertyValues.ToArray());
    }

    private CodeExpression VisitTuple(object o)
    {
        if (IsVisited(o))
            return new CodeSeparatedExpressionCollection(new CodeExpression[]
                {
                    new CodePrimitiveExpression(null),
                    new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { NoNewLine = true }))
                }, ",");

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
            return new CodeSeparatedExpressionCollection(new CodeExpression[]
                {
                    new CodePrimitiveExpression(null),
                    new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { NoNewLine = true }))
                }, ",");

        PushVisited(o);

        var objectType = o.GetType();

        var result = new CodeObjectCreateAndInitializeExpression(o.GetType().IsAnonymousType()
            ? new CodeAnonymousTypeReference()
            : new CodeTypeReference(objectType, _typeReferenceOptions))
        {
            InitializeExpressions = new CodeExpressionCollection(objectType.GetRuntimeProperties()
                .Where(p => p.CanRead)
                .Select(p => new
                {
                    PropertyName = p.Name,
                    Value = p.GetValue(o),
                    p.PropertyType
                })
                .Where(pv => !_excludeTypes.Contains(pv.PropertyType.FullName) &&
                             (!_ignoreNullValues || (_ignoreNullValues && pv.Value != null)) &&
                             (!_ignoreDefaultValues || !pv.PropertyType.IsValueType || (_ignoreDefaultValues &&
                                 ReflectionUtils.GetDefaultValue(pv.PropertyType)?.Equals(pv.Value) != true)))
                .Select(pv => (CodeExpression)new CodeAssignExpression(new CodePropertyReferenceExpression(null, pv.PropertyName), Visit(pv.Value)))
                .ToArray())
        };

        PopVisited();

        return result;
    }

    private CodeExpression VisitValueTuple(object o)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetFields().Select(p => p.GetValue(o)).Select(Visit);

        return new CodeValueTupleCreateExpression(propertyValues.ToArray());
    }

    private CodeExpression VisitDictionary(IDictionary dict)
    {
        if (IsVisited(dict))
            return new CodeSeparatedExpressionCollection(new CodeExpression[]
                {
                    new CodePrimitiveExpression(null),
                    new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { NoNewLine = true }))
                }, ",");

        PushVisited(dict);

        var valuesType = dict.Values.GetType();
        var keysType = dict.Keys.GetType();

        var result = ReflectionUtils.ContainsAnonymousType(keysType) ||
                     ReflectionUtils.ContainsAnonymousType(valuesType)
            ? VisitAnonymousDictionary(dict)
            : VisitSimpleDictionary(dict);

        PopVisited();

        return result;
    }

    private CodeExpression VisitSimpleDictionary(IDictionary dict)
    {
        var items = dict.Cast<object>().Select(VisitKeyValuePairGenerateImplicitly);

        var type = dict.GetType();
        var isImmutable = ReflectionUtils.IsImmutableCollection(type);

        if (isImmutable)
        {
            var keyType = ReflectionUtils.GetInnerElementType(dict.Keys.GetType());
            var valueType = ReflectionUtils.GetInnerElementType(dict.Values.GetType());

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            CodeExpression dictionaryCreateExpression = new CodeObjectCreateAndInitializeExpression(new CodeTypeReference(dictionaryType, _typeReferenceOptions), items);

            dictionaryCreateExpression = new CodeMethodInvokeExpression(dictionaryCreateExpression, $"To{type.Name.Split('`')[0]}");

            return dictionaryCreateExpression;
        }
        else
        {
            CodeExpression dictionaryCreateExpression = new CodeObjectCreateAndInitializeExpression(new CodeTypeReference(type, _typeReferenceOptions), items);
            return dictionaryCreateExpression;
        }
    }

    private CodeExpression VisitAnonymousDictionary(IEnumerable dictionary)
    {
        var items = dictionary.Cast<object>().Select(VisitKeyValuePairGenerateTuple);
        var type = dictionary.GetType();

        CodeExpression expr = new CodeArrayCreateExpression(new CodeAnonymousTypeReference(), items.ToArray());

        var variableReferenceExpression = new CodeVariableReferenceExpression("kvp");
        var keyLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Item1"), variableReferenceExpression);
        var valueLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Item2"), variableReferenceExpression);

        var isImmutable = ReflectionUtils.IsImmutableCollection(type);

        expr = isImmutable
            ? new CodeMethodInvokeExpression(expr, $"To{type.Name.Split('`')[0]}", keyLambdaExpression, valueLambdaExpression)
            : new CodeMethodInvokeExpression(expr, "ToDictionary", keyLambdaExpression, valueLambdaExpression);

        return expr;
    }

    private CodeExpression VisitCollection(IEnumerable collection)
    {
        if (IsVisited(collection))
            return new CodeSeparatedExpressionCollection(new CodeExpression[]
                {
                    new CodePrimitiveExpression(null),
                    new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { NoNewLine = true }))
                }, ",");

        PushVisited(collection);

        var collectionType = collection.GetType();

        var elementType = ReflectionUtils.GetInnerElementType(collectionType);

        var result = ReflectionUtils.ContainsAnonymousType(collectionType)
            ? VisitAnonymousCollection(collection)
            : VisitSimpleCollection(collection, elementType);

        PopVisited();

        return result;
    }

    private CodeExpression VisitSimpleCollection(IEnumerable enumerable, Type elementType)
    {
        var items = enumerable.Cast<object>().Select(Visit);

        var type = enumerable.GetType();

        var isImmutable = ReflectionUtils.IsImmutableCollection(type);

        if (!IsCollection(enumerable) || type.IsArray || isImmutable)
        {
            CodeExpression expr = new CodeArrayCreateExpression(
                new CodeTypeReference(elementType, _typeReferenceOptions),
                items.ToArray());

            if (isImmutable) expr = new CodeMethodInvokeExpression(expr, $"To{type.Name.Split('`')[0]}");

            return expr;
        }

        if (ReflectionUtils.IsReadonlyCollection(type))
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

        var isImmutable = ReflectionUtils.IsImmutableCollection(type);


        CodeExpression expr = new CodeArrayCreateExpression(
            new CodeAnonymousTypeReference(),
            items.ToArray());

        if (isImmutable || !type.IsArray) 
            expr = new CodeMethodInvokeExpression(expr, $"To{type.Name.Split('`')[0]}");

        return expr;
    }

    private CodeExpression VisitDateTimeOffset(DateTimeOffset dateTimeOffset)
    {
        var dateTimeOffsetCodeTypeReference = new CodeTypeReference(typeof(DateTimeOffset), _typeReferenceOptions);

        if (dateTimeOffset == DateTimeOffset.MaxValue)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateTimeOffsetCodeTypeReference),
                nameof(DateTimeOffset.MaxValue)
            );

        if (dateTimeOffset == DateTimeOffset.MinValue)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateTimeOffsetCodeTypeReference),
                nameof(DateTimeOffset.MinValue)
            );

        var year = new CodePrimitiveExpression(dateTimeOffset.Year);
        var month = new CodePrimitiveExpression(dateTimeOffset.Month);
        var day = new CodePrimitiveExpression(dateTimeOffset.Day);
        var hour = new CodePrimitiveExpression(dateTimeOffset.Hour);
        var minute = new CodePrimitiveExpression(dateTimeOffset.Minute);
        var second = new CodePrimitiveExpression(dateTimeOffset.Second);
        var millisecond = new CodePrimitiveExpression(dateTimeOffset.Millisecond);

        var offsetExpression = VisitTimeSpan(dateTimeOffset.Offset);

        return new CodeObjectCreateExpression(dateTimeOffsetCodeTypeReference, year, month, day, hour, minute, second,
            millisecond, offsetExpression);
    }

    private CodeExpression VisitTimeSpan(TimeSpan timeSpan)
    {
        var specialValuesDictionary = new Dictionary<TimeSpan, string>
        {
            { TimeSpan.MaxValue, nameof(TimeSpan.MaxValue) },
            { TimeSpan.MinValue, nameof(TimeSpan.MinValue) },
            { TimeSpan.Zero, nameof(TimeSpan.Zero) }
        };

        var timeSpanCodeTypeReference = new CodeTypeReference(typeof(TimeSpan), _typeReferenceOptions);

        if (specialValuesDictionary.TryGetValue(timeSpan, out var name))
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(timeSpanCodeTypeReference),
                name
            );

        var valuesCollection = new Dictionary<string, long>
        {
            { nameof(TimeSpan.FromDays), timeSpan.Days },
            { nameof(TimeSpan.FromHours), timeSpan.Hours },
            { nameof(TimeSpan.FromMinutes), timeSpan.Minutes },
            { nameof(TimeSpan.FromSeconds), timeSpan.Seconds },
            { nameof(TimeSpan.FromMilliseconds), timeSpan.Milliseconds }
        };

        var nonZeroValues = valuesCollection.Where(v => v.Value > 0).ToArray();

        if (nonZeroValues.Length == 1)
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeSpanCodeTypeReference),
                    nonZeroValues[0].Key),
                new CodePrimitiveExpression(nonZeroValues[0].Value)
            );

        if (timeSpan.TotalMilliseconds % TimeSpan.TicksPerMillisecond != 0)
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeSpanCodeTypeReference),
                    nameof(TimeSpan.FromTicks)),
                new CodePrimitiveExpression(timeSpan.Ticks)
            );

        var days = new CodePrimitiveExpression(timeSpan.Days);
        var hours = new CodePrimitiveExpression(timeSpan.Hours);
        var minutes = new CodePrimitiveExpression(timeSpan.Minutes);
        var seconds = new CodePrimitiveExpression(timeSpan.Seconds);
        var milliseconds = new CodePrimitiveExpression(timeSpan.Milliseconds);

        if (timeSpan.Days == 0 && timeSpan.Milliseconds == 0)
            return new CodeObjectCreateExpression(timeSpanCodeTypeReference, hours, minutes, seconds);

        if (timeSpan.Milliseconds == 0)
            return new CodeObjectCreateExpression(timeSpanCodeTypeReference, days, hours, minutes, seconds);

        return new CodeObjectCreateExpression(timeSpanCodeTypeReference, days, hours, minutes, seconds, milliseconds);
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

        return new CodeObjectCreateExpression(new CodeTypeReference(typeof(DateTime), _typeReferenceOptions), year,
            month, day, hour, minute, second, millisecond, kind);
    }

    private void PushVisited(object value)
    {
        _visitedObjects.Push(value);
    }

    private void PopVisited()
    {
        _visitedObjects.Pop();
    }

    private static bool IsCollection(object obj)
    {
        return obj is ICollection || ReflectionUtils.IsGenericCollection(obj.GetType());
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

    private bool IsVisited(object value)
    {
        return value != null && _visitedObjects.Contains(value);
    }

    private bool IsMaxDepth()
    {
        return _depth > _maxDepth;
    }

    private static bool IsTuple(object o)
    {
        var type = o.GetType();
        return ReflectionUtils.IsTuple(type);
    }

    private static bool IsKeyValuePair(object o)
    {
        var type = o.GetType();
        return ReflectionUtils.IsKeyValuePair(type);
    }

    private static bool IsValueTuple(object o)
    {
        var type = o.GetType();
        return ReflectionUtils.IsValueTuple(type);
    }
}