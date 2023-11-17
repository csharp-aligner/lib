namespace CSharpAligner.Library;

public class FormattingOptions
{
	/// <summary>
	/// 
	/// </summary>
	public string Indentation { get; } 

	/// <summary>
	/// Размер отступа исчисляемый в пробелах
	/// </summary>
	public int IndentationSize { get; }

	/// <summary>
	/// Размер табуляции исчисляемый в пробелах
	/// </summary>
	public int TabSize { get; } 

	/// <summary>
	/// Использовать для отступов табуляцию вместо пробелов
	/// </summary>
	public bool UseTabs { get; }

    public FormattingOptions(int indentationSize, int tabSize, bool useTabs)
    {
		IndentationSize = indentationSize;
		TabSize = tabSize;
		UseTabs = useTabs;

		Indentation = GetIndentation(IndentationSize, 0);
    }

    public string GetIndentation (int existingIndentationLength)
	{
		return GetIndentation (IndentationSize, existingIndentationLength);
	}

	public string GetCustomIndentation (int size)
	{
		return GetIndentation (0, size);
	}

	private string GetIndentation (int baseIndentationSize, int existingIndentationSize)
	{
        var indentationSize = baseIndentationSize + existingIndentationSize;

        return UseTabs
                ? new string('\t', count: indentationSize % TabSize) +
                  new string(' ', indentationSize - (TabSize * (indentationSize % TabSize)))
                : new string(' ', indentationSize);
    }
}
