namespace Kingmaker.Utility.DotNetExtensions;

public static class StringUtility
{
	public static bool IsNullOrInvisible(string str)
	{
		return string.IsNullOrWhiteSpace(str);
	}
}
