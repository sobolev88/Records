using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Records
{
    public class ClassGenerator
    {
        private readonly ClassDeclarationSyntax applyTo;
        private readonly SemanticModel semanticModel;
        private readonly bool generateWith;

        public ClassGenerator(ClassDeclarationSyntax applyTo, SemanticModel semanticModel, bool generateWith)
        {
            this.applyTo = applyTo;
            this.semanticModel = semanticModel;
            this.generateWith = generateWith;
        }

        public ClassDeclarationSyntax Generate()
        {
            return ClassDeclaration(applyTo.Identifier)
                .AddModifiers(Token(SyntaxKind.PartialKeyword))
                .AddMembers(CreateConstructor())
                .AddMembers(CreateWithMethods())
                .WithLeadingTrivia(NullablePragma(true));
        }

        private static SyntaxTrivia NullablePragma(bool active)
        {
            return Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), active));
        }

        private ConstructorDeclarationSyntax CreateConstructor()
        {
            var propertiesWithParameters = GetProperties()
                .Select(p => (Property: p, Parameter: CreateConstructorParameterForProperty(p)))
                .ToArray();

            var statements = propertiesWithParameters
                .Select(pair => CreateAssignment(pair.Property, pair.Parameter));

            return ConstructorDeclaration(applyTo.Identifier)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(propertiesWithParameters.Select(p => p.Parameter).ToArray())
                .AddBodyStatements(statements.ToArray());
        }

        private IEnumerable<PropertyDeclarationSyntax> GetProperties()
        {
            return applyTo
                            .Members
                            .OfType<PropertyDeclarationSyntax>()
                            .Where(p => p.AccessorList == null || p.AccessorList.Accessors.All(a => a.Body == null))
                            .OrderByDescending(IsRequiredProperty);
        }

        private MethodDeclarationSyntax[] CreateWithMethods()
        {
            return generateWith
                ? GetProperties()
                .Select(WithMethod)
                .ToArray()
                : Array.Empty<MethodDeclarationSyntax>();
        }

        private MethodDeclarationSyntax WithMethod(PropertyDeclarationSyntax property)
        {
            var classType = ParseTypeName(applyTo.Identifier.Text);

            var parameter = Parameter(Identifier(ToCamelCase(property.Identifier.Text))).WithType(property.Type);
            return MethodDeclaration(classType, $"With{property.Identifier.Text}")
                .AddParameterListParameters(parameter)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBodyStatements(ReturnStatement(ObjectCreationExpression(classType)
                    .AddArgumentListArguments(WithArguments(property, parameter))));
        }

        private ArgumentSyntax[] WithArguments(PropertyDeclarationSyntax property, ParameterSyntax parameter)
        {
            return GetProperties()
                .Select(p => Argument(p != property ? IdentifierName(p.Identifier) : IdentifierName(parameter.Identifier)))
                .ToArray();
        }

        private ParameterSyntax CreateConstructorParameterForProperty(PropertyDeclarationSyntax property)
        {
            var parameterType = property.Initializer == null || IsNullableProperty(property)
                ? property.Type
                : NullableType(property.Type);

            var @default = !IsRequiredProperty(property)
                ? EqualsValueClause(DefaultExpression(parameterType))
                : null;

            return Parameter(Identifier(ToCamelCase(property.Identifier.Text)))
                .WithType(parameterType)
                .WithDefault(@default);
        }

        private bool IsRequiredProperty(PropertyDeclarationSyntax p)
        {
            return !IsNullableProperty(p) && p.Initializer == null;
        }

        private bool IsNullableProperty(PropertyDeclarationSyntax property)
        {
            return semanticModel.GetDeclaredSymbol(property).NullableAnnotation == NullableAnnotation.Annotated;
        }

        private StatementSyntax CreateAssignment(PropertyDeclarationSyntax property, ParameterSyntax parameter)
        {
            if (property.Initializer == null)
                return SimpleAssignment(IdentifierName(property.Identifier), IdentifierName(parameter.Identifier));

            var type = semanticModel.GetTypeInfo(property.Type).Type;

            var value = type == null || type.IsReferenceType
                ? (ExpressionSyntax)IdentifierName(parameter.Identifier)
                : GetProperty(parameter, "Value");

            return IfStatement(NotNull(parameter), SimpleAssignment(IdentifierName(property.Identifier), value));
        }

        private static MemberAccessExpressionSyntax GetProperty(ParameterSyntax parameter, string propertyName)
        {
            return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(parameter.Identifier), IdentifierName(propertyName));
        }

        private static BinaryExpressionSyntax NotNull(ParameterSyntax parameter)
        {
            return BinaryExpression(SyntaxKind.NotEqualsExpression, IdentifierName(parameter.Identifier), LiteralExpression(SyntaxKind.NullLiteralExpression));
        }

        private static ExpressionStatementSyntax SimpleAssignment(ExpressionSyntax left, ExpressionSyntax right)
        {
            var assignment = AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
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
