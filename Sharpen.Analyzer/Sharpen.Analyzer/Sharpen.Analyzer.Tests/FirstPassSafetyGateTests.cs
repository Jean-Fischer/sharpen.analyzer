using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Sharpen.Analyzer.Safety;
using Xunit;

namespace Sharpen.Analyzer.Tests;

public sealed class FirstPassSafetyGateTests
{
    [Fact]
    public void Evaluate_StopsAtFirstFailure_AndIsDeterministic()
    {
        var calls = new List<string>();

        var gate = new FirstPassSafetyGate(new IFirstPassSafetyCheck[]
        {
            new RecordingCheck("A", calls, SafetyResult.Safe()),
            new RecordingCheck("B", calls, SafetyResult.Unsafe("B")),
            new RecordingCheck("C", calls, SafetyResult.Safe()),
        });

        var result = gate.Evaluate(
            syntaxTree: null!,
            semanticModel: null!,
            diagnostic: Diagnostic.Create("X", "X", "X", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 1),
            cancellationToken: CancellationToken.None);

        Assert.False(result.IsSafe);
        Assert.Equal("B", result.ReasonId);
        Assert.Equal(new[] { "A", "B" }, calls);
    }

    [Fact]
    public void Evaluate_RespectsCancellation()
    {
        var gate = new FirstPassSafetyGate(new IFirstPassSafetyCheck[]
        {
            new NoopFirstPassSafetyCheck(),
        });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            gate.Evaluate(
                syntaxTree: null!,
                semanticModel: null!,
                diagnostic: Diagnostic.Create("X", "X", "X", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 1),
                cancellationToken: cts.Token));
    }

    private sealed class RecordingCheck : IFirstPassSafetyCheck
    {
        private readonly string _id;
        private readonly List<string> _calls;
        private readonly SafetyResult _result;

        public RecordingCheck(string id, List<string> calls, SafetyResult result)
        {
            _id = id;
            _calls = calls;
            _result = result;
        }

        public SafetyResult IsSafe(
            SyntaxTree? syntaxTree,
            SemanticModel semanticModel,
            Diagnostic? diagnostic,
            CancellationToken cancellationToken = default)
        {
            _calls.Add(_id);
            return _result;
        }
    }
}
