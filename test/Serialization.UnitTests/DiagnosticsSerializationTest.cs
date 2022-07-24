using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System.Text;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class DiagnosticsSerializationTest
    {
        [Fact]
        public void SerializeDiagnosticsCsharp()
        {
            var code =
        @"namespace Debuggable
        {
            public class HelloWorld
            {
                public string Greet(string name)
                {
                    var result = ""Hello, "" + name;
                    var anonymous = new { Name = ""Boris"" };
                    Console.WriteLine(result);
                    return result;
                }
            }
        }
        ";
            var failures = CreateAssembly(code, DefaultReferences);

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(failures, JsonConvert.SerializeObject(new
            {
                WritablePropertiesOnly = false
            }));

            Assert.Equal(
@"var whereArrayIteratorOfDiagnostic = new Diagnostic[]
{
    new CSDiagnostic
    {
        Location = new SourceLocation
        {
            Kind = LocationKind.SourceFile,
            SourceSpan = new TextSpan
            {
                Start = 287,
                End = 294,
                Length = 7
            },
            SourceTree = new ParsedSyntaxTree
            {
                FilePath = ""generated.cs"",
                Encoding = new UTF8EncodingSealed
                {
                    Preamble = @""System.NotSupportedException: Specified method is not supported.
   at System.Reflection.RuntimeMethodInfo.ThrowNoInvokeException()
   at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
   at System.Reflection.RuntimePropertyInfo.GetValue(Object obj, BindingFlags invokeAttr, Binder binder, Object[] index, CultureInfo culture)
   at System.Reflection.PropertyInfo.GetValue(Object obj)
   at YellowFlavor.Serialization.Embedded.CodeDom.ObjectVisitor.GetValue(PropertyInfo propertyInfo, Object instance) in C:\Plays\ObjectDumper\src\Serialization\Embedded\CodeDom\ObjectVisitor.cs:line 827"",
                    BodyName = ""utf-8"",
                    EncodingName = ""Unicode (UTF-8)"",
                    HeaderName = ""utf-8"",
                    WebName = ""utf-8"",
                    WindowsCodePage = 1200,
                    IsBrowserDisplay = true,
                    IsBrowserSave = true,
                    IsMailNewsDisplay = true,
                    IsMailNewsSave = true,
                    EncoderFallback = new EncoderReplacementFallback
                    {
                        DefaultString = ""�"",
                        MaxCharCount = 1
                    },
                    DecoderFallback = new DecoderReplacementFallback
                    {
                        DefaultString = ""�"",
                        MaxCharCount = 1
                    },
                    IsReadOnly = true,
                    CodePage = 65001
                },
                Length = 404,
                HasCompilationUnitRoot = true,
                Options = new CSharpParseOptions
                {
                    LanguageVersion = LanguageVersion.CSharp10,
                    PreprocessorSymbolNames = new string[0].ToImmutableArray(),
                    Language = ""C#"",
                    Features = new ImmutableDictionary<string, string>(),
                    DocumentationMode = DocumentationMode.Parse,
                    Errors = new Diagnostic[0].ToImmutableArray()
                },
                DiagnosticOptions = new ImmutableDictionary<string, ReportDiagnostic>(),
                Options = new CSharpParseOptions
                {
                    LanguageVersion = LanguageVersion.CSharp10,
                    PreprocessorSymbolNames = new string[0].ToImmutableArray(),
                    Language = ""C#"",
                    Features = new ImmutableDictionary<string, string>(),
                    DocumentationMode = DocumentationMode.Parse,
                    Errors = new Diagnostic[0].ToImmutableArray()
                }
            },
            IsInSource = true
        },
        AdditionalLocations = new List<Location>(),
        Descriptor = new DiagnosticDescriptor
        {
            Id = ""CS0103"",
            Title = new LocalizableResourceString(),
            Description = new LocalizableResourceString(),
            HelpLinkUri = ""https://msdn.microsoft.com/query/roslyn.query?appId=roslyn&k=k(CS0103)"",
            MessageFormat = new LocalizableResourceString(),
            Category = ""Compiler"",
            DefaultSeverity = DiagnosticSeverity.Error,
            IsEnabledByDefault = true,
            CustomTags = new string[]
            {
                ""Compiler"",
                ""Telemetry"",
                ""NotConfigurable""
            }.ToImmutableArray()
        },
        Id = ""CS0103"",
        Severity = DiagnosticSeverity.Error,
        DefaultSeverity = DiagnosticSeverity.Error,
        Info = new CSDiagnosticInfo
        {
            AdditionalLocations = new List<Location>(),
            Code = 103,
            Descriptor = new DiagnosticDescriptor
            {
                Id = ""CS0103"",
                Title = new LocalizableResourceString(),
                Description = new LocalizableResourceString(),
                HelpLinkUri = ""https://msdn.microsoft.com/query/roslyn.query?appId=roslyn&k=k(CS0103)"",
                MessageFormat = new LocalizableResourceString(),
                Category = ""Compiler"",
                DefaultSeverity = DiagnosticSeverity.Error,
                IsEnabledByDefault = true,
                CustomTags = new string[]
                {
                    ""Compiler"",
                    ""Telemetry"",
                    ""NotConfigurable""
                }.ToImmutableArray()
            },
            Severity = DiagnosticSeverity.Error,
            DefaultSeverity = DiagnosticSeverity.Error,
            Category = ""Compiler"",
            MessageIdentifier = ""CS0103""
        },
        Properties = new ImmutableDictionary<string, string>()
    }
};
", result);
        }

        [Fact]
        public void SerializeDiagnosticsVb()
        {
            var code =
        @"namespace Debuggable
        {
            public class HelloWorld
            {
                public string Greet(string name)
                {
                    var result = ""Hello, "" + name;
                    var anonymous = new { Name = ""Boris"" };
                    Console.WriteLine(result);
                    return result;
                }
            }
        }
        ";
            var failures = CreateAssembly(code, DefaultReferences);

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(failures, JsonConvert.SerializeObject(new
            {
                WritablePropertiesOnly = false
            }));

            Assert.Equal(
@"Dim whereArrayIteratorOfDiagnostic = New Diagnostic(){
    New CSDiagnostic With {
        .Location = New SourceLocation With {
            .Kind = LocationKind.SourceFile,
            .SourceSpan = New TextSpan With {
                .Start = 287,
                .[End] = 294,
                .Length = 7
            },
            .SourceTree = New ParsedSyntaxTree With {
                .FilePath = ""generated.cs"",
                .Encoding = New UTF8EncodingSealed With {
                    .Preamble = ""System.NotSupportedException: Specified method is not supported.""&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)&""   at System.Reflection.RuntimeMethodInfo.ThrowNoInvokeException()""&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)&""   at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)""&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)&""   at System.Reflection.RuntimePropertyInfo.GetValue(Object obj, BindingFlags invokeAttr, Binder binder, Object[] index, CultureInfo culture)""&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)&""   at System.Reflection.PropertyInfo.GetValue(Object obj)""&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)&""   at YellowFlavor.Serialization.Embedded.CodeDom.ObjectVisitor.GetValue(PropertyInfo propertyInfo, Object instance) in C:\Plays\ObjectDumper\src\Serialization\Embedded\CodeDom\ObjectVisitor.cs:line 827"",
                    .BodyName = ""utf-8"",
                    .EncodingName = ""Unicode (UTF-8)"",
                    .HeaderName = ""utf-8"",
                    .WebName = ""utf-8"",
                    .WindowsCodePage = 1200,
                    .IsBrowserDisplay = true,
                    .IsBrowserSave = true,
                    .IsMailNewsDisplay = true,
                    .IsMailNewsSave = true,
                    .EncoderFallback = New EncoderReplacementFallback With {
                        .DefaultString = ""�"",
                        .MaxCharCount = 1
                    },
                    .DecoderFallback = New DecoderReplacementFallback With {
                        .DefaultString = ""�"",
                        .MaxCharCount = 1
                    },
                    .IsReadOnly = true,
                    .CodePage = 65001
                },
                .Length = 404,
                .HasCompilationUnitRoot = true,
                .Options = New CSharpParseOptions With {
                    .LanguageVersion = LanguageVersion.CSharp10,
                    .PreprocessorSymbolNames = New String(-1) {}.ToImmutableArray(),
                    .Language = ""C#"",
                    .Features = New ImmutableDictionary(Of String, String)(),
                    .DocumentationMode = DocumentationMode.Parse,
                    .Errors = New Diagnostic(-1) {}.ToImmutableArray()
                },
                .DiagnosticOptions = New ImmutableDictionary(Of String, ReportDiagnostic)(),
                .Options = New CSharpParseOptions With {
                    .LanguageVersion = LanguageVersion.CSharp10,
                    .PreprocessorSymbolNames = New String(-1) {}.ToImmutableArray(),
                    .Language = ""C#"",
                    .Features = New ImmutableDictionary(Of String, String)(),
                    .DocumentationMode = DocumentationMode.Parse,
                    .Errors = New Diagnostic(-1) {}.ToImmutableArray()
                }
            },
            .IsInSource = true
        },
        .AdditionalLocations = New List(Of Location)(),
        .Descriptor = New DiagnosticDescriptor With {
            .Id = ""CS0103"",
            .Title = New LocalizableResourceString(),
            .Description = New LocalizableResourceString(),
            .HelpLinkUri = ""https://msdn.microsoft.com/query/roslyn.query?appId=roslyn&k=k(CS0103)"",
            .MessageFormat = New LocalizableResourceString(),
            .Category = ""Compiler"",
            .DefaultSeverity = DiagnosticSeverity.[Error],
            .IsEnabledByDefault = true,
            .CustomTags = New String(){
                ""Compiler"",
                ""Telemetry"",
                ""NotConfigurable""
            }.ToImmutableArray()
        },
        .Id = ""CS0103"",
        .Severity = DiagnosticSeverity.[Error],
        .DefaultSeverity = DiagnosticSeverity.[Error],
        .Info = New CSDiagnosticInfo With {
            .AdditionalLocations = New List(Of Location)(),
            .Code = 103,
            .Descriptor = New DiagnosticDescriptor With {
                .Id = ""CS0103"",
                .Title = New LocalizableResourceString(),
                .Description = New LocalizableResourceString(),
                .HelpLinkUri = ""https://msdn.microsoft.com/query/roslyn.query?appId=roslyn&k=k(CS0103)"",
                .MessageFormat = New LocalizableResourceString(),
                .Category = ""Compiler"",
                .DefaultSeverity = DiagnosticSeverity.[Error],
                .IsEnabledByDefault = true,
                .CustomTags = New String(){
                    ""Compiler"",
                    ""Telemetry"",
                    ""NotConfigurable""
                }.ToImmutableArray()
            },
            .Severity = DiagnosticSeverity.[Error],
            .DefaultSeverity = DiagnosticSeverity.[Error],
            .Category = ""Compiler"",
            .MessageIdentifier = ""CS0103""
        },
        .Properties = New ImmutableDictionary(Of String, String)()
    }
}
", result);

        }

        private static string runtimePath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1\{0}.dll";

        private static readonly IEnumerable<MetadataReference> DefaultReferences =
            new[]
            {
                MetadataReference.CreateFromFile(string.Format(runtimePath, "mscorlib")),
                MetadataReference.CreateFromFile(string.Format(runtimePath, "System")),
                MetadataReference.CreateFromFile(string.Format(runtimePath, "System.Core"))
            };

        private static IEnumerable<Diagnostic> CreateAssembly(string code, IEnumerable<MetadataReference> references)
        {
            var encoding = Encoding.UTF8;

            var assemblyName = Path.GetRandomFileName();
            var symbolsName = Path.ChangeExtension(assemblyName, "pdb");
            var sourceCodePath = "generated.cs";

            var buffer = encoding.GetBytes(code);
            var sourceText = SourceText.From(buffer, buffer.Length, encoding, canBeEmbedded: true);

            var syntaxTree = CSharpSyntaxTree.ParseText(
                sourceText,
                new CSharpParseOptions(),
                path: sourceCodePath);

            var syntaxRootNode = syntaxTree.GetRoot() as CSharpSyntaxNode;
            var encoded = CSharpSyntaxTree.Create(syntaxRootNode, null, sourceCodePath, encoding);

            var optimizationLevel = OptimizationLevel.Debug;

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { encoded },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOptimizationLevel(optimizationLevel)
                    .WithPlatform(Platform.AnyCpu)
            );

            using var assemblyStream = new MemoryStream();
            using var symbolsStream = new MemoryStream();
            var emitOptions = new EmitOptions(
                debugInformationFormat: DebugInformationFormat.PortablePdb,
                pdbFilePath: symbolsName);

            var embeddedTexts = new List<EmbeddedText>
            {
                EmbeddedText.FromSource(sourceCodePath, sourceText),
            };

            EmitResult result = compilation.Emit(
                peStream: assemblyStream,
                pdbStream: symbolsStream,
                embeddedTexts: embeddedTexts,
                options: emitOptions);

            if (!result.Success)
            {
                var errors = new List<string>();

                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                return failures;
            }

            return null;
            //Console.WriteLine(code);

            //assemblyStream.Seek(0, SeekOrigin.Begin);
            //symbolsStream?.Seek(0, SeekOrigin.Begin);

            //var assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream, symbolsStream);
            //return assembly;
        }

    }
}