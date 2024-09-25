namespace Kingmaker.Blueprints.JsonSystem.PropertyUtility.Helper;

public static class FieldFromPropertyHelper
{
	public static string GetAssemblyQualifiedNameFromManagedTypeName(string n)
	{
		if (string.IsNullOrEmpty(n))
		{
			return "";
		}
		int num = n.IndexOf(' ');
		string text = n.Substring(0, num);
		n = n.Substring(num + 1);
		n = n.Replace('/', '+');
		return n + ", " + text;
	}
}
