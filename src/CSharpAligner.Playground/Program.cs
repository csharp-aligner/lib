using CSharpAligner.Library;

var code = """
namespace CSharpAligner.Library;

public static class TExt
{
    public static void M(this string value, ref string p1, string p2)
    {        
    }
}
""";

var options = new FormattingOptions
    (indentationSize: 4,
     tabSize: 4,
     useTabs: false);

var formatedCode = Formatter.Format(code, options);

Console.WriteLine(formatedCode);