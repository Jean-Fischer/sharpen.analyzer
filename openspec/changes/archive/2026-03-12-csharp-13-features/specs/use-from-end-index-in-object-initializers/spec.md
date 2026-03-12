## ADDED Requirements

### Requirement: Detect end-based index patterns in object/collection initializers
The analyzer SHALL detect assignments in object/collection initializers that compute an index from the end (e.g., `Length - 1`) and suggest using from-end indices (`^`).

#### Scenario: Analyzer flags Length-based last element assignment in initializer
- **WHEN** an initializer assigns to an index expression equivalent to `target.Length - 1`
- **THEN** the analyzer reports a suggestion diagnostic recommending `target[^1]`

#### Scenario: Analyzer does not flag non-end-based indices
- **WHEN** an initializer assigns to a constant index (e.g., `[0]`, `[1]`)
- **THEN** the analyzer does not report this diagnostic

### Requirement: Offer a code fix for safe from-end index substitutions
The fix provider SHALL offer a code fix that replaces safe end-based index expressions with the equivalent from-end index.

#### Scenario: Fix replaces Length - 1 with ^1
- **WHEN** the initializer uses an index expression equivalent to `Length - 1`
- **THEN** the fix provider replaces it with `^1`

#### Scenario: Fix not offered when semantics are unclear
- **WHEN** the index expression is not a simple, provably equivalent end-based pattern
- **THEN** the fix provider does not offer the code fix

### Requirement: Provide a safety checker for from-end index fixes
A safety checker SHALL ensure the replacement preserves semantics.

#### Scenario: Safety checker blocks fix when target is not indexable with Index
- **WHEN** the target type does not support `System.Index`-based indexing
- **THEN** the safety checker blocks the fix
