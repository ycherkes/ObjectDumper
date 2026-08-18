using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
using Serialization.UnitTests.Extensions;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests;

public class VarDumpUpgradeRegressionTest
{
    [Theory]
    [InlineData("ShortName", "new NestedModel")]
    [InlineData("NestedQualified", "new VarDumpUpgradeRegressionTest.NestedModel")]
    [InlineData("FullName", "new Serialization.UnitTests.VarDumpUpgradeRegressionTest.NestedModel")]
    public void CSharpSupportsEveryTypeNamingPolicy(string policy, string expectedTypeName)
    {
        var result = new CSharpSerializer().Serialize(
            new NestedModel { Name = "nested" },
            Settings(new { TypeNamePolicy = policy }));

        Assert.Contains(expectedTypeName, result);
    }

    [Theory]
    [InlineData("ShortName", "New NestedModel")]
    [InlineData("NestedQualified", "New VarDumpUpgradeRegressionTest.NestedModel")]
    [InlineData("FullName", "New Serialization.UnitTests.VarDumpUpgradeRegressionTest.NestedModel")]
    public void VisualBasicSupportsEveryTypeNamingPolicy(string policy, string expectedTypeName)
    {
        var result = new VisualBasicSerializer().Serialize(
            new NestedModel { Name = "nested" },
            Settings(new { TypeNamePolicy = policy }));

        Assert.Contains(expectedTypeName, result);
    }

    [Fact]
    public void NewLineStyleAppliesToBothLanguages()
    {
        var model = new NestedModel { Name = "nested" };
        var unixSettings = Settings(new { NewLineStyle = "Unix" });
        var windowsSettings = Settings(new { NewLineStyle = "Windows" });

        var csharpUnix = new CSharpSerializer().Serialize(model, unixSettings);
        var visualBasicUnix = new VisualBasicSerializer().Serialize(model, unixSettings);
        var csharpWindows = new CSharpSerializer().Serialize(model, windowsSettings);
        var visualBasicWindows = new VisualBasicSerializer().Serialize(model, windowsSettings);

        Assert.DoesNotContain("\r\n", csharpUnix);
        Assert.DoesNotContain("\r\n", visualBasicUnix);
        Assert.Contains("\r\n", csharpWindows);
        Assert.Contains("\r\n", visualBasicWindows);
    }

    [Fact]
    public void IncludesBaseClassFieldsInBothLanguagesWhenEnabled()
    {
        var model = new DerivedFieldsModel(10, 20);
        var settings = Settings(new
        {
            GetBaseClassFields = true,
            GetFieldsBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        });

        var csharp = new CSharpSerializer().Serialize(model, settings);
        var visualBasic = new VisualBasicSerializer().Serialize(model, settings);

        Assert.Contains("_baseNumber = 10", csharp);
        Assert.Contains("_derivedNumber = 20", csharp);
        Assert.Contains("._baseNumber = 10", visualBasic);
        Assert.Contains("._derivedNumber = 20", visualBasic);
    }

    [Theory]
    [InlineData("Escaped", "\\r\\n")]
    [InlineData("Verbatim", "@\"")]
    [InlineData("Raw", "\"\"\"")]
    public void CSharpSupportsEveryStringLiteralStyle(string style, string expectedSyntax)
    {
        var result = new CSharpSerializer().Serialize(
            "line1\r\npath\\file\"name\"",
            Settings(new { StringLiteralStyle = style }));

        Assert.Contains(expectedSyntax, result);
    }

    [Fact]
    public void CSharpSupportsCollectionExpressions()
    {
        var result = new CSharpSerializer().Serialize(
            new List<int> { 1, 2, 3 },
            Settings(new
            {
                CollectionLiteralStyle = "Expression",
                PrimitiveCollectionLayout = "SingleLine"
            }));

        Assert.Contains("List<int> listOfInt = [1, 2, 3];", result);
    }

    [Fact]
    public void CSharpSerializesUnnamedEnumValues()
    {
        var serializer = new CSharpSerializer();

        var zero = serializer.Serialize((UpgradeEnum)0, null);
        var negative = serializer.Serialize((UpgradeEnum)(object)-54, null);

        Assert.Contains("= 0;", zero);
        Assert.Contains("(UpgradeEnum)(object)-54", negative);
    }

    [Fact]
    public void VisualBasicSerializesUnnamedNegativeEnumValue()
    {
        var result = new VisualBasicSerializer().Serialize((UpgradeEnum)(object)-54, null);

        Assert.Contains("CType(CType(-54, Object), UpgradeEnum)", result);
    }

    [Fact]
    public void SerializesQueryableAsQueryableInBothLanguages()
    {
        var query = new[] { 5, 6 }.AsQueryable();

        var csharp = new CSharpSerializer().Serialize(query, null);
        var visualBasic = new VisualBasicSerializer().Serialize(query, null);

        Assert.Contains("new int[]", csharp);
        Assert.Contains(".AsQueryable()", csharp);
        Assert.Contains("New Integer()", visualBasic);
        Assert.Contains(".AsQueryable()", visualBasic);
    }

    [Fact]
    public void DetectsCyclesButNotSharedSiblingReferences()
    {
        var cycle = new Node { Name = "root" };
        cycle.Next = cycle;

        var serializer = new CSharpSerializer();
        var cycleResult = serializer.Serialize(cycle, null);

        var shared = new Node { Name = "shared" };
        var siblingResult = serializer.Serialize(new NodePair { First = shared, Second = shared }, null);

        Assert.Contains("Circular reference detected", cycleResult);
        Assert.DoesNotContain("Circular reference detected", siblingResult);
        Assert.Equal(2, CountOccurrences(siblingResult, "Name = \"shared\""));
    }

    [Fact]
    public void DetectsCircularCollectionsAndDictionaries()
    {
        var list = new ArrayList();
        list.Add(list);

        var dictionary = new Hashtable();
        dictionary["self"] = dictionary;

        var serializer = new CSharpSerializer();

        Assert.Contains("Circular reference detected", serializer.Serialize(list, null));
        Assert.Contains("Circular reference detected", serializer.Serialize(dictionary, null));
    }

    [Fact]
    public void AppliesDefaultValueAndPrivateSetterRules()
    {
        var model = new DefaultsModel(5, "included");
        var settings = Settings(new
        {
            GetPropertiesBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        });

        var result = new CSharpSerializer().Serialize(model, settings);

        Assert.DoesNotContain("AttributedDefault", result);
        Assert.DoesNotContain("ZeroDuration", result);
        Assert.Contains("PrivateSetter = \"included\"", result);
        Assert.DoesNotContain("ReadOnly", result);
    }

    [Fact]
    public void StreamsSingleUseEnumerableAndHonorsMaxCollectionSize()
    {
        var values = new SingleUseEnumerable(1, 2, 3, 4);

        var result = new CSharpSerializer().Serialize(
            values,
            Settings(new { MaxCollectionSize = 2 }));

        Assert.Equal(1, values.EnumerationCount);
        Assert.Contains("Too many items (> 2)", result);
        Assert.Contains("1", result);
        Assert.Contains("2", result);
        Assert.DoesNotContain("3,", result);
    }

    [Fact]
    public void DefaultStringLiteralStylePreservesAutomaticFormatting()
    {
        var longPath = string.Concat(Enumerable.Repeat("C:\\folder\\", 35));
        var multiline = "line one\r\nline two";
        var serializer = new CSharpSerializer();

        var longPathResult = serializer.Serialize(longPath, null);
        var multilineResult = serializer.Serialize(multiline, null);

        Assert.Contains("@\"", longPathResult);
        Assert.Contains("\\r\\n", multilineResult);
    }

    private static string Settings(object value) => JsonConvert.SerializeObject(value);

    private static int CountOccurrences(string value, string search) =>
        value.Split([search], StringSplitOptions.None).Length - 1;

    private enum UpgradeEnum
    {
        One = 1
    }

    private sealed class NestedModel
    {
        public string Name { get; set; }
    }

    private sealed class Node
    {
        public string Name { get; set; }
        public Node Next { get; set; }
    }

    private sealed class NodePair
    {
        public Node First { get; set; }
        public Node Second { get; set; }
    }

    private sealed class DefaultsModel
    {
        public DefaultsModel(int attributedDefault, string privateSetter)
        {
            AttributedDefault = attributedDefault;
            PrivateSetter = privateSetter;
        }

        [DefaultValue(5)]
        public int AttributedDefault { get; set; }

        public TimeSpan ZeroDuration { get; set; }

        public string PrivateSetter { get; private set; }

        public string ReadOnly => "excluded";
    }

    private class BaseFieldsModel(int baseNumber)
    {
        private readonly int _baseNumber = baseNumber;
    }

    private sealed class DerivedFieldsModel(int baseNumber, int derivedNumber) : BaseFieldsModel(baseNumber)
    {
        private readonly int _derivedNumber = derivedNumber;
    }

    private sealed class SingleUseEnumerable(params int[] values) : IEnumerable<int>
    {
        public int EnumerationCount { get; private set; }

        public IEnumerator<int> GetEnumerator()
        {
            EnumerationCount++;
            if (EnumerationCount > 1)
            {
                throw new InvalidOperationException("The enumerable was enumerated more than once.");
            }

            return ((IEnumerable<int>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
