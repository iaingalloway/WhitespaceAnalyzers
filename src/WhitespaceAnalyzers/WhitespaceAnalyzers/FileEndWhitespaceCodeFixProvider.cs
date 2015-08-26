using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace WhitespaceAnalyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FileEndWhitespaceCodeFixProvider)), Shared]
    public class FileEndWhitespaceCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Insert a newline at the end of the file";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(FileEndWhitespaceAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
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

            var oldToken = syntaxRoot.GetLastToken();

            var newTrivia = oldToken.TrailingTrivia.Insert(0, SyntaxFactory.CarriageReturnLineFeed);
            var newToken = oldToken.WithTrailingTrivia(newTrivia);
            var newSyntaxRoot = syntaxRoot.ReplaceToken(oldToken, newToken);
            var newDocument = document.WithSyntaxRoot(newSyntaxRoot);

            return newDocument;
        }
    }
}
