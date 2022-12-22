using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Common;

namespace YellowFlavor.Serialization.Embedded.CodeDom;

internal class ObjectVisitor
{
    private readonly ICollection<string> _excludeTypes;
    private readonly bool _ignoreDefaultValues;
    private readonly bool _ignoreNullValues;
    private readonly int _maxDepth;
    private readonly CodeTypeReferenceOptions _typeReferenceOptions;
    private readonly Stack<object> _visitedObjects;
    private int _depth;
    private readonly DateTimeInstantiation _dateTimeInstantiation;
    private readonly DateKind _dateKind;
    private readonly bool _useNamedArgumentsForReferenceRecordTypes;
    private readonly bool _writablePropertiesOnly;
    private readonly BindingFlags _getPropertiesBindingFlags;
    private readonly BindingFlags? _getFieldsBindingFlags;
    private readonly ListSortDirection? _sortDirection;

    public ObjectVisitor(VisitorOptions visitorOptions)
    {
        _dateTimeInstantiation = visitorOptions.DateTimeInstantiation;
        _dateKind = visitorOptions.DateKind;
        _maxDepth = visitorOptions.MaxDepth;
        _ignoreDefaultValues = visitorOptions.IgnoreDefaultValues;
        _ignoreNullValues = visitorOptions.IgnoreNullValues;
        _typeReferenceOptions = visitorOptions.UseTypeFullName
            ? CodeTypeReferenceOptions.FullTypeName
            : CodeTypeReferenceOptions.ShortTypeName;
        _excludeTypes = visitorOptions.ExcludeTypes ?? new List<string>();
        _useNamedArgumentsForReferenceRecordTypes = visitorOptions.UseNamedArgumentsForReferenceRecordTypes;
        _getPropertiesBindingFlags = visitorOptions.GetPropertiesBindingFlags;
        _writablePropertiesOnly = visitorOptions.WritablePropertiesOnly;
        _getFieldsBindingFlags = visitorOptions.GetFieldsBindingFlags;
        _sortDirection = visitorOptions.SortDirection;

        _visitedObjects = new Stack<object>();
    }

    public CodeExpression Visit(object @object)
    {
        if (IsMaxDepth())
        {
            return GetMaxDepthExpression(@object);
        }

        try
        {
            _depth++;

            if (IsPrimitiveOrNull(@object))
                return VisitPrimitive(@object);

            if (@object is TimeSpan timeSpan)
                return VisitTimeSpan(timeSpan);

            if (@object is DateTime dateTime)
                return VisitDateTime(dateTime);

            if (@object is DateTimeOffset dateTimeOffset)
                return VisitDateTimeOffset(dateTimeOffset);

            if (IsDateOnly(@object))
                return VisitDateOnly(@object);

            if (IsTimeOnly(@object))
                return VisitTimeOnly(@object);

            if (IsRecord(@object))
                return VisitRecord(@object);

            if (IsAnonymous(@object))
                return VisitAnonymous(@object);

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

            if (IsGrouping(@object))
                return VisitGrouping(@object);

            if (@object is IDictionary dict)
                return VisitDictionary(dict);

            if (@object is IEnumerable enumerable)
                return VisitCollection(enumerable);

            try
            {
                if (@object is ISerializable serializable)
                    return VisitSerializable(serializable);
            }
            catch
            {
                // ignored
            }

            return VisitObject(@object);
        }
        finally
        {
            _depth--;
        }
    }

    private CodeExpression VisitGrouping(object o)
    {
        CodeExpression expr = VisitGroupings(new[] { o });

        var variableReferenceExpression = new CodeVariableReferenceExpression("grp");
        var keyLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Key"), variableReferenceExpression);
        var valueLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Element"), variableReferenceExpression);

        expr = new CodeMethodInvokeExpression(expr, "GroupBy", keyLambdaExpression, valueLambdaExpression);
        expr = new CodeMethodInvokeExpression(expr, "Single");

        return expr;
    }

    private static KeyValuePair<object, IEnumerable> GetIGroupingValue(object o)
    {
        var objectType = o.GetType();
        var fieldValues = objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(x => x.Name is "_key" or "key" or "_elements" or "elements")
            .Select(p => GetValue(p, o))
            .ToArray();

        return new KeyValuePair<object, IEnumerable>(fieldValues[0], (IEnumerable)fieldValues[1]);
    }

    private CodeExpression VisitGroupings(IEnumerable<object> objects)
    {
        var groupingValues = objects.Select(GetIGroupingValue)
            .SelectMany(g => g.Value.Cast<object>().Select(e => new { g.Key, Element = e }))
            .ToArray();

        return Visit(groupingValues);
    }

    private CodeExpression VisitAnonymous(object o)
    {
        var objectType = o.GetType();
        var result = new CodeObjectCreateAndInitializeExpression(new CodeAnonymousTypeReference())
        {
            InitializeExpressions = new CodeExpressionCollection(objectType.GetProperties(_getPropertiesBindingFlags)
                .Where(p => p.CanRead)
                .Select(p => new
                {
                    PropertyName = p.Name,
                    Value = GetValue(p, o),
                    p.PropertyType
                })
                .Select(pv => (CodeExpression)new CodeAssignExpression(
                    new CodePropertyReferenceExpression(null, pv.PropertyName),
                    ReflectionUtils.IsNullableType(pv.PropertyType) || pv.Value == null ? new CodeCastExpression(pv.PropertyType, Visit(pv.Value), true) : Visit(pv.Value)))
                .ToArray())
        };

        return result;
    }

    private static CodeExpression VisitPrimitive(object @object)
    {
        if (@object == null || ValueEquality(@object, 0) || @object is byte)
        {
            return new CodePrimitiveExpression(@object);
        }

        var objectType = @object.GetType();

        var specialValueExpression = new[]
        {
            nameof(int.MaxValue),
            nameof(int.MinValue),
            nameof(float.PositiveInfinity),
            nameof(float.NegativeInfinity),
            nameof(float.Epsilon),
            nameof(float.NaN)
        }
        .Select(specialValue => GetSpecialValue(@object, objectType, specialValue))
        .FirstOrDefault(x => x != null);

        return specialValueExpression ?? new CodePrimitiveExpression(@object);
    }

    private static bool ValueEquality(object val1, object val2)
    {
        if (val1 is not IConvertible) return false;
        if (val2 is not IConvertible) return false;

        var converted2 = Convert.ChangeType(val2, val1.GetType());

        return val1.Equals(converted2);
    }

    private static CodeExpression GetSpecialValue(object @object, Type objectType, string fieldName)
    {
        var field = objectType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

        if (field == null) return null;

        return Equals(GetValue(field, null), @object)
            ? new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(objectType), fieldName)
            : null;
    }

    private CodeExpression VisitRecord(object o)
    {
        var objectType = o.GetType();
        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(p => p.CanWrite);
        var argumentValues = _useNamedArgumentsForReferenceRecordTypes ?
            properties.Select(p => (CodeExpression)new CodeNamedArgumentExpression(p.Name, Visit(GetValue(p, o))))
            : properties.Select(p => GetValue(p, o)).Select(Visit);

        return new CodeObjectCreateExpression(
            new CodeTypeReference(objectType, _typeReferenceOptions),
            argumentValues.ToArray());
    }

    private static bool IsRecord(object o)
    {
        var objectType = o.GetType();

        var constructor = objectType.GetConstructors().FirstOrDefault();

        if (constructor == null)
        {
            return false;
        }

        if (objectType.GetMethods().All(m => m.Name != "<Clone>$"))
        {
            return false;
        }

        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.CanWrite);

        return constructor.GetParameters().Select(x => new { x.Name, Type = x.ParameterType })
            .SequenceEqual(properties.Select(x => new { x.Name, Type = x.PropertyType }));
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
        var values = o.ToString().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        if (values.Length == 1)
        {
            return new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(new CodeTypeReference(o.GetType(), _typeReferenceOptions)), o.ToString());
        }

        var expressions = values.Select(v => (CodeExpression)new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(new CodeTypeReference(o.GetType(), _typeReferenceOptions)), v.Trim())).ToArray();

        var bitwiseOrExpression = new CodeFlagsBinaryOperatorExpression(CodeBinaryOperatorType.BitwiseOr, expressions);

        return bitwiseOrExpression;
    }

    private CodeExpression VisitKeyValuePair(object o)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => GetValue(p, o)).Select(Visit);
        return new CodeObjectCreateExpression(new CodeTypeReference(objectType, _typeReferenceOptions),
            propertyValues.ToArray());
    }

    private CodeExpression VisitTuple(object o)
    {
        if (IsVisited(o))
        {
            return GetCircularReferenceDetectedExpression();
        }

        PushVisited(o);

        try
        {
            var objectType = o.GetType();
            var propertyValues = objectType.GetProperties().Select(p => GetValue(p, o)).Select(Visit);
            var result = new CodeObjectCreateExpression(new CodeTypeReference(objectType, _typeReferenceOptions), propertyValues.ToArray());
            return result;
        }
        finally
        {
            PopVisited();
        }
    }

    private CodeExpression VisitKeyValuePairGenerateAnonymousType(object o, string keyName, string valueName)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => GetValue(p, o)).Select(Visit).ToArray();
        var result = new CodeObjectCreateAndInitializeExpression(new CodeAnonymousTypeReference())
        {
            InitializeExpressions = new CodeExpressionCollection(new[]
            {
                (CodeExpression)new CodeAssignExpression(new CodePropertyReferenceExpression(null, keyName), propertyValues[0]),
                new CodeAssignExpression(new CodePropertyReferenceExpression(null, valueName), propertyValues[1])
            })
        };

        return result;
    }

    private CodeExpression VisitKeyValuePairGenerateImplicitly(object o)
    {
        var objectType = o.GetType();
        var propertyValues = objectType.GetProperties().Select(p => GetValue(p, o)).Select(Visit).Take(2).ToArray();
        return new CodeImplicitKeyValuePairCreateExpression(propertyValues.First(), propertyValues.Last());
    }

    private CodeExpression VisitSerializable(ISerializable serializable)
    {
        if (IsVisited(serializable))
        {
            return GetCircularReferenceDetectedExpression();
        }

        PushVisited(serializable);

        try
        {
            var objectType = serializable.GetType();
            SerializationInfo serializationInfo = new SerializationInfo(objectType, new FormatterConverter());
            serializable.GetObjectData(serializationInfo, new StreamingContext());

            var serializationEntries = serializationInfo.GetEnumerator()
                                     .Cast<SerializationEntry>();

            if (_sortDirection != null)
            {
                serializationEntries = _sortDirection == ListSortDirection.Ascending
                    ? serializationEntries.OrderBy(x => x.Name)
                    : serializationEntries.OrderByDescending(x => x.Name);
            }

            var result = new CodeObjectCreateAndInitializeExpression(new CodeTypeReference(objectType, _typeReferenceOptions))
            {
                InitializeExpressions = new CodeExpressionCollection(serializationEntries
                                     .Where(se => !_excludeTypes.Contains(se.ObjectType.FullName) &&
                                                  (!_ignoreNullValues || (_ignoreNullValues && se.Value != null)) &&
                                                  (!_ignoreDefaultValues || !se.ObjectType.IsValueType || (_ignoreDefaultValues &&
                                                      ReflectionUtils.GetDefaultValue(se.ObjectType)?.Equals(se.Value) != true)))
                                     .Select(pv => (CodeExpression)new CodeAssignExpression(new CodePropertyReferenceExpression(null, pv.Name), Visit(pv.Value)))
                                     .ToArray())
            };

            return result;
        }
        finally
        {
            PopVisited();
        }
    }

    private CodeExpression VisitObject(object o)
    {
        if (IsVisited(o))
        {
            return GetCircularReferenceDetectedExpression();
        }

        PushVisited(o);

        try
        {
            var objectType = o.GetType();

            var initProperties = objectType.GetProperties(_getPropertiesBindingFlags)
                    .Where(p => p.CanRead && (p.CanWrite || !_writablePropertiesOnly))
                    .Select(p => new
                    {
                        p.Name,
                        Value = GetValue(p, o),
                        Type = p.PropertyType
                    });

            if (_getFieldsBindingFlags != null)
            {
                var fields = objectType.GetFields(_getFieldsBindingFlags.Value)
                    .Select(f => new
                    {
                        f.Name,
                        Value = GetValue(f, o),
                        Type = f.FieldType
                    });

                initProperties = initProperties.Concat(fields);
            }

            if (_sortDirection != null)
            {
                initProperties = _sortDirection == ListSortDirection.Ascending
                    ? initProperties.OrderBy(x => x.Name)
                    : initProperties.OrderByDescending(x => x.Name);
            }

            var result = new CodeObjectCreateAndInitializeExpression(new CodeTypeReference(objectType, _typeReferenceOptions))
            {
                InitializeExpressions = new CodeExpressionCollection(initProperties
                    .Where(pv => !_excludeTypes.Contains(pv.Type.FullName) &&
                                 (!_ignoreNullValues || (_ignoreNullValues && pv.Value != null)) &&
                                 (!_ignoreDefaultValues || !pv.Type.IsValueType || (_ignoreDefaultValues &&
                                     ReflectionUtils.GetDefaultValue(pv.Type)?.Equals(pv.Value) != true)))
                    .Select(pv => (CodeExpression)new CodeAssignExpression(new CodePropertyReferenceExpression(null, pv.Name), Visit(pv.Value)))
                    .ToArray())
            };

            return result;
        }
        finally
        {
            PopVisited();
        }
    }

    private CodeExpression VisitValueTuple(object @object)
    {
        var objectType = @object.GetType();
        var propertyValues = objectType.GetFields().Select(p => GetValue(p, @object)).Select(Visit);

        return new CodeValueTupleCreateExpression(propertyValues.ToArray());
    }

    private CodeExpression VisitDictionary(IDictionary dict)
    {
        if (IsVisited(dict))
        {
            return GetCircularReferenceDetectedExpression();
        }

        PushVisited(dict);

        try
        {
            var valuesType = dict.Values.GetType();
            var keysType = dict.Keys.GetType();

            var result = ReflectionUtils.ContainsAnonymousType(keysType) ||
                         ReflectionUtils.ContainsAnonymousType(valuesType)
                ? VisitAnonymousDictionary(dict)
                : VisitSimpleDictionary(dict);

            return result;
        }
        finally
        {
            PopVisited();
        }
    }

    private CodeExpression VisitSimpleDictionary(IDictionary dict)
    {
        var items = dict.Cast<object>().Select(VisitKeyValuePairGenerateImplicitly);

        var type = dict.GetType();
        var isImmutable = ReflectionUtils.IsPublicImmutableCollection(type);

        if (isImmutable)
        {
            var keyType = ReflectionUtils.GetInnerElementType(dict.Keys.GetType());
            var valueType = ReflectionUtils.GetInnerElementType(dict.Values.GetType());

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            CodeExpression dictionaryCreateExpression = new CodeObjectCreateAndInitializeExpression(new CodeCollectionTypeReference(dictionaryType, _typeReferenceOptions), items);

            dictionaryCreateExpression = new CodeMethodInvokeExpression(dictionaryCreateExpression, $"To{type.Name.Split('`')[0]}");

            return dictionaryCreateExpression;
        }
        else
        {
            CodeExpression dictionaryCreateExpression = new CodeObjectCreateAndInitializeExpression(new CodeCollectionTypeReference(type, _typeReferenceOptions), items);
            return dictionaryCreateExpression;
        }
    }

    private CodeExpression VisitAnonymousDictionary(IEnumerable dictionary)
    {
        const string keyName = "Key";
        const string valueName = "Value";
        var items = dictionary.Cast<object>().Select(o => VisitKeyValuePairGenerateAnonymousType(o, keyName, valueName));
        var type = dictionary.GetType();

        CodeExpression expr = new CodeArrayCreateExpression(new CodeAnonymousTypeReference(), items.ToArray());

        var variableReferenceExpression = new CodeVariableReferenceExpression("kvp");
        var keyLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, keyName), variableReferenceExpression);
        var valueLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, valueName), variableReferenceExpression);

        var isImmutable = ReflectionUtils.IsPublicImmutableCollection(type);

        expr = isImmutable
            ? new CodeMethodInvokeExpression(expr, $"To{type.Name.Split('`')[0]}", keyLambdaExpression, valueLambdaExpression)
            : new CodeMethodInvokeExpression(expr, "ToDictionary", keyLambdaExpression, valueLambdaExpression);

        return expr;
    }

    private CodeExpression VisitCollection(IEnumerable collection)
    {
        if (IsVisited(collection))
        {
            return GetCircularReferenceDetectedExpression();
        }

        PushVisited(collection);

        try
        {
            var collectionType = collection.GetType();

            var elementType = ReflectionUtils.GetInnerElementType(collectionType);

            if (ReflectionUtils.IsGrouping(elementType))
            {
                return VisitGroupingCollection(collection);
            }

            var result = ReflectionUtils.ContainsAnonymousType(collectionType)
                ? VisitAnonymousCollection(collection)
                : VisitSimpleCollection(collection, elementType);

            return result;
        }
        finally
        {
            PopVisited();
        }
    }

    private CodeExpression VisitGroupingCollection(IEnumerable collection)
    {
        var type = collection.GetType();

        CodeExpression expr = VisitGroupings(collection.Cast<object>());

        var variableReferenceExpression = new CodeVariableReferenceExpression("grp");
        var keyLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Key"), variableReferenceExpression);
        var valueLambdaExpression = new CodeLambdaExpression(new CodePropertyReferenceExpression(variableReferenceExpression, "Element"), variableReferenceExpression);

        var isLookup = ReflectionUtils.IsLookup(type);

        expr = isLookup
            ? new CodeMethodInvokeExpression(expr, "ToLookup", keyLambdaExpression, valueLambdaExpression)
            : new CodeMethodInvokeExpression(expr, "GroupBy", keyLambdaExpression, valueLambdaExpression);

        if (type.IsArray)
        {
            expr = new CodeMethodInvokeExpression(expr, "ToArray");
        }
        else if (collection is IList)
        {
            expr = new CodeMethodInvokeExpression(expr, "ToList");
        }

        return expr;
    }

    private CodeExpression VisitSimpleCollection(IEnumerable enumerable, Type elementType)
    {
        var items = enumerable.Cast<object>().Select(Visit);

        var type = enumerable.GetType();

        var isImmutable = ReflectionUtils.IsPublicImmutableCollection(type);

        if (type.IsArray || isImmutable || !IsCollection(enumerable))
        {
            if (type.IsArray && ((Array)enumerable).Rank > 1)
            {
                items = ChunkMultiDimensionalArrayExpression((Array)enumerable, items);
            }

            CodeExpression expr = new CodeArrayCreateExpression(
                new CodeTypeReference(type.IsArray ? type : elementType.MakeArrayType(), _typeReferenceOptions),
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
            new CodeCollectionTypeReference(type, _typeReferenceOptions),
            items.ToArray()
        );

        return initializeExpression;
    }

    private static IEnumerable<CodeExpression> ChunkMultiDimensionalArrayExpression(Array array, IEnumerable<CodeExpression> enumerable)
    {
        var dimensions = new int[array.Rank - 1];

        for (var i = 0; i < dimensions.Length; i++)
        {
            dimensions[i] = array.GetLength(i + 1);
        }

        IEnumerable<CodeExpression> result = enumerable;

        for (var index = dimensions.Length - 1; index >= 0; index--)
        {
            var dimension = dimensions[index];
            result = result.Chunk(dimension).Select(x => new CodeArrayDimensionExpression(x));
        }

        return result;
    }

    private CodeExpression VisitAnonymousCollection(IEnumerable enumerable)
    {
        var items = enumerable.Cast<object>().Select(Visit);
        var type = enumerable.GetType();

        var isImmutable = ReflectionUtils.IsPublicImmutableCollection(type);

        var typeReference = new CodeAnonymousTypeReference();

        if (type.IsArray && ((Array)enumerable).Rank > 1)
        {
            typeReference.ArrayRank = ((Array)enumerable).Rank;
            items = ChunkMultiDimensionalArrayExpression((Array)enumerable, items);
        }

        CodeExpression expr = new CodeArrayCreateExpression(
            typeReference,
            items.ToArray());

        if (isImmutable || (enumerable is IList && !type.IsArray))
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

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(dateTimeOffsetCodeTypeReference),
                    nameof(DateTimeOffset.ParseExact)),
                new CodePrimitiveExpression(dateTimeOffset.ToString("O")),
                new CodePrimitiveExpression("O"),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(CultureInfo), _typeReferenceOptions)),
                        nameof(CultureInfo.InvariantCulture)),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(DateTimeStyles), _typeReferenceOptions)),
                        nameof(DateTimeStyles.RoundtripKind))
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

        if (timeSpan.Ticks % TimeSpan.TicksPerMillisecond != 0)
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeSpanCodeTypeReference),
                    nameof(TimeSpan.FromTicks)),
                new CodePrimitiveExpression(timeSpan.Ticks)
            );

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeSpanCodeTypeReference),
                    nameof(TimeSpan.ParseExact)),
                new CodePrimitiveExpression(timeSpan.ToString("c")),
                new CodePrimitiveExpression("c"),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(CultureInfo), _typeReferenceOptions)),
                        nameof(CultureInfo.InvariantCulture)),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(TimeSpanStyles), _typeReferenceOptions)),
                        nameof(TimeSpanStyles.None))
            );
        }

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
        var dateTimeCodeTypeReference = new CodeTypeReference(typeof(DateTime), _typeReferenceOptions);

        if (dateTime == DateTime.MaxValue)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateTimeCodeTypeReference),
                nameof(DateTime.MaxValue)
            );

        if (dateTime == DateTime.MinValue)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateTimeCodeTypeReference),
                nameof(DateTime.MinValue)
            );

        if (_dateKind == DateKind.ConvertToUtc)
        {
            dateTime = dateTime.ToUniversalTime();
        }

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(dateTimeCodeTypeReference),
                    nameof(DateTime.ParseExact)),
                new CodePrimitiveExpression(dateTime.ToString("O")),
                new CodePrimitiveExpression("O"),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(CultureInfo), _typeReferenceOptions)),
                        nameof(CultureInfo.InvariantCulture)),
                new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(DateTimeStyles), _typeReferenceOptions)),
                        nameof(DateTimeStyles.RoundtripKind))
            );
        }

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

        return new CodeObjectCreateExpression(dateTimeCodeTypeReference, year,
            month, day, hour, minute, second, millisecond, kind);
    }

    private CodeExpression VisitDateOnly(object dateOnly)
    {
        var objectType = dateOnly.GetType();
        var dateOnlyCodeTypeReference = new CodeTypeReference(objectType, _typeReferenceOptions);
        var dayNumber = (int?)objectType.GetProperty("DayNumber")?.GetValue(dateOnly);

        if (dayNumber == null)
        {
            return GetErrorDetectedExpression("Wrong DateOnly struct");
        }

        if (dayNumber == 3652058U)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateOnlyCodeTypeReference),
                nameof(DateTime.MaxValue)
            );

        if (dayNumber == 1)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(dateOnlyCodeTypeReference),
                nameof(DateTime.MinValue)
            );

        var dateTime = new DateTime((long)dayNumber * 864000000000L);

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(dateOnlyCodeTypeReference),
                    nameof(DateTime.ParseExact)),
                new CodePrimitiveExpression($"{dateTime:yyyy-MM-dd}"),
                new CodePrimitiveExpression("O")
            );
        }

        var year = new CodePrimitiveExpression(dateTime.Year);
        var month = new CodePrimitiveExpression(dateTime.Month);
        var day = new CodePrimitiveExpression(dateTime.Day);

        return new CodeObjectCreateExpression(dateOnlyCodeTypeReference, year, month, day);
    }

    private CodeExpression VisitTimeOnly(object timeOnly)
    {
        var objectType = timeOnly.GetType();
        var timeOnlyCodeTypeReference = new CodeTypeReference(objectType, _typeReferenceOptions);
        var ticks = (long?)objectType.GetProperty("Ticks")?.GetValue(timeOnly);

        if (ticks == null)
        {
            return GetErrorDetectedExpression("Wrong TimeOnly struct");
        }

        if (ticks == 863999999999)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(timeOnlyCodeTypeReference),
                nameof(DateTime.MaxValue)
            );

        if (ticks == 0)
            return new CodeFieldReferenceExpression
            (
                new CodeTypeReferenceExpression(timeOnlyCodeTypeReference),
                nameof(DateTime.MinValue)
            );

        var timeSpan = TimeSpan.FromTicks(ticks.Value);

        if (timeSpan.Ticks % TimeSpan.TicksPerMillisecond != 0)
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeOnlyCodeTypeReference),
                    nameof(TimeSpan.FromTicks)),
                new CodePrimitiveExpression(ticks.Value)
            );

        if (_dateTimeInstantiation == DateTimeInstantiation.Parse)
        {
            return new CodeMethodInvokeExpression
            (
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(timeOnlyCodeTypeReference),
                    nameof(DateTime.ParseExact)),
                new CodePrimitiveExpression($"{timeSpan:c}"),
                new CodePrimitiveExpression("O")
            );
        }

        var hour = new CodePrimitiveExpression(timeSpan.Hours);
        var minute = new CodePrimitiveExpression(timeSpan.Minutes);
        var second = new CodePrimitiveExpression(timeSpan.Seconds);
        var millisecond = new CodePrimitiveExpression(timeSpan.Milliseconds);

        return new CodeObjectCreateExpression(timeOnlyCodeTypeReference, hour, minute, second, millisecond);
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

    private static CodeExpression GetErrorDetectedExpression(string errorMessage)
    {
        return new CodeSeparatedExpressionCollection(new CodeExpression[]
        {
            new CodePrimitiveExpression(null),
            new CodeStatementExpression(new CodeCommentStatement(new CodeComment(errorMessage) { NoNewLine = true }))
        }, ", ");
    }

    private static CodeExpression GetCircularReferenceDetectedExpression()
    {
        return new CodeSeparatedExpressionCollection(new CodeExpression[]
        {
            new CodePrimitiveExpression(null),
            new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Circular reference detected") { NoNewLine = true }))
        }, ", ");
    }

    private CodeExpression GetMaxDepthExpression(object @object)
    {
        return new CodeSeparatedExpressionCollection(new CodeExpression[]
        {
            @object == null
            ? new CodePrimitiveExpression(null)
            : new CodeDefaultValueExpression(new CodeTypeReference(@object.GetType(), _typeReferenceOptions)),
            new CodeStatementExpression(new CodeCommentStatement(new CodeComment("Max depth") { NoNewLine = true }))
        }, ", ");
    }

    private static bool IsPrimitiveOrNull(object @object)
    {
        return @object == null || IsPrimitive(@object);
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

    private static object GetValue(PropertyInfo propertyInfo, object instance)
    {
        try
        {
            return propertyInfo.GetValue(instance);
        }
        catch (Exception exception)
        {
            return exception.ToString();
        }
    }

    private static object GetValue(FieldInfo fieldInfo, object instance)
    {
        try
        {
            return fieldInfo.GetValue(instance);
        }
        catch (Exception exception)
        {
            return exception.ToString();
        }
    }

    private static bool IsAnonymous(object o)
    {
        return o.GetType().IsAnonymousType();
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

    private static bool IsGrouping(object o)
    {
        var type = o.GetType();
        return ReflectionUtils.IsGrouping(type);
    }

    private static bool IsValueTuple(object o)
    {
        var type = o.GetType();
        return ReflectionUtils.IsValueTuple(type);
    }
    private static bool IsDateOnly(object o)
    {
        var objectType = o.GetType();
        return objectType.Namespace == "System" && objectType.Name == "DateOnly";
    }

    private static bool IsTimeOnly(object o)
    {
        var objectType = o.GetType();
        return objectType.Namespace == "System" && objectType.Name == "TimeOnly";
    }
}