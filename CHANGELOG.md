# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Security
### Added
### Fixed
### Changed
- Dependencies - Updated FunFair.CodeAnalysis to 7.2.4.2117
- Dependencies - Updated FunFair.Test.Source.Generator to 6.3.3.2407
- Dependencies - Updated Meziantou.Analyzer to 3.0.122
- Dependencies - Updated Microsoft.CodeAnalysis.Analyzers to 5.6.0
- Dependencies - Updated Microsoft.CodeAnalysis.CSharp to 5.6.0
- Dependencies - Updated SonarAnalyzer.CSharp to 10.29.0.143774
- Dependencies - Updated FunFair.Test to 6.3.4.2441
### Deprecated
### Removed
### Deployment Changes
<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.7] - 2026-06-26
### Changed
- Dependencies - Updated Credfeto.Version.Information.Generator to 1.0.131.1336
- Dependencies - Updated FunFair.Test.Common to 6.3.1.2342
- Dependencies - Updated FunFair.Test.Source.Generator to 6.3.1.2342

## [0.0.6] - 2026-06-23
### Changed
- Dependencies - Updated Credfeto.Version.Information.Generator to 1.0.130.1322
- Dependencies - Updated Meziantou.Analyzer to 3.0.109
- Dependencies - Updated Microsoft.VisualStudio.Threading.Analyzers to 18.7.23

## [0.0.5] - 2026-06-18
### Changed
- Dependencies - Updated Credfeto.Version.Information.Generator to 1.0.129.1310
- Dependencies - Updated Meziantou.Analyzer to 3.0.104

## [0.0.4] - 2026-06-17
### Fixed
- Removed InternalsVisibleTo and made internal types public to comply with FFS0051 rule introduced in FunFair.CodeAnalysis 7.2.1.2035
### Changed
- Dependencies - Updated Credfeto.Version.Information.Generator to 1.0.127.1265
- Dependencies - Updated FunFair.CodeAnalysis to 7.2.1.2035
- Dependencies - Updated Meziantou.Analyzer to 3.0.102

## [0.0.3] - 2026-06-12
### Fixed
- Removed tracked IDE file that was inadvertently committed despite being in .gitignore
- Fixed AnalyzerReleases.Shipped.md and AnalyzerReleases.Unshipped.md to use correct release tracking format required by RS2007
- Cleared AnalyzerReleases.Unshipped.md - Microsoft.CodeAnalysis.Analyzers 5.3.0 requires empty file when no unreleased entries exist
### Changed
- SDK - Updated DotNet SDK to 10.0.301

## [0.0.2] - 2026-06-08
### Added
- Unit test for exception code builder with abstract exceptions and special character escaping

## [0.0.1] - 2026-06-08
### Added
- Credfeto.Exceptions.SourceGenerator: Roslyn incremental source generator that generates standard exception constructors for partial exception classes
- Credfeto.Exceptions.SourceGenerator.CodeFixes: Roslyn code fix provider (EXCGEN001) that offers to convert existing exception classes to use the source generator
- Support for abstract partial exception classes; constructors are generated as protected
### Fixed
- All three projects now pass FunFair.BuildCheck: added missing properties, analyzer packages, and fixed code quality issues flagged by the newly enabled analyzers
- Implement IEquatable<ExceptionInfo> for correct incremental generator caching; fix DebuggerDisplay for global-namespace classes; replace nullable throw-on-null select with HasValue/GetValueOrDefault
### Changed
- die() must output to stderr so error messages are not swallowed by stdout pipelines

## [0.0.0] - Project created