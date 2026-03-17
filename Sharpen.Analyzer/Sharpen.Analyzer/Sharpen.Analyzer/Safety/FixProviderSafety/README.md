# FixProviderSafety

This folder contains **per-fix-provider** safety checkers.

## Purpose

A safety checker is responsible for determining whether a transformation is safe to offer in the current syntax/semantic
context.

Safety checks are used to gate:

- diagnostic reporting (analyzers)
- code action registration (code fix providers)

## Conventions

- One fix provider ↔ one safety checker.
- Keep checkers small and conservative.
- Prefer syntax-only checks first; use semantic model only when needed.

## Registry

The canonical fix provider ↔ safety checker mapping lives in [
`FixProviderSafetyMapping`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/FixProviderSafetyMapping.cs:1).
