# await-task-delay-instead-of-thread-sleep Specification

## Purpose
TBD - created by archiving change csharp-5. Update Purpose after archive.
## Requirements
### Requirement: Detect Thread.Sleep usage in async-capable contexts
The analyzer SHALL report a diagnostic when `Thread.Sleep(...)` is invoked in a context where the containing member can be made `async` and the call can be replaced by `await Task.Delay(...)`.

#### Scenario: Thread.Sleep in method that can be made async
- **WHEN** `Thread.Sleep(...)` is invoked inside a method that can be made `async`
- **THEN** the analyzer reports a diagnostic on the invocation

#### Scenario: Thread.Sleep in context that cannot be made async
- **WHEN** `Thread.Sleep(...)` is invoked inside a member that cannot be made `async` (e.g., interface implementation with fixed signature, `Main` without async support in target language version, or other non-updatable signature)
- **THEN** the analyzer does not report a diagnostic

### Requirement: Offer code fix to replace Thread.Sleep with await Task.Delay
When the diagnostic is reported and the invocation is in a syntactically valid position for `await`, the code fix SHALL replace `Thread.Sleep(<duration>)` with `await Task.Delay(<duration>)` and SHALL update the containing member to be `async` if required.

#### Scenario: Replace Thread.Sleep with await Task.Delay
- **WHEN** the diagnostic is reported for `Thread.Sleep(1000)` inside a method that can be made `async`
- **THEN** the code fix produces `await Task.Delay(1000)` and updates the containing member to be `async`

#### Scenario: Do not offer fix when await is not legal
- **WHEN** the diagnostic is reported but the invocation occurs in a context where `await` is not legal (e.g., inside a `lock` statement)
- **THEN** the code fix is not offered

### Requirement: Preserve argument expression and trivia
The code fix SHALL preserve the original argument expression and leading/trailing trivia when rewriting the invocation.

#### Scenario: Preserve argument and comments
- **WHEN** the original code is `Thread.Sleep(/*ms*/ delay)`
- **THEN** the code fix produces `await Task.Delay(/*ms*/ delay)` preserving comments and formatting

