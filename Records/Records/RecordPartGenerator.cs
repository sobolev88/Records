using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Records
{
    public class RecordPartGenerator
    {
        private readonly ClassDeclarationSyntax applyTo;
        private readonly SemanticModel semanticModel;

        public RecordPartGenerator(ClassDeclarationSyntax applyTo, SemanticModel semanticModel)
        {
            this.applyTo = applyTo;
            this.semanticModel = semanticModel;
        }

        public ClassDeclarationSyntax Generate()
        {
            return ClassDeclaration(applyTo.Identifier)
                .AddModifiers(Token(SyntaxKind.PartialKeyword))
                .AddMembers(CreateConstructor())
                .WithLeadingTrivia(NullablePragma(true));
        }

        private static SyntaxTrivia NullablePragma(bool active)
        {
            return Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), active));
        }

        private ConstructorDeclarationSyntax CreateConstructor()
        {
            var propertiesWithParameters = applyTo
                .Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(p => p.AccessorList == null || p.AccessorList.Accessors.All(a => a.Body == null))
                .OrderByDescending(IsRequiredProperty)
                .Select(p => (Property: p, Parameter: CreateConstructorParameterForProperty(p)))
                .ToArray();

            var statements = propertiesWithParameters
                .Select(pair => CreateAssignment(pair.Property.Identifier, pair.Parameter.Identifier));

            return ConstructorDeclaration(applyTo.Identifier)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(propertiesWithParameters.Select(p => p.Parameter).ToArray())
                .AddBodyStatements(statements.ToArray());
        }

        private ParameterSyntax CreateConstructorParameterForProperty(PropertyDeclarationSyntax p)
        {
            var @default = !IsRequiredProperty(p)
                ? EqualsValueClause(DefaultExpression(p.Type))
                : null;

            return Parameter(Identifier(ToCamelCase(p.Identifier.Text)))
                .WithType(p.Type)
                .WithDefault(@default);
        }

        private bool IsRequiredProperty(PropertyDeclarationSyntax p)
        {
            return !IsNullableProperty(p);
        }

        private bool IsNullableProperty(PropertyDeclarationSyntax property)
        {
            return semanticModel.GetDeclaredSymbol(property).NullableAnnotation == NullableAnnotation.Annotated;
        }

        private static ExpressionStatementSyntax CreateAssignment(SyntaxToken left, SyntaxToken right)
        {
            var assignment = AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(left), IdentifierName(right));
            return ExpressionStatement(assignment);
        }

        private static string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
                return name;

            var chars = name.ToCharArray();
            chars[0] = char.ToLower(chars[0]);
            return new string(chars);
        }
    }
}
