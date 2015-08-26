using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace WhitespaceAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FileStartWhitespaceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WS001";
        private const string Category = "Whitespace";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(Resources.FileStartWhitespaceTitle), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.FileStartWhitespaceMessageFormat), Resources.ResourceManager,
                typeof (Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.FileStartWhitespaceDescription), Resources.ResourceManager,
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
            var leadingTrivia = context.Tree.GetRoot().GetLeadingTrivia();

            if (!leadingTrivia.Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) || x.IsKind(SyntaxKind.EndOfLineTrivia)))
            {
                return;
            }
                
            var span = TextSpan.FromBounds(leadingTrivia.Span.Start, leadingTrivia.Span.End);
            var location = Location.Create(context.Tree, span);
            context.ReportDiagnostic(Diagnostic.Create(Rule, location));
        }
    }
}