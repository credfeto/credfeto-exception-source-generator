using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Credfeto.Exceptions.SourceGenerator.Tests;

public sealed class ExceptionGeneratorTests : TestBase
{
    [Fact]
    public void Generate_PartialExceptionWithDescription_ChainsDefaultConstructorToDescription()
    {
        const string source = """
            using System;
            using System.ComponentModel;

            namespace Example.Namespace;

            [Description("Something went wrong")]
            public sealed partial class SampleException : Exception
            {
            }
            """;

        GeneratedSourceResult generated = RunGenerator(source);

        Assert.Equal(expected: "SampleException.g.cs", actual: generated.HintName);
        Assert.Contains(": this(\"Something went wrong\")", generated.SourceText.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Generate_PartialExceptionWithoutDescription_ChainsDefaultConstructorToBase()
    {
        const string source = """
            using System;

            namespace Example.Namespace;

            public partial class PlainException : Exception
            {
            }
            """;

        GeneratedSourceResult generated = RunGenerator(source);

        string text = generated.SourceText.ToString();
        Assert.Equal(expected: "PlainException.g.cs", actual: generated.HintName);
        Assert.Contains(": base()", text, StringComparison.Ordinal);
        Assert.DoesNotContain(": this(\"", text, StringComparison.Ordinal);
    }

    private static GeneratedSourceResult RunGenerator(string source)
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Example",
            syntaxTrees: [CSharpSyntaxTree.ParseText(text: source, cancellationToken: cancellationToken)],
            references: GetPlatformReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        GeneratorDriverRunResult result = CSharpGeneratorDriver
            .Create(new ExceptionGenerator())
            .RunGenerators(compilation: compilation, cancellationToken: cancellationToken)
            .GetRunResult();

        Assert.Empty(result.Diagnostics);

        return Assert.Single(Assert.Single(result.Results).GeneratedSources);
    }

    private static ImmutableArray<MetadataReference> GetPlatformReferences()
    {
        string assemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;

        return
        [
            .. assemblies
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(path => MetadataReference.CreateFromFile(path)),
        ];
    }
}
