## ADDED Requirements

### Requirement: Suggest OverloadResolutionPriorityAttribute usage for ambiguous overload sets
The analyzer SHALL detect overload sets where overload resolution ambiguity or undesired selection could be mitigated by `OverloadResolutionPriorityAttribute` and provide guidance.

#### Scenario: Analyzer flags overload set with potential ambiguity
- **WHEN** a type defines multiple overloads that are likely to cause ambiguous calls or select an older overload unintentionally
- **THEN** the analyzer reports an info/suggestion diagnostic recommending consideration of `OverloadResolutionPriorityAttribute`

#### Scenario: Analyzer does not flag when overload set is unambiguous
- **WHEN** overload resolution is clearly unambiguous for typical call patterns
- **THEN** the analyzer does not report this diagnostic

### Requirement: Optional code action is review-required
If a code action is provided, it SHALL be labeled as requiring review.

#### Scenario: Code action adds OverloadResolutionPriorityAttribute
- **WHEN** the user invokes the code action
- **THEN** the fix adds the attribute with a suggested priority value and labels the action as “requires review”
