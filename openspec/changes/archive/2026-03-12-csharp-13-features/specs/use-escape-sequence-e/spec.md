## ADDED Requirements

### Requirement: Detect ESC character escapes that can be replaced with \e
The analyzer SHALL detect string and character literals that represent the ESC character using `\u001b` or `\x1b` and suggest using the C# 13 `\e` escape sequence.

#### Scenario: Analyzer flags string literal containing \u001b
- **WHEN** a string literal contains the escape sequence `\u001b`
- **THEN** the analyzer reports a suggestion diagnostic recommending `\e`

#### Scenario: Analyzer flags character literal '\x1b'
- **WHEN** a character literal uses `\x1b`
- **THEN** the analyzer reports a suggestion diagnostic recommending `\e`

### Requirement: Offer a code fix for unambiguous replacements
The fix provider SHALL offer a code fix that replaces `\u001b` and unambiguous `\x1b` escapes with `\e`.

#### Scenario: Fix replaces \u001b with \e
- **WHEN** a literal contains `\u001b`
- **THEN** the fix provider replaces it with `\e`

#### Scenario: Fix replaces \x1b with \e only when unambiguous
- **WHEN** a literal contains `\x1b` and the escape is not part of a longer hex escape sequence
- **THEN** the fix provider replaces it with `\e`

#### Scenario: Fix not offered for ambiguous \x escapes
- **WHEN** a literal contains a `\x` escape where the consumed hex digits are ambiguous (e.g., `\x1b2`)
- **THEN** the fix provider does not offer the code fix

### Requirement: Provide a safety checker for \e replacements
A safety checker SHALL ensure the replacement is safe.

#### Scenario: Safety checker blocks fix when replacement would change tokenization
- **WHEN** replacing `\x1b` with `\e` would change how the literal is parsed
- **THEN** the safety checker blocks the fix
