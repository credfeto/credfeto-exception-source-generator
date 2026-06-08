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

        string source = ExceptionCodeBuilder.Build(info);

        Assert.Contains(
            ": this(\"Something \\\"quoted\\\" and slash \\\\ and newline \\n tab \\t\")",
            source,
            StringComparison.Ordinal
        );
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

        string source = ExceptionCodeBuilder.Build(info);

        Assert.Contains("protected AbstractSampleException() { }", source, StringComparison.Ordinal);
        Assert.Contains("protected AbstractSampleException(string? message)", source, StringComparison.Ordinal);
        Assert.Contains(
            "protected AbstractSampleException(string? message, Exception? innerException)",
            source,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(": this(\"", source, StringComparison.Ordinal);
    }
}
