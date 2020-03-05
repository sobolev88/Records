using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            return SyntaxFactory
                .ClassDeclaration(applyTo.Identifier)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(CreateConstructor());
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

            return SyntaxFactory
                .ConstructorDeclaration(applyTo.Identifier)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(propertiesWithParameters.Select(p => p.Parameter).ToArray())
                .AddBodyStatements(statements.ToArray());
        }

        private ParameterSyntax CreateConstructorParameterForProperty(PropertyDeclarationSyntax p)
        {
            var @default = !IsRequiredProperty(p)
                ? SyntaxFactory.EqualsValueClause(SyntaxFactory.DefaultExpression(p.Type))
                : null;

            return SyntaxFactory
                .Parameter(SyntaxFactory.Identifier(ToCamelCase(p.Identifier.Text)))
                .WithType(p.Type)
                .WithDefault(@default);
        }

        private bool IsRequiredProperty(PropertyDeclarationSyntax p)
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
    }
}
