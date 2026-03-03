## ADDED Requirements

### Requirement: Detect synchronous invocations with valid async equivalents in async callers
The analyzer SHALL report a diagnostic when a synchronous method invocation has a valid equivalent asynchronous method and the invocation occurs within an `async` method or `async` local function.

#### Scenario: Invocation in async method
- **WHEN** a synchronous invocation `x.M()` occurs inside an `async` method and an equivalent `MAsync` exists per the equivalent-method rules
- **THEN** the analyzer reports a diagnostic on the invocation

#### Scenario: Invocation in non-async method
- **WHEN** a synchronous invocation `x.M()` occurs inside a non-`async` method
- **THEN** the analyzer does not report a diagnostic

### Requirement: Define equivalence using the shared equivalent-async finder rules
The system SHALL determine whether an async equivalent exists using the same symbol-based equivalence rules as the shared equivalent-async finder, including:
- candidate name is the synchronous method name with the `Async` suffix
- candidate lookup supports instance, static, and extension methods (including reduced extension methods)
- return type compatibility follows known awaitable types and wrapping rules
- parameter compatibility requires matching parameter types and names, with an optional trailing `CancellationToken`

#### Scenario: Extension method equivalent
- **WHEN** the synchronous method is an extension method and the async equivalent is available as an extension method in scope
- **THEN** the analyzer and code fix treat it as a valid equivalent

#### Scenario: Optional CancellationToken
- **WHEN** the async equivalent has the same parameters as the sync method plus a trailing `CancellationToken`
- **THEN** the analyzer and code fix treat it as a valid equivalent

### Requirement: Apply code fix by replacing invocation with async equivalent and awaiting when required
The code fix SHALL replace the synchronous invocation with the resolved async equivalent invocation and SHALL add `await` when the invocation is in a context where awaiting is required and syntactically valid.

#### Scenario: Expression statement in async method
- **WHEN** the synchronous invocation is used as an expression statement inside an `async` method
- **THEN** the code fix replaces it with `await <async-invocation>`

#### Scenario: Assignment in async method
- **WHEN** the synchronous invocation is used on the right-hand side of an assignment inside an `async` method
- **THEN** the code fix replaces it with `await <async-invocation>` on the right-hand side

#### Scenario: Return statement in async method
- **WHEN** the synchronous invocation is returned from an `async` method
- **THEN** the code fix replaces it with `return await <async-invocation>`

### Requirement: Avoid double-await
The code fix SHALL NOT introduce a nested `await` when the invocation is already awaited.

#### Scenario: Already awaited invocation
- **WHEN** the original code is `await x.M()` and the async equivalent is `MAsync`
- **THEN** the code fix produces `await x.MAsync()` (single await)

### Requirement: Preserve trivia and formatting
The code fix SHALL preserve leading and trailing trivia of the original invocation expression.

#### Scenario: Invocation with comments and whitespace
- **WHEN** the invocation has leading or trailing trivia (comments/whitespace)
- **THEN** the rewritten invocation preserves that trivia

### Requirement: Do not suggest ignored methods
The analyzer SHALL NOT report a diagnostic for methods that are explicitly ignored by the equivalent-async finder.

#### Scenario: Ignored method
- **WHEN** the invoked method matches an ignored method rule in the equivalent-async finder
- **THEN** the analyzer does not report a diagnostic
