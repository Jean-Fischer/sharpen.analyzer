# csharp-10-file-scoped-namespaces Specification

## Purpose
TBD - created by archiving change csharp-10. Update Purpose after archive.
## Requirements
### Requirement: Detect block-scoped namespace eligible for file-scoped conversion
The analyzer SHALL report a diagnostic when a C# file contains a single top-level block-scoped namespace declaration (`namespace X { ... }`) that can be safely converted to a file-scoped namespace (`namespace X;`).

#### Scenario: Single namespace with multiple type declarations
- **WHEN** a file contains `namespace MyNs { class A {} struct B {} }`
- **THEN** the analyzer reports a diagnostic on the namespace declaration

#### Scenario: Namespace contains nested namespaces
- **WHEN** a file contains `namespace Outer { namespace Inner { class C {} } }`
- **THEN** the analyzer reports a diagnostic on the outer namespace declaration

### Requirement: Do not suggest file-scoped conversion when multiple top-level namespaces exist
The analyzer SHALL NOT report a diagnostic when a file contains more than one top-level namespace declaration.

#### Scenario: Two sibling namespaces in the same file
- **WHEN** a file contains `namespace A { class A1 {} } namespace B { class B1 {} }`
- **THEN** the analyzer does not report a diagnostic

### Requirement: Code fix converts block-scoped namespace to file-scoped namespace
When the diagnostic is reported, the code fix SHALL replace the block-scoped namespace declaration with a file-scoped namespace declaration and preserve the contained members.

#### Scenario: Preserve trivia and member ordering
- **WHEN** the code fix is applied to `namespace MyNs { /*c*/ class C { } }`
- **THEN** the result is `namespace MyNs; /*c*/ class C { }` with the class unchanged

#### Scenario: Preserve using directives and file header
- **WHEN** the code fix is applied to a file with a header comment and `using` directives before the namespace
- **THEN** the header comment and `using` directives remain at the top of the file and the namespace becomes file-scoped

### Requirement: Code fix preserves nested namespace structure
If the original file contains nested namespaces, the code fix SHALL preserve the nested namespace declarations inside the file-scoped namespace.

#### Scenario: Nested namespace remains nested
- **WHEN** the code fix is applied to `namespace Outer { namespace Inner { class C {} } }`
- **THEN** the result is `namespace Outer; namespace Inner { class C {} }`

