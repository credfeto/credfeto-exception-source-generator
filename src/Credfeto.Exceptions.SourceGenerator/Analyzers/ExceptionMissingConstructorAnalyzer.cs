using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Credfeto.Exceptions.SourceGenerator.Analyzers;

/// <summary>
///     Diagnostic analyzer that identifies non-partial exception classes whose constructors could be source-generated.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionMissingConstructorAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     The diagnostic ID reported when an exception class has standard constructors that can be source-generated.
    /// </summary>
    public const string DiagnosticId = "EXCGEN001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Exception class can use source-generated constructors",
        messageFormat: "Exception class '{0}' has standard constructors that can be source-generated; consider making the class partial",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Exception classes with standard constructors should use the Credfeto.Exceptions.SourceGenerator source generator to reduce boilerplate."
    );

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        ClassDeclarationSyntax classDecl = (ClassDeclarationSyntax)context.Node;

        if (classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }

        if (
            context.SemanticModel.GetDeclaredSymbol(classDecl, context.CancellationToken) is not INamedTypeSymbol symbol
        )
        {
            return;
        }

        if (!IsExceptionDerivedType(symbol, context.SemanticModel.Compilation, context.CancellationToken))
        {
            return;
        }

        if (!HasAnyStandardConstructor(symbol))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(descriptor: Rule, location: classDecl.Identifier.GetLocation(), messageArgs: symbol.Name)
        );
    }

    private static bool IsExceptionDerivedType(
        INamedTypeSymbol symbol,
        Compilation compilation,
        CancellationToken cancellationToken
    )
    {
        INamedTypeSymbol? exceptionType = compilation.GetTypeByMetadataName("System.Exception");

        if (exceptionType is null)
        {
            return false;
        }

        INamedTypeSymbol? baseType = symbol.BaseType;

        while (baseType is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (SymbolEqualityComparer.Default.Equals(baseType, exceptionType))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static bool HasAnyStandardConstructor(INamedTypeSymbol symbol)
    {
        return symbol.Constructors.Any(c =>
            IsDefaultConstructor(c) || IsMessageConstructor(c) || IsMessageAndInnerExceptionConstructor(c)
        );
    }

    private static bool IsDefaultConstructor(IMethodSymbol method)
    {
        return !method.IsStatic && method.Parameters.Length == 0;
    }

    private static bool IsMessageConstructor(IMethodSymbol method)
    {
        if (method.IsStatic || method.Parameters.Length != 1)
        {
            return false;
        }

        IParameterSymbol param = method.Parameters[0];

        return param.Type.SpecialType == SpecialType.System_String
            && string.Equals(param.Name, "message", StringComparison.Ordinal);
    }

    private static bool IsMessageAndInnerExceptionConstructor(IMethodSymbol method)
    {
        if (method.IsStatic || method.Parameters.Length != 2)
        {
            return false;
        }

        IParameterSymbol messageParam = method.Parameters[0];
        IParameterSymbol innerExceptionParam = method.Parameters[1];

        return messageParam.Type.SpecialType == SpecialType.System_String
            && string.Equals(messageParam.Name, "message", StringComparison.Ordinal)
            && string.Equals(innerExceptionParam.Type.ToDisplayString(), "System.Exception?", StringComparison.Ordinal)
            && string.Equals(innerExceptionParam.Name, "innerException", StringComparison.Ordinal);
    }
}
