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

            var results = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(CreateGeneratedPart(applyTo, context.SemanticModel));
            return Task.FromResult(results);
        }

        private static ClassDeclarationSyntax CreateGeneratedPart(ClassDeclarationSyntax applyTo, SemanticModel semanticModel)
        {
            return SyntaxFactory
                .ClassDeclaration(applyTo.Identifier)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(CreateConstructor(applyTo, semanticModel));
        }

        private static ConstructorDeclarationSyntax CreateConstructor(ClassDeclarationSyntax applyTo, SemanticModel semanticModel)
        {
            var propertiesWithParameters = applyTo
                .Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(p => p.AccessorList == null || p.AccessorList.Accessors.All(a => a.Body == null))
                .OrderByDescending(p => IsRequiredProperty(p, semanticModel))
                .Select(p => (Property: p, Parameter: CreateConstructorParameterForProperty(p, semanticModel)))
                .ToArray();

            var statements = propertiesWithParameters
                .Select(pair => CreateAssignment(pair.Property.Identifier, pair.Parameter.Identifier));

            return SyntaxFactory
                .ConstructorDeclaration(applyTo.Identifier)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(propertiesWithParameters.Select(p => p.Parameter).ToArray())
                .AddBodyStatements(statements.ToArray());
        }

        private static ParameterSyntax CreateConstructorParameterForProperty(PropertyDeclarationSyntax p, SemanticModel semanticModel)
        {
            var @default = !IsRequiredProperty(p, semanticModel)
                ? SyntaxFactory.EqualsValueClause(SyntaxFactory.DefaultExpression(p.Type))
                : null;

            return SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(ToCamelCase(p.Identifier.Text)))
                .WithType(p.Type)
                .WithDefault(@default);
        }

        private static bool IsRequiredProperty(PropertyDeclarationSyntax p, SemanticModel semanticModel)
        {
            if (!(semanticModel.GetTypeInfo(p.Type).Type is INamedTypeSymbol type))
                return false;

            return !IsNullableValueType(type) || !IsNullableReferenceType(type);
        }

        private static bool IsNullableValueType(INamedTypeSymbol type)
        {
            return type.IsValueType && type.IsGenericType && type.ConstructUnboundGenericType().Name == "Nullable";
        }

        private static bool IsNullableReferenceType(ITypeSymbol type)
        {
            return true;
        }

        private static ExpressionStatementSyntax CreateAssignment(SyntaxToken left, SyntaxToken right)
        {
            var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(left),
                SyntaxFactory.IdentifierName(right));

            return SyntaxFactory.ExpressionStatement(assignment);
        }

        private static string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
                return name;

            var chars = name.ToCharArray();
            chars[0] = char.ToLower(chars[0]);
            return new string(chars);
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
