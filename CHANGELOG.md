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
### Deprecated
### Removed
### Deployment Changes
<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
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