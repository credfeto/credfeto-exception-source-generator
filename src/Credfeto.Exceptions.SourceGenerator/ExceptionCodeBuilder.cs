using Credfeto.SourceGeneration.Helpers.Builders;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Credfeto.Exceptions.SourceGenerator;

public static class ExceptionCodeBuilder
{
    public static SourceText Build(in ExceptionInfo info)
    {
        CodeBuilder source = new CodeBuilder()
            .AppendFileHeader()
            .AppendLine("#nullable enable")
            .AppendBlankLine()
            .AppendLine("using System;")
            .AppendLine("using System.CodeDom.Compiler;")
            .AppendBlankLine()
            .AppendNamespace(info.Namespace)
            .AppendGeneratedCodeAttribute(tool: "Credfeto.Exceptions.CodeGenerator", version: "0.0.1.0");

        using (source.StartBlock(ClassDeclaration(info)))
        {
            source = source
                .AppendDefaultConstructor(info)
                .AppendBlankLine()
                .AppendMessageConstructor(info)
                .AppendBlankLine()
                .AppendMessageAndInnerExceptionConstructor(info);
        }

        return source.Text;
    }

    private static CodeBuilder AppendNamespace(this CodeBuilder source, string? namespaceName)
    {
        return namespaceName is null ? source : source.AppendLine("namespace " + namespaceName + ";").AppendBlankLine();
    }

    private static string ClassDeclaration(in ExceptionInfo info)
    {
        string abstractModifier = info.IsAbstract ? " abstract" : string.Empty;
        string sealedModifier = info.IsSealed ? " sealed" : string.Empty;

        return info.AccessModifier + abstractModifier + sealedModifier + " partial class " + info.ClassName;
    }

    private static CodeBuilder AppendDefaultConstructor(this CodeBuilder source, in ExceptionInfo info)
    {
        string chainedCall = info.Description is null
            ? ": base()"
            : ": this(" + SymbolDisplay.FormatLiteral(info.Description, quote: true) + ")";

        return source.AppendConstructor(info: info, parameters: string.Empty, chainedCall: chainedCall);
    }

    private static CodeBuilder AppendMessageConstructor(this CodeBuilder source, in ExceptionInfo info)
    {
        return source.AppendConstructor(info: info, parameters: "string? message", chainedCall: ": base(message)");
    }

    private static CodeBuilder AppendMessageAndInnerExceptionConstructor(this CodeBuilder source, in ExceptionInfo info)
    {
        return source.AppendConstructor(
            info: info,
            parameters: "string? message, Exception? innerException",
            chainedCall: ": base(message: message, innerException: innerException)"
        );
    }

    private static CodeBuilder AppendConstructor(
        this CodeBuilder source,
        in ExceptionInfo info,
        string parameters,
        string chainedCall
    )
    {
        source
            .AppendLine(ConstructorAccess(info) + " " + info.ClassName + "(" + parameters + ")")
            .StartBlock("    " + chainedCall)
            .Dispose();

        return source;
    }

    private static string ConstructorAccess(in ExceptionInfo info)
    {
        return info.IsAbstract ? "protected" : "public";
    }
}
