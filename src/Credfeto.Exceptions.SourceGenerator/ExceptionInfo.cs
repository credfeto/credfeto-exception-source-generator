namespace Credfeto.Exceptions.SourceGenerator;

/// <summary>
///     Holds the information extracted from a partial exception class declaration needed to generate constructors.
/// </summary>
/// <param name="Namespace">The namespace of the class, or <see langword="null" /> if the class is in the global namespace.</param>
/// <param name="ClassName">The simple name of the exception class.</param>
/// <param name="AccessModifier">The access modifier keyword (e.g. <c>public</c>, <c>internal</c>).</param>
/// <param name="IsSealed">Whether the class is declared <c>sealed</c>.</param>
/// <param name="Description">The value from a <c>[Description]</c> attribute, or <see langword="null" /> if absent.</param>
internal sealed record ExceptionInfo(string? Namespace, string ClassName, string AccessModifier, bool IsSealed, string? Description);
