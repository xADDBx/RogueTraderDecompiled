using System;

namespace Owlcat.Runtime.Core.Utility.EditorAttributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class ClassInfoBox : Attribute
{
	public readonly string Text;

	public ClassInfoBox(string text)
	{
		Text = text;
	}
}
