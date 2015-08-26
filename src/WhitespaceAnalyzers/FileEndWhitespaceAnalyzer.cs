using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace WhitespaceAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FileEndWhitespaceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WS002";
        private const string Category = "Whitespace";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(Resources.FileEndWhitespaceTitle), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.FileEndWhitespaceMessageFormat), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.FileEndWhitespaceDescription), Resources.ResourceManager,
                typeof (Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            if (context.Tree.IsGenerated())
            {
                return;
            }

            var lastToken = context.Tree.GetRoot().GetLastToken();
            var trailingTrivia = lastToken.TrailingTrivia;

            if (trailingTrivia.Count == 1 && trailingTrivia.First().IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return;
            }

            var span = TextSpan.FromBounds(lastToken.Span.Start, lastToken.Span.End);
            var location = Location.Create(context.Tree, span);
            context.ReportDiagnostic(Diagnostic.Create(Rule, location));
        }
    }
}
