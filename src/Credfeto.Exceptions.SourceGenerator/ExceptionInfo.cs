using System;
using System.Diagnostics;

namespace Credfeto.Exceptions.SourceGenerator;

/// <summary>
///     Holds the information extracted from a partial exception class declaration needed to generate constructors.
/// </summary>
[DebuggerDisplay("{DebugDisplayName,nq}")]
internal readonly struct ExceptionInfo : IEquatable<ExceptionInfo>
{
    /// <summary>
    ///     Initialises a new instance of the <see cref="ExceptionInfo" /> struct.
    /// </summary>
    /// <param name="namespaceName">The namespace of the class, or <see langword="null" /> if the class is in the global namespace.</param>
    /// <param name="className">The simple name of the exception class.</param>
    /// <param name="accessModifier">The access modifier keyword (e.g. <c>public</c>, <c>internal</c>).</param>
    /// <param name="isSealed">Whether the class is declared <c>sealed</c>.</param>
    /// <param name="isAbstract">Whether the class is declared <c>abstract</c>.</param>
    /// <param name="description">The value from a <c>[Description]</c> attribute, or <see langword="null" /> if absent.</param>
    public ExceptionInfo(
        string? namespaceName,
        string className,
        string accessModifier,
        bool isSealed,
        bool isAbstract,
        string? description
    )
    {
        this.Namespace = namespaceName;
        this.ClassName = className;
        this.AccessModifier = accessModifier;
        this.IsSealed = isSealed;
        this.IsAbstract = isAbstract;
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

    /// <summary>Gets a value indicating whether the class is abstract.</summary>
    public bool IsAbstract { get; }

    /// <summary>Gets the description from a <c>[Description]</c> attribute, or <see langword="null" /> if absent.</summary>
    public string? Description { get; }

    private string DebugDisplayName =>
        this.Namespace is null
            ? $"{this.ClassName} ({this.AccessModifier}) Sealed={this.IsSealed} Abstract={this.IsAbstract}"
            : $"{this.Namespace}.{this.ClassName} ({this.AccessModifier}) Sealed={this.IsSealed} Abstract={this.IsAbstract}";

    /// <inheritdoc />
    public bool Equals(ExceptionInfo other)
    {
        return string.Equals(this.Namespace, other.Namespace, StringComparison.Ordinal)
            && string.Equals(this.ClassName, other.ClassName, StringComparison.Ordinal)
            && string.Equals(this.AccessModifier, other.AccessModifier, StringComparison.Ordinal)
            && this.IsSealed == other.IsSealed
            && this.IsAbstract == other.IsAbstract
            && string.Equals(this.Description, other.Description, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ExceptionInfo other && this.Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        (
            this.Namespace,
            this.ClassName,
            this.AccessModifier,
            this.IsSealed,
            this.IsAbstract,
            this.Description
        ).GetHashCode();

    /// <summary>Returns a value that indicates whether two <see cref="ExceptionInfo" /> values are equal.</summary>
    public static bool operator ==(in ExceptionInfo left, in ExceptionInfo right) => left.Equals(right);

    /// <summary>Returns a value that indicates whether two <see cref="ExceptionInfo" /> values are not equal.</summary>
    public static bool operator !=(in ExceptionInfo left, in ExceptionInfo right) => !left.Equals(right);
}
