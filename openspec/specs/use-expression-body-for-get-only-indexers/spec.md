# use-expression-body-for-get-only-indexers Specification

## Purpose
TBD - created by archiving change csharp-6. Update Purpose after archive.
## Requirements
### Requirement: Detect get-only indexers eligible for expression-bodied form
The analyzer SHALL report a diagnostic when an indexer is a get-only indexer and its getter body can be expressed as a single expression.

#### Scenario: Block-bodied getter with a single return statement
- **WHEN** an indexer has a `get` accessor whose body contains exactly one `return <expr>;` statement
- **THEN** the analyzer reports a diagnostic on the indexer declaration

#### Scenario: Expression-bodied indexer is already used
- **WHEN** an indexer is already declared using expression-bodied syntax (`=>`)
- **THEN** the analyzer does NOT report a diagnostic

#### Scenario: Indexer has a setter
- **WHEN** an indexer has a `set` accessor (or `init` accessor)
- **THEN** the analyzer does NOT report a diagnostic

#### Scenario: Getter contains more than one statement
- **WHEN** an indexer getter body contains multiple statements (including multiple `return` statements)
- **THEN** the analyzer does NOT report a diagnostic

### Requirement: Provide a code fix to convert eligible get-only indexers
The code fix provider SHALL offer a fix that converts an eligible get-only indexer to an expression-bodied indexer.

#### Scenario: Apply code fix
- **WHEN** the user applies the code fix on a reported diagnostic
- **THEN** the indexer is rewritten to `=> <expr>;` preserving trivia (comments/whitespace) as much as possible

#### Scenario: Preserve signature and modifiers
- **WHEN** the code fix rewrites the indexer
- **THEN** the indexer keeps the same accessibility, modifiers, attributes, return type, and parameter list

