using Microsoft.CodeAnalysis.CSharp;

namespace CSharpAligner.Library;

public static class Formatter
{
    public static string Format(string code, FormattingOptions options)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root       = syntaxTree.GetCompilationUnitRoot();
        
        return new MethodDeclarationRewriter(options)
            .Visit(root)
            .ToString();
    }
}