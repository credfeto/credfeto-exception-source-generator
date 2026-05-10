using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Credfeto.Exceptions.SourceGenerator.Tests.GeneratorTests;

public sealed class ExceptionGeneratorTests : TestBase
{
    private static IReadOnlyList<string> RunGenerator(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
                                                             .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                                                             .Select(a => MetadataReference.CreateFromFile(a.Location))
                                                             .Cast<MetadataReference>();

        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName: "TestAssembly",
                                                                 syntaxTrees: [syntaxTree],
                                                                 references: references,
                                                                 options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        ExceptionGenerator generator = new();
        CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        GeneratorDriverRunResult result = driver.GetRunResult();

        return result.GeneratedTrees.Select(t => t.GetText()
                                                  .ToString())
                     .ToList();
    }

    [Fact]
    public void SimplePublicSealedExceptionGeneratesThreeConstructors()
    {
        const string source = """
                              using System;

                              namespace MyApp;

                              public partial sealed class ExampleException : Exception;
                              """;

        IReadOnlyList<string> generated = RunGenerator(source);

        Assert.Single(generated);

        string code = generated[0];
        Assert.Contains("public ExampleException()", code);
        Assert.Contains("public ExampleException(string? message)", code);
        Assert.Contains("public ExampleException(string? message, Exception? innerException)", code);
        Assert.Contains(": base(message)", code);
        Assert.Contains(": base(message: message, innerException: innerException)", code);
        Assert.Contains("namespace MyApp;", code);
        Assert.Contains("public sealed partial class ExampleException", code);
    }

    [Fact]
    public void ExceptionWithDescriptionAttributeGeneratesThisCallInDefaultConstructor()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              namespace MyApp;

                              [Description("Hello World")]
                              public partial sealed class ExampleException : Exception;
                              """;

        IReadOnlyList<string> generated = RunGenerator(source);

        Assert.Single(generated);

        string code = generated[0];
        Assert.Contains(""": this("Hello World")""", code);
        Assert.Contains("public ExampleException(string? message)", code);
        Assert.Contains("public ExampleException(string? message, Exception? innerException)", code);
        Assert.DoesNotContain(": this(\"Hello World\")\r\n    { }", code);
    }

    [Fact]
    public void PublicNonSealedExceptionGeneratesWithoutSealedModifier()
    {
        const string source = """
                              using System;

                              namespace MyApp;

                              public partial class BaseException : Exception;
                              """;

        IReadOnlyList<string> generated = RunGenerator(source);

        Assert.Single(generated);

        string code = generated[0];
        Assert.Contains("public partial class BaseException : Exception", code);
        Assert.DoesNotContain("sealed", code);
    }

    [Fact]
    public void InternalSealedExceptionGeneratesCorrectModifiers()
    {
        const string source = """
                              using System;

                              namespace MyApp;

                              internal partial sealed class InternalException : Exception;
                              """;

        IReadOnlyList<string> generated = RunGenerator(source);

        Assert.Single(generated);

        string code = generated[0];
        Assert.Contains("internal sealed partial class InternalException : Exception", code);
    }

    [Fact]
    public void NonExceptionClassIsIgnored()
    {
        const string source = """
                              namespace MyApp;

                              public partial class NotAnException;
                              """;

        IReadOnlyList<string> generated = RunGenerator(source);

        Assert.Empty(generated);
    }

    [Fact]
    public void NonPartialExceptionClassIsIgnored()
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

        IReadOnlyList<string> generated = RunGenerator(source);

        Assert.Empty(generated);
    }

    [Fact]
    public void ExceptionClassInGlobalNamespaceGeneratesWithoutNamespaceDeclaration()
    {
        const string source = """
                              using System;

                              public partial sealed class GlobalException : Exception;
                              """;

        IReadOnlyList<string> generated = RunGenerator(source);

        Assert.Single(generated);

        string code = generated[0];
        Assert.DoesNotContain("namespace ", code);
        Assert.Contains("public sealed partial class GlobalException : Exception", code);
        Assert.Contains("public GlobalException()", code);
    }

    [Fact]
    public void ExceptionWithSpecialCharactersInDescriptionIsEscaped()
    {
        const string source = """
                              using System;
                              using System.ComponentModel;

                              namespace MyApp;

                              [Description("Say \"hello\" world")]
                              public partial sealed class QuotedException : Exception;
                              """;

        IReadOnlyList<string> generated = RunGenerator(source);

        Assert.Single(generated);

        string code = generated[0];
        Assert.Contains(@": this(""Say \""hello\"" world"")", code);
    }

    [Fact]
    public void GeneratedCodeContainsAutoGeneratedHeader()
    {
        const string source = """
                              using System;

                              namespace MyApp;

                              public partial sealed class ExampleException : Exception;
                              """;

        IReadOnlyList<string> generated = RunGenerator(source);

        Assert.Single(generated);

        string code = generated[0];
        Assert.StartsWith("// <auto-generated/>", code);
        Assert.Contains("#nullable enable", code);
    }
}
