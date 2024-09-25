using UnityEngine;

namespace Owlcat.Runtime.Core.Utility.EditorAttributes;

public class InfoBoxAttribute : PropertyAttribute
{
	public static bool Disabled;

	public string Text;

	public InfoBoxAttribute()
	{
	}

	public InfoBoxAttribute(string text)
	{
		Text = text;
	}
}
