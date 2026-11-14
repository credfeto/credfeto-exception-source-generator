using Credfeto.Exceptions.SourceGenerator.Analyzers;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Exceptions.SourceGenerator.Tests;

public sealed class ExceptionMissingConstructorAnalyzerTests : TestBase
{
    [Fact]
    public void SupportedDiagnostics_ContainsOnlyEnabledInfoRule()
    {
        ExceptionMissingConstructorAnalyzer analyzer = new();

        DiagnosticDescriptor descriptor = Assert.Single(analyzer.SupportedDiagnostics);

        Assert.Equal(expected: ExceptionMissingConstructorAnalyzer.DiagnosticId, actual: descriptor.Id);
        Assert.Equal(expected: DiagnosticSeverity.Info, actual: descriptor.DefaultSeverity);
        Assert.True(condition: descriptor.IsEnabledByDefault, userMessage: "Rule should be enabled by default");
    }
}
