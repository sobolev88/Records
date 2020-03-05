using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Records
{
    public class RecordCodeGenerator : ICodeGenerator
    {
        public RecordCodeGenerator(AttributeData _)
        {

        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            if (!(context.ProcessingNode is ClassDeclarationSyntax applyTo) || !applyTo.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                ReportError($"Type can't be made into a record. It must be a partial class", "Records.Error", context.ProcessingNode, progress);
                return Task.FromResult(SyntaxFactory.List<MemberDeclarationSyntax>());
            }

            var generator = new RecordPartGenerator(applyTo, context.SemanticModel);

            var results = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(generator.Generate());
            return Task.FromResult(results);
        }

        private static void ReportError(string message, string codeGenCategory, SyntaxNode node, IProgress<Diagnostic> progress)
        {
            Report(message, codeGenCategory, DiagnosticSeverity.Error, node, progress);
        }

        private static void Report(string message, string codeGenCategory, DiagnosticSeverity severity, SyntaxNode node, IProgress<Diagnostic> progress)
        {
            progress.Report(Diagnostic.Create(
                new DiagnosticDescriptor(
                    $"CG{Math.Abs(message.GetHashCode() >> 16)}",
                    message,
                    message,
                    codeGenCategory,
                    severity,
                    true,
                    message),
                node.GetLocation()));
        }
    }
}
