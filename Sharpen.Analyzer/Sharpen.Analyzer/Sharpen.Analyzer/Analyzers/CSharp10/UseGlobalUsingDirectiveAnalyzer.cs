using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp10;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseGlobalUsingDirectiveAnalyzer : DiagnosticAnalyzer
{
    private const int Threshold = 2;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.CSharp10Rules.UseGlobalUsingDirectiveRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext context)
    {
        if (!Common.CSharpLanguageVersion.IsCSharp10OrAbove(context.Compilation))
            return;

        // Collect using directives per syntax tree.
        var perTree = new ConcurrentDictionary<SyntaxTree, List<UsingKey>>();

        context.RegisterSyntaxTreeAction(treeContext =>
        {
            if (treeContext.Tree.Options is not CSharpParseOptions parseOptions || parseOptions.LanguageVersion < LanguageVersion.CSharp10)
                return;

            var root = treeContext.Tree.GetRoot(treeContext.CancellationToken);
            var usings = root.DescendantNodes(n => n is CompilationUnitSyntax or UsingDirectiveSyntax)
                .OfType<UsingDirectiveSyntax>()
                .Where(u => u.Parent is CompilationUnitSyntax)
                .Select(CreateKey)
                .Where(k => k != null)
                .Cast<UsingKey>()
                .ToList();

            perTree[treeContext.Tree] = usings;
        });

        context.RegisterCompilationEndAction(endContext =>
        {
            // Group by normalized key.
            var all = perTree.SelectMany(kvp => kvp.Value.Select(v => (Tree: kvp.Key, Key: v))).ToList();

            var groups = all.GroupBy(x => x.Key, UsingKeyComparer.Instance).ToList();

            // Determine which keys are repeated and safe to suggest.
            var repeated = new HashSet<UsingKey>(UsingKeyComparer.Instance);

            foreach (var g in groups)
            {
                if (g.Count() < Threshold)
                    continue;

                // Alias safety: only suggest if alias maps to the same target everywhere.
                if (g.Key.Alias != null)
                {
                    var distinctTargets = g.Select(x => x.Key.Target).Distinct(StringComparer.Ordinal).ToList();
                    if (distinctTargets.Count != 1)
                        continue;
                }

                repeated.Add(g.Key);
            }

            if (repeated.Count == 0)
                return;

            // Report diagnostics on each repeated using directive.
            foreach (var kvp in perTree)
            {
                var tree = kvp.Key;
                var root = tree.GetRoot(endContext.CancellationToken);
                foreach (var usingDirective in root.DescendantNodes(n => n is CompilationUnitSyntax or UsingDirectiveSyntax)
                             .OfType<UsingDirectiveSyntax>()
                             .Where(u => u.Parent is CompilationUnitSyntax))
                {
                    var key = CreateKey(usingDirective);
                    if (key == null)
                        continue;

                    if (!repeated.Contains(key.Value))
                        continue;

                    endContext.ReportDiagnostic(Diagnostic.Create(
                        Rules.CSharp10Rules.UseGlobalUsingDirectiveRule,
                        usingDirective.GetLocation()));
                }
            }
        });
    }

    private static UsingKey? CreateKey(UsingDirectiveSyntax u)
    {
        // Ignore already-global usings.
        if (u.GlobalKeyword != default)
            return null;

        var isStatic = u.StaticKeyword != default;

        var alias = u.Alias?.Name.Identifier.ValueText;
        var target = u.Name?.ToString();
        if (string.IsNullOrWhiteSpace(target))
            return null;

        return new UsingKey(alias, isStatic, target);
    }

    private readonly struct UsingKey
    {
        public UsingKey(string? alias, bool isStatic, string target)
        {
            Alias = alias;
            IsStatic = isStatic;
            Target = target;
        }

        public string? Alias { get; }
        public bool IsStatic { get; }
        public string Target { get; }
    }

    private sealed class UsingKeyComparer : IEqualityComparer<UsingKey>
    {
        public static readonly UsingKeyComparer Instance = new();

        public bool Equals(UsingKey x, UsingKey y)
        {
            return string.Equals(x.Alias, y.Alias, StringComparison.Ordinal)
                   && x.IsStatic == y.IsStatic
                   && string.Equals(x.Target, y.Target, StringComparison.Ordinal);
        }

        public int GetHashCode(UsingKey obj)
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) + (obj.Alias != null ? StringComparer.Ordinal.GetHashCode(obj.Alias) : 0);
                hash = (hash * 31) + obj.IsStatic.GetHashCode();
                hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(obj.Target);
                return hash;
            }
        }
    }
}
