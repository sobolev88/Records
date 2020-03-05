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
                progress.ReportError($"Type can't be made into a record. It must be a partial class", "Records.Error", context.ProcessingNode);
                return Task.FromResult(SyntaxFactory.List<MemberDeclarationSyntax>());
            }

            var generator = new RecordPartGenerator(applyTo, context.SemanticModel);
            var results = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(generator.Generate());
            return Task.FromResult(results);
        }
    }
}
