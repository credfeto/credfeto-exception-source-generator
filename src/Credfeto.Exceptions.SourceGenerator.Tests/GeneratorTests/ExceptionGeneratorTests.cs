using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Credfeto.Exceptions.SourceGenerator.Tests.GeneratorTests;

public sealed class ExceptionGeneratorTests : TestBase
{
    private static async Task<IReadOnlyList<string>> RunGeneratorAsync(
        string source,
        CancellationToken cancellationToken
    )
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);

        IEnumerable<MetadataReference> references = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        ExceptionGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)
            driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _, cancellationToken);

        GeneratorDriverRunResult result = driver.GetRunResult();

        List<string> texts = [];

        foreach (SyntaxTree generatedTree in result.GeneratedTrees)
        {
            Microsoft.CodeAnalysis.Text.SourceText text = await generatedTree.GetTextAsync(cancellationToken);
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

        IReadOnlyList<string> generated = await RunGeneratorAsync(source, TestContext.Current.CancellationToken);

        Assert.Single(generated);

        string code = generated[0];
        Assert.Contains("public ExampleException()", code, StringComparison.Ordinal);
        Assert.Contains("public ExampleException(string? message)", code, StringComparison.Ordinal);
        Assert.Contains(
            "public ExampleException(string? message, Exception? innerException)",
            code,
            StringComparison.Ordinal
        );
        Assert.Contains(": base(message)", code, StringComparison.Ordinal);
        Assert.Contains(": base(message: message, innerException: innerException)", code, StringComparison.Ordinal);
        Assert.Contains("namespace MyApp;", code, StringComparison.Ordinal);
        Assert.Contains("public sealed partial class ExampleException", code, StringComparison.Ordinal);
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

        IReadOnlyList<string> generated = await RunGeneratorAsync(source, TestContext.Current.CancellationToken);

        Assert.Single(generated);

        string code = generated[0];
        Assert.Contains(""": this("Hello World")""", code, StringComparison.Ordinal);
        Assert.Contains("public ExampleException(string? message)", code, StringComparison.Ordinal);
        Assert.Contains(
            "public ExampleException(string? message, Exception? innerException)",
            code,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(": this(\"Hello World\")\r\n    { }", code, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublicNonSealedExceptionGeneratesWithoutSealedModifier()
    {
        const string source = """
            using System;

            namespace MyApp;

            public partial class BaseException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(source, TestContext.Current.CancellationToken);

        Assert.Single(generated);

        string code = generated[0];
        Assert.Contains("public partial class BaseException : Exception", code, StringComparison.Ordinal);
        Assert.DoesNotContain("sealed", code, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InternalSealedExceptionGeneratesCorrectModifiers()
    {
        const string source = """
            using System;

            namespace MyApp;

            internal partial sealed class InternalException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(source, TestContext.Current.CancellationToken);

        Assert.Single(generated);

        string code = generated[0];
        Assert.Contains("internal sealed partial class InternalException : Exception", code, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NonExceptionClassIsIgnored()
    {
        const string source = """
            namespace MyApp;

            public partial class NotAnException;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(source, TestContext.Current.CancellationToken);

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

        IReadOnlyList<string> generated = await RunGeneratorAsync(source, TestContext.Current.CancellationToken);

        Assert.Empty(generated);
    }

    [Fact]
    public async Task ExceptionClassInGlobalNamespaceGeneratesWithoutNamespaceDeclaration()
    {
        const string source = """
            using System;

            public partial sealed class GlobalException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(source, TestContext.Current.CancellationToken);

        Assert.Single(generated);

        string code = generated[0];
        Assert.DoesNotContain("namespace ", code, StringComparison.Ordinal);
        Assert.Contains("public sealed partial class GlobalException : Exception", code, StringComparison.Ordinal);
        Assert.Contains("public GlobalException()", code, StringComparison.Ordinal);
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

        IReadOnlyList<string> generated = await RunGeneratorAsync(source, TestContext.Current.CancellationToken);

        Assert.Single(generated);

        string code = generated[0];
        Assert.Contains(@": this(""Say \""hello\"" world"")", code, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GeneratedCodeContainsAutoGeneratedHeader()
    {
        const string source = """
            using System;

            namespace MyApp;

            public partial sealed class ExampleException : Exception;
            """;

        IReadOnlyList<string> generated = await RunGeneratorAsync(source, TestContext.Current.CancellationToken);

        Assert.Single(generated);

        string code = generated[0];
        Assert.StartsWith("// <auto-generated/>", code, StringComparison.Ordinal);
        Assert.Contains("#nullable enable", code, StringComparison.Ordinal);
    }
}
