using System.Diagnostics;

namespace Credfeto.Exceptions.SourceGenerator;

/// <summary>
///     Holds the information extracted from a partial exception class declaration needed to generate constructors.
/// </summary>
[DebuggerDisplay("{Namespace,nq}.{ClassName,nq} ({AccessModifier,nq}) Sealed={IsSealed}")]
internal readonly struct ExceptionInfo
{
    /// <summary>
    ///     Initialises a new instance of the <see cref="ExceptionInfo" /> struct.
    /// </summary>
    /// <param name="namespaceName">The namespace of the class, or <see langword="null" /> if the class is in the global namespace.</param>
    /// <param name="className">The simple name of the exception class.</param>
    /// <param name="accessModifier">The access modifier keyword (e.g. <c>public</c>, <c>internal</c>).</param>
    /// <param name="isSealed">Whether the class is declared <c>sealed</c>.</param>
    /// <param name="description">The value from a <c>[Description]</c> attribute, or <see langword="null" /> if absent.</param>
    public ExceptionInfo(
        string? namespaceName,
        string className,
        string accessModifier,
        bool isSealed,
        string? description
    )
    {
        this.Namespace = namespaceName;
        this.ClassName = className;
        this.AccessModifier = accessModifier;
        this.IsSealed = isSealed;
        this.Description = description;
    }

    /// <summary>Gets the namespace of the class, or <see langword="null" /> if in the global namespace.</summary>
    public string? Namespace { get; }

    /// <summary>Gets the simple name of the exception class.</summary>
    public string ClassName { get; }

    /// <summary>Gets the access modifier keyword.</summary>
    public string AccessModifier { get; }

    /// <summary>Gets a value indicating whether the class is sealed.</summary>
    public bool IsSealed { get; }

    /// <summary>Gets the description from a <c>[Description]</c> attribute, or <see langword="null" /> if absent.</summary>
    public string? Description { get; }
}
