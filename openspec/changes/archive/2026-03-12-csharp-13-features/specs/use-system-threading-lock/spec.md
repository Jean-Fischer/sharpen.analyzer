## ADDED Requirements

### Requirement: Detect dedicated sync objects that can be migrated to System.Threading.Lock
The analyzer SHALL detect private dedicated synchronization fields used only as the target of `lock` statements and suggest migrating them to `System.Threading.Lock`.

#### Scenario: Analyzer flags private readonly object used only for lock
- **WHEN** a type declares `private readonly object _sync = new();` and `_sync` is used only in `lock (_sync)` statements
- **THEN** the analyzer reports a suggestion diagnostic recommending `System.Threading.Lock`

#### Scenario: Analyzer does not flag when the object is used for other purposes
- **WHEN** the sync field is used outside `lock` (e.g., passed as an argument, stored in collections, compared, or used with `Monitor.*`)
- **THEN** the analyzer does not report this diagnostic

### Requirement: Offer a code fix only when migration is safe
The fix provider SHALL offer a code fix to change the field type and initialization to `System.Threading.Lock` only when the migration can be proven safe.

#### Scenario: Fix changes field type and initializer
- **WHEN** the field is a dedicated lock object and `System.Threading.Lock` is available in the compilation
- **THEN** the fix provider changes the field type to `System.Threading.Lock` and updates the initializer accordingly

#### Scenario: Fix not offered when Monitor APIs are used
- **WHEN** the code uses `Monitor.Enter/Exit/Wait/Pulse` with the same object
- **THEN** the fix provider does not offer the code fix

### Requirement: Provide a safety checker for System.Threading.Lock migration
A safety checker SHALL validate that the field is dedicated to locking and that the target type is available.

#### Scenario: Safety checker blocks fix when Lock type is missing
- **WHEN** `System.Threading.Lock` cannot be resolved in the compilation
- **THEN** the safety checker blocks the fix
