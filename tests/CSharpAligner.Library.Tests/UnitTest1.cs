namespace CSharpAligner.Library.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var code = """
        namespace CSharpAligner.Library;

        public static class TExt
        {
            public static void M(this string value, in string p1, ref string p2)
            {
            }
        }
        """;

        //var formatedCode = Formatter.Format(code);
    }
}