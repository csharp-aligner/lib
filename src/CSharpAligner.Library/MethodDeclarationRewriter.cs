using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpAligner.Library;


public static class TExt
{
    [Conditional("DEBUG"), Conditional("TEST")]
    public static void M <T1>
        (this string value,
         in string p1,
         ref string p2,
              MethodDeclarationFormatter p3)
    {

    }

    public static void M2<T1>(MethodDeclarationFormatter p1)
    {

    }
}

public class MethodDeclarationFormatter
{
    public void Format()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("");
    }
}

public class MethodDeclarationRewriter(FormattingOptions options) : CSharpSyntaxRewriter
{
    private readonly FormattingOptions _options = options;

    private SyntaxTrivia? _methodIndentationTrivia;
    private bool _methodParameterWrappingRequired;
    private bool _methodHasTypeParameterList;

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax methodDeclaration)
    {
        //----------------------------------------------------------
        // 
        //----------------------------------------------------------        

        _methodIndentationTrivia = GetMethodIndentationTrivia(methodDeclaration);
        _methodParameterWrappingRequired = methodDeclaration.ParameterList.Parameters.Count > 1;
        _methodHasTypeParameterList = methodDeclaration.TypeParameterList is not null;

        var attributeLists = GetAttributeLists(methodDeclaration);
        var modifiers = GetModifiers(methodDeclaration);
        var returnType = GetReturnType(methodDeclaration);
        var explicitInterfaceSpecifier = GetExplicitInterfaceSpecifier(methodDeclaration);
        var identifier = GetIdentifier(methodDeclaration);
        var typeParameterList = GetTypeParameterList(methodDeclaration);
        var parameterList = GetParameterList(methodDeclaration);
        
        methodDeclaration = SyntaxFactory.MethodDeclaration(
            attributeLists,
            modifiers,
            returnType,
            explicitInterfaceSpecifier,
            identifier,
            typeParameterList,
            parameterList,
            methodDeclaration.ConstraintClauses,
            methodDeclaration.Body,
            methodDeclaration.SemicolonToken);

        return base.VisitMethodDeclaration(methodDeclaration);
    }

    private SyntaxTrivia GetMethodIndentationTrivia(MethodDeclarationSyntax methodDeclaration)
    {
        var indentation = _options.Indentation;

        var hasParent = methodDeclaration.Parent is not null;
        if (hasParent)
        {
            if (methodDeclaration.Parent!.HasLeadingTrivia)
            {
                var leadingWhiteSpaceTrivia = methodDeclaration.Parent
                    .GetLeadingTrivia()
                    .Cast<SyntaxTrivia?>()
                    .LastOrDefault(trivia => trivia!.Value.IsKind(SyntaxKind.WhitespaceTrivia));

                if (leadingWhiteSpaceTrivia.HasValue)
                {
                    indentation = _options.GetIndentation(leadingWhiteSpaceTrivia.Value.Span.Length);
                }
            }
        }

        return SyntaxFactory.Whitespace(indentation);
    }

    private SyntaxList<AttributeListSyntax> GetAttributeLists(MethodDeclarationSyntax methodDeclaration)
    {
        var hasAttributes = methodDeclaration.AttributeLists.Count > 0;
        if (hasAttributes is false)
        {
            return methodDeclaration.AttributeLists;
        }
      
        var attributeLists = methodDeclaration.AttributeLists.Select(attributeList =>
        {
            var openBracketToken = SyntaxFactory
                .Token(SyntaxKind.OpenBracketToken)
                .WithLeadingTrivia(_methodIndentationTrivia!.Value);

            var closeBracketToken = SyntaxFactory
                .Token(SyntaxKind.CloseBracketToken)
                .WithTrailingTrivia(SyntaxFactory.LineFeed);

            return SyntaxFactory.AttributeList(
                openBracketToken: openBracketToken,
                target: attributeList.Target,
                attributes: attributeList.Attributes,
                closeBracketToken: closeBracketToken);
        });

        return new(attributeLists);
    }

    private SyntaxTokenList GetModifiers(MethodDeclarationSyntax methodDeclaration)
    {
        var modifiers = methodDeclaration.Modifiers.Select((token, index) =>
        {
            if (index == 0)
            {
                return token
                    .WithLeadingTrivia(_methodIndentationTrivia!.Value)
                    .WithTrailingTrivia(SyntaxFactory.Space);
            }

            return token.WithTrailingTrivia(SyntaxFactory.Space);
        });

        return new(modifiers);
    }

    private TypeSyntax GetReturnType(MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration.ReturnType.WithTrailingTrivia(SyntaxFactory.Space);
    }

    private ExplicitInterfaceSpecifierSyntax? GetExplicitInterfaceSpecifier(MethodDeclarationSyntax methodDeclaration)
    {        
        return methodDeclaration.ExplicitInterfaceSpecifier;
    }

    private SyntaxToken GetIdentifier(MethodDeclarationSyntax methodDeclaration)
    {
        if (_methodParameterWrappingRequired &&
            _methodHasTypeParameterList is false)
        {
            return methodDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.LineFeed);
        }

        return methodDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.Space);
    }

    private TypeParameterListSyntax? GetTypeParameterList(MethodDeclarationSyntax methodDeclaration)
    {
        if (_methodHasTypeParameterList &&
            _methodParameterWrappingRequired)
        {
            return methodDeclaration.TypeParameterList!.WithTrailingTrivia(SyntaxFactory.LineFeed);
        }

        return methodDeclaration.TypeParameterList;
    }

    private ParameterListSyntax GetParameterList(MethodDeclarationSyntax methodDeclaration)
    {
        if (_methodParameterWrappingRequired is false)
        {
            return methodDeclaration.ParameterList.WithTrailingTrivia(SyntaxFactory.LineFeed);
        }

        var openParenTokenIndentation = _options.GetIndentation(_methodIndentationTrivia!.Value.Span.Length);
        var openParenTokenLeadingTrivia = SyntaxFactory.Whitespace(openParenTokenIndentation);

        var openParenToken = SyntaxFactory
            .Token(SyntaxKind.OpenParenToken)
            .WithLeadingTrivia(openParenTokenLeadingTrivia);

        var closeParenToken = SyntaxFactory
            .Token(SyntaxKind.CloseParenToken)
            .WithTrailingTrivia(SyntaxFactory.LineFeed);

        var parametersIndentation = _options.GetCustomIndentation(openParenTokenIndentation.Length + 1);
        var parametersLeadingTrivia = SyntaxFactory.Whitespace(parametersIndentation);

        var parameterModifiersMaxLength = methodDeclaration.ParameterList.Parameters
            .Select(parameter => parameter.Modifiers.Sum(token => token.Span.Length))
            .Max();


        var parameters = methodDeclaration.ParameterList.Parameters.Select((parameter, parameterIndex) =>
        {
            var attributeLists = new SyntaxList<AttributeListSyntax>(
                parameter.AttributeLists.Select((attributeList, index) =>
                {
                    if (index == 0)
                    {
                        return attributeList.WithLeadingTrivia(SyntaxFactory.LineFeed, parametersLeadingTrivia);
                    }

                    return attributeList;
                }));

            var modifiersLength = parameter.Modifiers.Sum(token => token.Span.Length);
            var modifiersMissingLength = parameterModifiersMaxLength - modifiersLength;

            var modifierList = new SyntaxTokenList(
                parameter.Modifiers.Select((modifier, modifierIndex) =>
                {
                    if (parameterIndex != 0 && 
                        modifierIndex == 0 && 
                        attributeLists.Count == 0)
                    {
                        modifier = modifier.WithLeadingTrivia(SyntaxFactory.LineFeed, parametersLeadingTrivia);
                    }

                    var modifierTrailingTrivia = SyntaxFactory.Space;

                    if (modifierIndex == parameter.Modifiers.Count - 1 && modifiersMissingLength != 0)
                    {
                        var modifierTrailingWhitespace = _options.GetCustomIndentation(modifiersMissingLength + 1);                    
                        modifierTrailingTrivia = SyntaxFactory.Whitespace(modifierTrailingWhitespace);
                    }

                    return modifier.WithTrailingTrivia(modifierTrailingTrivia);
                }));           

            var typeLeadingTriviaList = SyntaxFactory.TriviaList();

            var typeIsFirstToken =
                parameter.AttributeLists.Count == 0 &&
                parameter.Modifiers.Count == 0;

            if (typeIsFirstToken && parameterIndex != 0)
            {
                typeLeadingTriviaList = typeLeadingTriviaList
                    .Add(SyntaxFactory.LineFeed)
                    .Add(parametersLeadingTrivia);
            }

            var typeMustHaveWhitespaceFromModifiers =
                modifierList.Count == 0 &&
                modifiersMissingLength != 0;

            if (typeMustHaveWhitespaceFromModifiers)
            {
                var typeLeadingWhitespace = _options.GetCustomIndentation(modifiersMissingLength + 1);
                var typeLeadingTrivia = SyntaxFactory.Whitespace(typeLeadingWhitespace);

                typeLeadingTriviaList = typeLeadingTriviaList.Add(typeLeadingTrivia);
            }

            var type = parameter.Type!.WithLeadingTrivia(typeLeadingTriviaList);
            
            return parameter
                .WithAttributeLists(attributeLists)
                .WithModifiers(modifierList)
                .WithType(type);
        });

        var parameterList = new SeparatedSyntaxList<ParameterSyntax>().AddRange(parameters);
        
        return SyntaxFactory.ParameterList(openParenToken, parameterList, closeParenToken);
    }

}
