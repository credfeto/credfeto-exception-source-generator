using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Credfeto.Exceptions.SourceGenerator.Tests.GeneratorTests;

public sealed class ExceptionGeneratorTests : TestBase
{
    private static async Task<IReadOnlyList<string>> RunGeneratorAsync(
        string source,
        CancellationToken cancellationToken
    )
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(text: source, cancellationToken: cancellationToken);

        IEnumerable<MetadataReference> references = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location));

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            [syntaxTree],
            references: references,
            new(OutputKind.DynamicallyLinkedLibrary)
        );

        ExceptionGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out _,
                cancellationToken: cancellationToken
            );

        GeneratorDriverRunResult result = driver.GetRunResult();

        List<string> texts = [];

        foreach (SyntaxTree generatedTree in result.GeneratedTrees)
        {
            SourceText text = await generatedTree.GetTextAsync(cancellationToken);
            texts.Add(text.ToString());
        }

        return texts;
    }

    [Fact]
    public async Task SimplePublicSealedExceptionGeneratesThreeConstructors()
    {
        const string source = """
            using System;

            namespace MyApp;

            public partial sealed class ExampleException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(
            source: source,
            cancellationToken: TestContext.Current.CancellationToken
        );

        string code = Assert.Single(generated);
        Assert.Contains(
            expectedSubstring: "public ExampleException()",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "public ExampleException(string? message)",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "public ExampleException(string? message, Exception? innerException)",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: ": base(message)",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: ": base(message: message, innerException: innerException)",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "namespace MyApp;",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "public sealed partial class ExampleException",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public async Task ExceptionWithDescriptionAttributeGeneratesThisCallInDefaultConstructor()
    {
        const string source = """
            using System;
            using System.ComponentModel;

            namespace MyApp;

            [Description("Hello World")]
            public partial sealed class ExampleException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(
            source: source,
            cancellationToken: TestContext.Current.CancellationToken
        );

        string code = Assert.Single(generated);
        Assert.Contains(
            expectedSubstring: """: this("Hello World")""",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "public ExampleException(string? message)",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "public ExampleException(string? message, Exception? innerException)",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            expectedSubstring: ": this(\"Hello World\")\r\n    { }",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public async Task PublicNonSealedExceptionGeneratesWithoutSealedModifier()
    {
        const string source = """
            using System;

            namespace MyApp;

            public partial class BaseException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(
            source: source,
            cancellationToken: TestContext.Current.CancellationToken
        );

        string code = Assert.Single(generated);
        Assert.Contains(
            expectedSubstring: "public partial class BaseException : Exception",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            expectedSubstring: "sealed",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public async Task PublicAbstractExceptionGeneratesWithAbstractModifierAndProtectedConstructors()
    {
        const string source = """
            using System;

            namespace MyApp;

            public abstract partial class BaseException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(
            source: source,
            cancellationToken: TestContext.Current.CancellationToken
        );

        string code = Assert.Single(generated);
        Assert.Contains(
            expectedSubstring: "public abstract partial class BaseException : Exception",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "protected BaseException()",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "protected BaseException(string? message)",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "protected BaseException(string? message, Exception? innerException)",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            expectedSubstring: "public BaseException()",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public async Task InternalSealedExceptionGeneratesCorrectModifiers()
    {
        const string source = """
            using System;

            namespace MyApp;

            internal partial sealed class InternalException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(
            source: source,
            cancellationToken: TestContext.Current.CancellationToken
        );

        string code = Assert.Single(generated);
        Assert.Contains(
            expectedSubstring: "internal sealed partial class InternalException : Exception",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public async Task NonExceptionClassIsIgnored()
    {
        const string source = """
            namespace MyApp;

            public partial class NotAnException;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(
            source: source,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Empty(generated);
    }

    [Fact]
    public async Task NonPartialExceptionClassIsIgnored()
    {
        const string source = """
            using System;

            namespace MyApp;

            public sealed class NonPartialException : Exception
            {
                public NonPartialException() { }

                public NonPartialException(string? message)
                    : base(message) { }

                public NonPartialException(string? message, Exception? innerException)
                    : base(message: message, innerException: innerException) { }
            }
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(
            source: source,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Empty(generated);
    }

    [Fact]
    public async Task ExceptionClassInGlobalNamespaceGeneratesWithoutNamespaceDeclaration()
    {
        const string source = """
            using System;

            public partial sealed class GlobalException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(
            source: source,
            cancellationToken: TestContext.Current.CancellationToken
        );

        string code = Assert.Single(generated);
        Assert.DoesNotContain(
            expectedSubstring: "namespace ",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "public sealed partial class GlobalException : Exception",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "public GlobalException()",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public async Task ExceptionWithSpecialCharactersInDescriptionIsEscaped()
    {
        const string source = """
            using System;
            using System.ComponentModel;

            namespace MyApp;

            [Description("Say \"hello\" world")]
            public partial sealed class QuotedException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(
            source: source,
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Single(generated);

        string code = Assert.Single(generated);
        Assert.Contains(
            expectedSubstring: @": this(""Say \""hello\"" world"")",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public async Task GeneratedCodeContainsAutoGeneratedHeader()
    {
        const string source = """
            using System;

            namespace MyApp;

            public partial sealed class ExampleException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(
            source: source,
            cancellationToken: TestContext.Current.CancellationToken
        );

        string code = Assert.Single(generated);
        Assert.StartsWith(
            expectedStartString: "// <auto-generated/>",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
        Assert.Contains(
            expectedSubstring: "[GeneratedCode(",
            actualString: code,
            comparisonType: StringComparison.Ordinal
        );
    }
}
