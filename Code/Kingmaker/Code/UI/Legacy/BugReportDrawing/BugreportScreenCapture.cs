using System.IO;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.Legacy.BugReportDrawing;

public static class BugreportScreenCapture
{
	public static void SaveImage(Texture2D lower, Texture2D upper, Texture2D original)
	{
		byte[] bytes = MixTextures(lower, upper).EncodeToPNG();
		File.WriteAllBytes(GetSavePath(), bytes);
	}

	private static string GetSavePath()
	{
		string currentReportFolder = ReportingUtils.Instance.CurrentReportFolder;
		if (!Directory.Exists(currentReportFolder))
		{
			Directory.CreateDirectory(currentReportFolder);
		}
		return currentReportFolder + "/edited_screen.png";
	}

	private static Texture2D MixTextures(Texture2D lower, Texture2D upper)
	{
		return AlphaBlend(lower, upper);
	}

	private static Texture2D AlphaBlend(Texture2D aBottom, Texture2D aTop)
	{
		int width = aBottom.width;
		int height = aBottom.height;
		int width2 = aTop.width;
		int height2 = aTop.height;
		Scale(aTop, width, height);
		if (aBottom.width != aTop.width || aBottom.height != aTop.height)
		{
			return aBottom;
		}
		Color[] pixels = aBottom.GetPixels();
		Color[] pixels2 = aTop.GetPixels();
		int num = pixels.Length;
		Color[] array = new Color[num];
		for (int i = 0; i < num; i++)
		{
			Color color = pixels[i];
			Color color2 = pixels2[i];
			float a = color2.a;
			float num2 = 1f - color2.a;
			float num3 = a + num2 * color.a;
			Color color3 = (color2 * a + color * color.a * num2) / num3;
			color3.a = num3;
			array[i] = color3;
		}
		Texture2D texture2D = new Texture2D(aTop.width, aTop.height);
		texture2D.SetPixels(array);
		texture2D.Apply();
		Scale(aTop, width2, height2);
		return texture2D;
	}

	private static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
	{
		Rect source = new Rect(0f, 0f, width, height);
		GpuScale(tex, width, height, mode);
		tex.Reinitialize(width, height);
		tex.ReadPixels(source, 0, 0, recalculateMipMaps: true);
		tex.Apply(updateMipmaps: true);
	}

	private static void GpuScale(Texture2D src, int width, int height, FilterMode fmode)
	{
		src.filterMode = fmode;
		src.Apply(updateMipmaps: true);
		Graphics.SetRenderTarget(new RenderTexture(width, height, 32));
		GL.LoadPixelMatrix(0f, 1f, 1f, 0f);
		GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
		Graphics.DrawTexture(new Rect(0f, 0f, 1f, 1f), src);
	}
}
