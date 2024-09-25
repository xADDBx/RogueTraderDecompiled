using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;

public static class TextFieldParamsExtension
{
	public static void ApplyTextFieldParams(this TextMeshProUGUI textField, TextFieldParams textFieldParams, TextFieldParams defaultParams = null)
	{
		if (!(textField == null) && (textFieldParams != null || defaultParams != null))
		{
			FontStyles? fontStyles = textFieldParams?.FontStyles ?? defaultParams?.FontStyles;
			if (fontStyles.HasValue)
			{
				textField.fontStyle = fontStyles.Value;
			}
			Color? color = textFieldParams?.FontColor ?? defaultParams?.FontColor;
			if (color.HasValue)
			{
				textField.color = color.Value;
			}
			float? num = textFieldParams?.FontSize ?? defaultParams?.FontSize;
			if (num.HasValue)
			{
				textField.fontSize = num.Value;
			}
		}
	}

	public static TextFieldParams GetTextFieldParams(this TextMeshProUGUI textField)
	{
		if (textField == null)
		{
			return null;
		}
		return new TextFieldParams
		{
			FontStyles = textField.fontStyle,
			FontColor = textField.color,
			FontSize = textField.fontSize
		};
	}
}
