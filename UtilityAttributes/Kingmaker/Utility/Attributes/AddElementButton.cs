using System;

namespace Kingmaker.Utility.Attributes;

public class AddElementButton : Attribute
{
	public string Text { get; set; }

	public AddElementButton(string text)
	{
		Text = text;
	}
}
