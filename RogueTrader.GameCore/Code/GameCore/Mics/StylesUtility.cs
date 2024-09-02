using System.Drawing;
using UnityEngine;

namespace Code.GameCore.Mics;

public static class StylesUtility
{
	public static UnityEngine.Color ToUnityColor(this System.Drawing.Color color)
	{
		return new UnityEngine.Color((float)(int)color.R / 255f, (float)(int)color.G / 255f, (float)(int)color.B / 255f);
	}
}
