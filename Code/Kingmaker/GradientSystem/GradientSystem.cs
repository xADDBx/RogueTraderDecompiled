using UnityEngine;

namespace Kingmaker.GradientSystem;

public static class GradientSystem
{
	public enum Mode
	{
		Runtime,
		Prebake,
		Bake
	}

	public const float COLOR_EQUAL_COEF = 1f / 128f;

	public const float STEP_X_GRADIENT_EQUAL = 1f / 32f;

	public const string FX_PATH = "Assets\\FX\\Bundles\\Prefabs\\";

	public const string TEXTURE_GRADIENTS_PATH = "Assets\\Art\\FX\\Textures\\Color Alpha Ramps\\AutoGen";

	private static GradientMap s_GradientMap = new GradientMap();

	public static bool IsBakingProcess = false;

	public static Texture2D GetTextureFromGradient(Gradient gradient)
	{
		bool isNewTexure;
		return GetTextureFromGradient(gradient, Mode.Runtime, out isNewTexure);
	}

	public static Texture2D GetTextureFromGradient(Gradient gradient, Mode mode, out bool isNewTexure)
	{
		return s_GradientMap.GetTextureFromGradient(gradient, mode, out isNewTexure);
	}

	public static void Clear()
	{
		s_GradientMap.Clear();
	}

	public static void ReloadFromFiles()
	{
		s_GradientMap.ReloadFromFiles();
	}

	public static Texture2D GenerateTextureFromGradient(Gradient gradient)
	{
		Texture2D texture2D = new Texture2D(256, 1);
		if (!Application.isPlaying)
		{
			texture2D.hideFlags = HideFlags.DontSave;
		}
		Color[] array = new Color[256];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = gradient.Evaluate((float)i / 255f);
		}
		texture2D.SetPixels(array);
		texture2D.Apply(updateMipmaps: true);
		return texture2D;
	}
}
