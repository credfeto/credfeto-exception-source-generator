using System;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Exceptions.SourceGenerator.Tests;

public sealed class ExceptionCodeBuilderTests : TestBase
{
    [Fact]
    public void Build_DescriptionContainsSpecialCharacters_EscapesDescriptionInGeneratedConstructor()
    {
        const string description = "Something \"quoted\" and slash \\ and newline \n tab \t";

        ExceptionInfo info = new(
            namespaceName: "Example.Namespace",
            className: "SampleException",
            accessModifier: "public",
            isSealed: false,
            isAbstract: false,
            description: description
        );

        string source = ExceptionCodeBuilder.Build(info).ToString();

        Assert.Contains(
            ": this(\"Something \\\"quoted\\\" and slash \\\\ and newline \\n tab \\t\")",
            source,
            StringComparison.Ordinal
        );
    }

    [Fact]
    public void Build_SealedExceptionWithDescription_GeneratesExpectedSource()
    {
        ExceptionInfo info = new(
            namespaceName: "Example.Namespace",
            className: "SampleException",
            accessModifier: "internal",
            isSealed: true,
            isAbstract: false,
            description: "Something went wrong"
        );

        string source = ExceptionCodeBuilder.Build(info).ToString();

        string expected = string.Join(
            separator: Environment.NewLine,
            "namespace Example.Namespace;",
            "",
            "[GeneratedCode(tool: \"Credfeto.Exceptions.CodeGenerator\", version: \"0.0.1.0\")]",
            "internal sealed partial class SampleException",
            "{",
            "    public SampleException()",
            "        : this(\"Something went wrong\")",
            "    {",
            "    }",
            "",
            "    public SampleException(string? message)",
            "        : base(message)",
            "    {",
            "    }",
            "",
            "    public SampleException(string? message, Exception? innerException)",
            "        : base(message: message, innerException: innerException)",
            "    {",
            "    }",
            "}",
            ""
        );

        Assert.EndsWith(expected, source, StringComparison.Ordinal);
        Assert.StartsWith(
            "//------------------------------------------------------------------------------",
            source,
            StringComparison.Ordinal
        );
        Assert.Contains("#nullable enable", source, StringComparison.Ordinal);
        Assert.Contains("using System.CodeDom.Compiler;", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_GlobalNamespace_OmitsNamespaceDeclaration()
    {
        ExceptionInfo info = new(
            namespaceName: null,
            className: "GlobalException",
            accessModifier: "public",
            isSealed: false,
            isAbstract: false,
            description: null
        );

        string source = ExceptionCodeBuilder.Build(info).ToString();

        Assert.DoesNotContain("namespace ", source, StringComparison.Ordinal);
        Assert.Contains("public partial class GlobalException", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_AbstractExceptionWithoutDescription_UsesProtectedConstructors()
    {
        ExceptionInfo info = new(
            namespaceName: "Example.Namespace",
            className: "AbstractSampleException",
            accessModifier: "public",
            isSealed: false,
            isAbstract: true,
            description: null
        );

        string source = ExceptionCodeBuilder.Build(info).ToString();

        Assert.Contains("public abstract partial class AbstractSampleException", source, StringComparison.Ordinal);
        Assert.Contains("protected AbstractSampleException()", source, StringComparison.Ordinal);
        Assert.Contains("protected AbstractSampleException(string? message)", source, StringComparison.Ordinal);
        Assert.Contains(
            "protected AbstractSampleException(string? message, Exception? innerException)",
            source,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(": this(\"", source, StringComparison.Ordinal);
    }
}
