using System.Threading;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Credfeto.Exceptions.SourceGenerator;

/// <summary>
///     Incremental source generator that generates standard exception constructors for partial exception classes.
/// </summary>
[Generator]
public sealed class ExceptionGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ExceptionInfo?> exceptionInfos = context.SyntaxProvider.CreateSyntaxProvider(
                                                                                             predicate: static (node, _) => IsPartialClassWithBase(node),
                                                                                             transform: static (ctx, token) => GetExceptionInfo(ctx, token))
                                                                                         .Where(static info => info is not null);

        context.RegisterSourceOutput(exceptionInfos, static (spc, info) => Execute(spc, info!));
    }

    private static bool IsPartialClassWithBase(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { BaseList: not null } classDecl && classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    private static ExceptionInfo? GetExceptionInfo(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        ClassDeclarationSyntax classDecl = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDecl, cancellationToken) is not INamedTypeSymbol symbol)
        {
            return null;
        }

        if (!IsExceptionDerivedType(symbol))
        {
            return null;
        }

        string? description = GetDescriptionAttribute(symbol);

        string? namespaceName = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        return new ExceptionInfo(Namespace: namespaceName,
                                 ClassName: symbol.Name,
                                 AccessModifier: GetAccessModifier(symbol.DeclaredAccessibility),
                                 IsSealed: symbol.IsSealed,
                                 Description: description);
    }

    private static bool IsExceptionDerivedType(INamedTypeSymbol symbol)
    {
        INamedTypeSymbol? baseType = symbol.BaseType;

        while (baseType is not null)
        {
            if (baseType.ToDisplayString() == "System.Exception")
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static string? GetDescriptionAttribute(INamedTypeSymbol symbol)
    {
        foreach (AttributeData attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == "System.ComponentModel.DescriptionAttribute" &&
                attribute.ConstructorArguments.Length > 0 &&
                attribute.ConstructorArguments[0].Value is string description)
            {
                return description;
            }
        }

        return null;
    }

    private static string GetAccessModifier(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            _ => "public"
        };
    }

    private static void Execute(SourceProductionContext context, ExceptionInfo info)
    {
        string source = ExceptionCodeBuilder.Build(info);
        context.AddSource($"{info.ClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
    }
}
