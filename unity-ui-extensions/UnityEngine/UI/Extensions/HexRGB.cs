using System.Globalization;

namespace UnityEngine.UI.Extensions;

public class HexRGB : MonoBehaviour
{
	public InputField hexInput;

	public HSVPicker hsvpicker;

	public void ManipulateViaRGB2Hex()
	{
		string text = ColorToHex(hsvpicker.currentColor);
		hexInput.text = text;
	}

	public static string ColorToHex(Color color)
	{
		int num = (int)(color.r * 255f);
		int num2 = (int)(color.g * 255f);
		int num3 = (int)(color.b * 255f);
		return $"#{num:X2}{num2:X2}{num3:X2}";
	}

	public void ManipulateViaHex2RGB()
	{
		string text = hexInput.text;
		Vector3 vector = Hex2RGB(text);
		Color color = NormalizeVector4(vector, 255f, 1f);
		MonoBehaviour.print(vector);
		hsvpicker.AssignColor(color);
	}

	private static Color NormalizeVector4(Vector3 v, float r, float a)
	{
		float r2 = v.x / r;
		float g = v.y / r;
		float b = v.z / r;
		return new Color(r2, g, b, a);
	}

	private Vector3 Hex2RGB(string hexColor)
	{
		if (hexColor.IndexOf('#') != -1)
		{
			hexColor = hexColor.Replace("#", "");
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (hexColor.Length == 6)
		{
			num = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
			num2 = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
			num3 = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
		}
		else if (hexColor.Length == 3)
		{
			num = int.Parse(hexColor[0].ToString() + hexColor[0], NumberStyles.AllowHexSpecifier);
			num2 = int.Parse(hexColor[1].ToString() + hexColor[1], NumberStyles.AllowHexSpecifier);
			num3 = int.Parse(hexColor[2].ToString() + hexColor[2], NumberStyles.AllowHexSpecifier);
		}
		return new Vector3(num, num2, num3);
	}
}
