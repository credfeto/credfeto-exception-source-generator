using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Exceptions.SourceGenerator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Exceptions.SourceGenerator.CodeFixes;

/// <summary>
///     Code fix provider that converts a non-partial exception class to use the source generator.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExceptionCodeFixProvider))]
[Shared]
public sealed class ExceptionCodeFixProvider : CodeFixProvider
{
    private const string Title = "Convert to source-generated exception constructors";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [ExceptionMissingConstructorAnalyzer.DiagnosticId];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);

        if (root is null)
        {
            return;
        }

        Diagnostic diagnostic = context.Diagnostics[0];
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        ClassDeclarationSyntax? classDecl = root.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDecl is null)
        {
            return;
        }

        context.RegisterCodeFix(
            action: CodeAction.Create(
                title: Title,
                createChangedDocument: ct => ConvertToPartialAsync(context.Document, classDecl, ct),
                equivalenceKey: Title
            ),
            diagnostic: diagnostic
        );
    }

    private static async Task<Document> ConvertToPartialAsync(
        Document document,
        ClassDeclarationSyntax classDecl,
        CancellationToken cancellationToken
    )
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken);

        if (root is null)
        {
            return document;
        }

        SyntaxToken partialToken = SyntaxFactory
            .Token(SyntaxKind.PartialKeyword)
            .WithTrailingTrivia(SyntaxFactory.Space);

        SyntaxTokenList updatedModifiers = classDecl.Modifiers.Add(partialToken);

        SyntaxList<MemberDeclarationSyntax> updatedMembers = RemoveStandardConstructors(classDecl.Members);

        ClassDeclarationSyntax updatedClassDecl = classDecl.WithModifiers(updatedModifiers).WithMembers(updatedMembers);

        SyntaxNode updatedRoot = root.ReplaceNode(classDecl, updatedClassDecl);

        return document.WithSyntaxRoot(updatedRoot);
    }

    private static SyntaxList<MemberDeclarationSyntax> RemoveStandardConstructors(
        in SyntaxList<MemberDeclarationSyntax> members
    )
    {
        SyntaxList<MemberDeclarationSyntax> result = members;

        foreach (MemberDeclarationSyntax member in members)
        {
            if (member is ConstructorDeclarationSyntax ctor && IsStandardConstructor(ctor))
            {
                result = result.Remove(ctor);
            }
        }

        return result;
    }

    private static bool IsStandardConstructor(ConstructorDeclarationSyntax ctor)
    {
        return IsDefaultConstructor(ctor) || IsMessageConstructor(ctor) || IsMessageAndInnerExceptionConstructor(ctor);
    }

    private static bool IsDefaultConstructor(ConstructorDeclarationSyntax ctor)
    {
        return ctor.ParameterList.Parameters.Count == 0;
    }

    private static bool IsMessageConstructor(ConstructorDeclarationSyntax ctor)
    {
        if (ctor.ParameterList.Parameters.Count != 1)
        {
            return false;
        }

        ParameterSyntax param = ctor.ParameterList.Parameters[0];

        return string.Equals(param.Identifier.Text, "message", StringComparison.Ordinal);
    }

    private static bool IsMessageAndInnerExceptionConstructor(ConstructorDeclarationSyntax ctor)
    {
        if (ctor.ParameterList.Parameters.Count != 2)
        {
            return false;
        }

        ParameterSyntax messageParam = ctor.ParameterList.Parameters[0];
        ParameterSyntax innerExceptionParam = ctor.ParameterList.Parameters[1];

        return string.Equals(messageParam.Identifier.Text, "message", StringComparison.Ordinal)
            && string.Equals(innerExceptionParam.Identifier.Text, "innerException", StringComparison.Ordinal);
    }
}
