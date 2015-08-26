using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace WhitespaceAnalyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileStartWhitespaceCodeFixProvider)), Shared]
    public class FileStartWhitespaceCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Remove whitespace from the start of the file";

        public override sealed ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(FileStartWhitespaceAnalyzer.DiagnosticId);

        public override sealed FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override sealed Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics.Where(x => FixableDiagnosticIds.Contains(x.Id)))
            {
                context.RegisterCodeFix(CodeAction.Create(Title,
                    x => GetTransformedDocumentAsync(context.Document, x), nameof(FileStartWhitespaceCodeFixProvider)),
                    diagnostic);
            }

            return Task.FromResult(0);
        }

        private async Task<Document> GetTransformedDocumentAsync(Document document, CancellationToken token)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);

            var firstToken = syntaxRoot.GetFirstToken(includeZeroWidth: true);
            var leadingTrivia = firstToken.LeadingTrivia;
            var newTriviaList = SyntaxFactory.TriviaList();

            var index = IndexOfFirstNonWhitespaceTrivia(leadingTrivia);

            if (index != -1)
            {
                for (var i = index; index < leadingTrivia.Count; index++)
                {
                    newTriviaList = newTriviaList.Add(leadingTrivia[i]);
                }
            }

            var newFirstToken = firstToken.WithLeadingTrivia(newTriviaList);
            var newSyntaxRoot = syntaxRoot.ReplaceToken(firstToken, newFirstToken);
            var newDocument = document.WithSyntaxRoot(newSyntaxRoot);

            return newDocument;
        }

        internal static int IndexOfFirstNonWhitespaceTrivia(IReadOnlyList<SyntaxTrivia> triviaList)
        {
            for (var index = 0; index < triviaList.Count; index++)
            {
                var currentTrivia = triviaList[index];
                if (currentTrivia.Kind() != SyntaxKind.EndOfLineTrivia && currentTrivia.Kind() != SyntaxKind.WhitespaceTrivia)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}