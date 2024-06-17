namespace Owlcat.QA;

internal static class StringExtensions
{
	internal static string Cut(this string text, int startIndex)
	{
		if (text.Length <= startIndex)
		{
			return text;
		}
		return text.Remove(startIndex);
	}
}
