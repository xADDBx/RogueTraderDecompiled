using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings.GameLog;

[Serializable]
public class ColorTextString
{
	public LocalizedString String;

	public Color32 Color;

	public string GetColorText()
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(Color) + ">" + String.Text + "</color>";
	}
}
