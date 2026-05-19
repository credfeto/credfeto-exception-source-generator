# Credfeto.Exceptions.SourceGenerator

A C# Roslyn incremental source generator that generates standard exception constructors for exception classes.

## Build Status

| Branch  | Status                                                                                                                                                                                                                                                                                                                                      |
|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| main    | [![Build: Pre-Release](https://github.com/credfeto/credfeto-exception-source-generator/actions/workflows/build-and-publish-pre-release.yml/badge.svg)](https://github.com/credfeto/credfeto-exception-source-generator/actions/workflows/build-and-publish-pre-release.yml) |
| release | [![Build: Release](https://github.com/credfeto/credfeto-exception-source-generator/actions/workflows/build-and-publish-release.yml/badge.svg)](https://github.com/credfeto/credfeto-exception-source-generator/actions/workflows/build-and-publish-release.yml)             |

## NuGet Packages

| Package                                         | Version                                                                                                                                                                                                                                                   |
|-------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Credfeto.Exceptions.SourceGenerator`           | [![NuGet](https://img.shields.io/nuget/v/Credfeto.Exceptions.SourceGenerator.svg)](https://www.nuget.org/packages/Credfeto.Exceptions.SourceGenerator/)           |
| `Credfeto.Exceptions.SourceGenerator.CodeFixes` | [![NuGet](https://img.shields.io/nuget/v/Credfeto.Exceptions.SourceGenerator.CodeFixes.svg)](https://www.nuget.org/packages/Credfeto.Exceptions.SourceGenerator.CodeFixes/) |

## Overview

Writing exception classes in C# involves significant boilerplate – every exception class needs the same three constructors. This source generator eliminates that boilerplate by automatically generating the standard constructors when you declare a `partial` exception class.

## Installation

Add the source generator to your project:

```xml
<ItemGroup>
  <PackageReference Include="Credfeto.Exceptions.SourceGenerator" Version="x.y.z" />
</ItemGroup>
```

Optionally, add the code-fix companion package to get IDE refactoring support (offers to convert existing exception classes to use the generator):

```xml
<ItemGroup>
  <PackageReference Include="Credfeto.Exceptions.SourceGenerator.CodeFixes" Version="x.y.z" />
</ItemGroup>
```

## Usage

### Basic usage

Declare your exception class as `partial` and inherit from `Exception` (or any exception type):

```csharp
public partial sealed class ExampleException : Exception;
```

The source generator will automatically produce:

```csharp
public sealed partial class ExampleException : Exception
{
    public ExampleException() { }

    public ExampleException(string? message)
        : base(message) { }

    public ExampleException(string? message, Exception? innerException)
        : base(message: message, innerException: innerException) { }
}
```

### With a default message

Apply the standard `[System.ComponentModel.Description]` attribute to provide a default message for the no-argument constructor:

```csharp
using System.ComponentModel;

[Description("An example error occurred.")]
public partial sealed class ExampleException : Exception;
```

The source generator will produce:

```csharp
public sealed partial class ExampleException : Exception
{
    public ExampleException()
        : this("An example error occurred.")
    {
    }

    public ExampleException(string? message)
        : base(message) { }

    public ExampleException(string? message, Exception? innerException)
        : base(message: message, innerException: innerException) { }
}
```

### Derived exceptions

The generator works with any class that directly or indirectly inherits from `System.Exception`:

```csharp
public partial sealed class MyDomainException : InvalidOperationException;
```

### Access modifiers

The generator respects the access modifiers declared on the partial class:

```csharp
// public, internal, protected, private etc.
internal partial sealed class InternalException : Exception;
```

## Refactoring Support

The `Credfeto.Exceptions.SourceGenerator.CodeFixes` package provides an IDE refactoring (a Roslyn code fix) that detects existing exception classes with standard constructors and offers to convert them to use the source generator. This fires as a diagnostic suggestion (`EXCGEN001`) and presents a lightbulb action in Visual Studio and JetBrains Rider.

To take advantage of the refactoring:

1. Install `Credfeto.Exceptions.SourceGenerator.CodeFixes`.
2. Open an existing non-partial exception class in your IDE.
3. Look for the `EXCGEN001` suggestion and apply the code fix.

The code fix will:

- Add the `partial` modifier to the class declaration.
- Remove the standard constructors (no-args, message, message + inner exception) so the source generator can generate them.

## Technical Details

- Uses the latest Roslyn **incremental source generator** APIs (`IIncrementalGenerator`).
- Only triggers on `partial` classes whose inheritance chain includes `System.Exception`.
- Generated files are marked with `// <auto-generated/>` and use `#nullable enable`.
- The `[Description]` attribute value is string-escaped to handle quotes, backslashes and common escape sequences safely.

## Changelog

View [changelog](CHANGELOG.md)

## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->