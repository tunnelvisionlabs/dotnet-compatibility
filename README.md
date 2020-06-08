# Binary Compatibility Checker

This project implements a binary compatibility checker for .NET assemblies. The goal of this project is to automate the
process of identifying binary- and source-breaking changes between releases of a .NET library. Libraries with string
compatibility policies will eventually be able to incorporate this check in the unit tests for the library. The checker
will download a declared previous release of the library from NuGet (or potentially from another source), and verify
that the latest build of the library is compatible with the binary interfaces defined by the earlier release.

This library may also feature support for higher levels of compatibility, such as warning users if new overloads of a
method could result in an `AmbiguousMatchException` for users who didn't specify a complete signature when using
reflection.

Diagnostics produced by this checker will be categorized by their likely impact on consumers, and by whether the change
constitutes a binary- or source-level breaking change. At this time, there are no plans to implement detection or
reporting for changes in runtime semantics.

## Usage

Compare versions:  
  `dotnet compat Assembly-1.0.0.dll Assembly-1.0.1.dll`  
Compare versions in Azure Pipelines as CI:  
  `dotnet compat --azure-pipelines Assembly-1.0.0.dll Assembly-1.0.1.dll`  
Compare versions in Azure Pipelines as CI without failing the CI job:  
  `dotnet compat --azure-pipelines --warnings-only Assembly-1.0.0.dll Assembly-1.0.1.dll`  

  -a, --azure-pipelines          (Default: false) Include the logging prefixes for Azure Pipelines.

  -w, --warnings-only            (Default: false) Do not raise errors for Azure Pipelines, it also swallows the return code.

  --help                         Display this help screen.

  --version                      Display version information.

  reference assembly (pos. 0)    Required. The reference assembly.

  new assembly (pos. 1)          Required. The new assembly.

## Definitions

* **Reference assembly**: The "old" version of the assembly used for compatibility analysis.
* **New assembly**: The "new" version of the assembly used for compatibility analysis. The term "current assembly" may
  be used as an alias of this term.

## Current Analysis

### Assembly analysis

1. [x] The strong name of an assembly is checked.
2. [ ] Referenced assemblies are checked for basic compatibility (not yet implemented; see
   [#11](https://github.com/rackerlabs/dotnet-compatibility/issues/11)).

### Type analysis

A type is *publicly accessible* if it meets all of the following conditions.

1. The type is not marked `private`, `internal`, or has the accessibility
   [NestedFamANDAssem](http://msdn.microsoft.com/en-us/library/system.reflection.typeattributes.aspx) (not expressible
   in C#).
2. If the type is nested within another type, the declaring type is publicly accessible.

#### Publicly accessible type analysis

These analysis rules only apply to types which are publicly accessible in the reference assembly.

1. [x] Publicly accessible types cannot be removed. This rule does not currently consider `[TypeForwardedTo]` (see
   [#10](https://github.com/rackerlabs/dotnet-compatibility/issues/10)).
2. [x] The base type of a publicly accessible type cannot change. This rule may be relaxed in the future as a result of
   [#9](https://github.com/rackerlabs/dotnet-compatibility/issues/9).
3. [ ] New interface implementations cannot be added to publicly-accessible interface types (not yet implemented; see
   [#12](https://github.com/rackerlabs/dotnet-compatibility/issues/12)).
4. [x] New methods cannot be added to an existing interface.

### Field analysis

A field is *publicly accessible* if it meets all of the following conditions.

1. The field is not marked `private`, `internal`, or has the accessibility
   [FamANDAssem](http://msdn.microsoft.com/en-us/library/system.reflection.fieldattributes.aspx) (not expressible
   in C#).
2. The declaring type of the field is publicly accessible.

#### Publicly accessible field analysis

These analysis rules only apply to fields which are publicly accessible in the reference assembly.

1. [x] Publicly accessible fields cannot be removed.
2. [x] Publicly accessible fields cannot change type.
3. [x] The attributes for a field cannot change (may be relaxed; see
   [#13](https://github.com/rackerlabs/dotnet-compatibility/issues/13)).

### Method analysis

A method is *publicly accessible* if it meets all of the following conditions.

1. The method is not marked `private`, `internal`, or has the accessibility
   [FamANDAssem](http://msdn.microsoft.com/en-us/library/system.reflection.methodattributes.aspx) (not expressible
   in C#).
2. The declaring type of the method is publicly accessible.

#### Publicly accessible method analysis

These analysis rules only apply to methods which are publicly accessible in the reference assembly.

1. [x] Publicly accessible methods cannot be removed
2. [x] The return type and parameter types cannot change.
3. [x] The attributes for a method cannot change (may be relaxed; see
   [#14](https://github.com/rackerlabs/dotnet-compatibility/issues/14)).

### Event analysis

An event is *publicly accessible* if it meets all of the following conditions.

1. At least one accessor method of the event is publicly accessible.
2. The declaring type of the event is publicly accessible.

#### Publicly accessible event analysis

These analysis rules only apply to events which are publicly accessible in the reference assembly.

1. [x] The event type cannot change.
2. [ ] Each publicly accessible accessor from the reference assembly must correspond to the same method in the new
   assembly (not yet implemented; see [#15](https://github.com/rackerlabs/dotnet-compatibility/issues/15)).

### Property analysis

An property is *publicly accessible* if it meets all of the following conditions.

1. At least one accessor method of the property is publicly accessible.
2. The declaring type of the property is publicly accessible.

#### Publicly accessible property analysis

These analysis rules only apply to properties which are publicly accessible in the reference assembly.

1. [x] The property type cannot change.
2. [ ] Each publicly accessible accessor from the reference assembly must correspond to the same method in the new
   assembly (not yet implemented; see [#16](https://github.com/rackerlabs/dotnet-compatibility/issues/16)).
