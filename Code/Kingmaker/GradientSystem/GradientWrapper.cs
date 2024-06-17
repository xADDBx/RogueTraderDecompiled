using UnityEngine;

namespace Kingmaker.GradientSystem;

public class GradientWrapper
{
	public readonly Gradient Gradient;

	public string TextureGuid;

	public GradientWrapper(Gradient gradient)
	{
		Gradient = new Gradient();
		Gradient.SetKeys(gradient.colorKeys, gradient.alphaKeys);
		Gradient.mode = gradient.mode;
	}

	public override bool Equals(object obj)
	{
		if (obj is GradientWrapper gradientWrapper)
		{
			for (float num = 0f; num < 1f; num += 1f / 32f)
			{
				if (!CompareColors(Gradient.Evaluate(num), gradientWrapper.Gradient.Evaluate(num)))
				{
					return false;
				}
			}
			return CompareColors(Gradient.Evaluate(1f), gradientWrapper.Gradient.Evaluate(1f));
		}
		return false;
	}

	public override int GetHashCode()
	{
		return GetColorShort(Gradient.Evaluate(0f)) << 16 + GetColorShort(Gradient.Evaluate(1f));
	}

	private static bool CompareColors(Color color1, Color color2)
	{
		return (new Vector4(color1.r, color1.g, color1.b, color1.a) - new Vector4(color2.r, color2.g, color2.b, color2.a)).magnitude < 1f / 128f;
	}

	private static ushort GetColorShort(Color32 color)
	{
		int num = 0;
		int num2 = 4;
		for (int i = 0; i < 4; i++)
		{
			int num3 = color[i] >> num2;
			num += num3 << (3 - i) * num2;
		}
		return (ushort)num;
	}
}
