using System;
using System.IO;
using Core.Cheats;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.Utility;

public static class Screenshot
{
	public static void Initialize(KeyboardAccess keyboard)
	{
		string screenshotPath = GetScreenshotPath();
		if (!Directory.Exists(screenshotPath))
		{
			Directory.CreateDirectory(screenshotPath);
		}
		keyboard.Bind("screenshot", CaptureDefault);
	}

	private static string GetScreenshotPath()
	{
		return Path.Combine(ApplicationPaths.persistentDataPath, "Screenshots");
	}

	private static string GetScreenshotName()
	{
		return $"{DateTime.Now:yyyyMMddHHmmssffff}.png";
	}

	private static void DoCapture(string path)
	{
		PFLog.Default.Log("Capture screenshot to " + path);
		ScreenCapture.CaptureScreenshot(path);
	}

	private static void CaptureDefault()
	{
		DoCapture(Path.Combine(GetScreenshotPath(), GetScreenshotName()));
	}

	[Cheat(Name = "screenshot", Description = "capture screenshot")]
	public static void Capture(string path = null, string name = null)
	{
		UberLoggerAppWindow.Instance.IsShown = false;
		DoCapture(Path.Combine(path ?? GetScreenshotPath(), name ?? GetScreenshotName()));
	}

	public static byte[] CapturePNG(Camera camera = null, int width = 0, int height = 0)
	{
		width = ((width == 0) ? Screen.width : width);
		height = ((height == 0) ? Screen.height : height);
		RenderTexture targetTexture = (camera ? camera.targetTexture : null);
		if ((bool)camera)
		{
			camera.targetTexture = new RenderTexture(width, height, 24);
			RenderTexture.active = camera.targetTexture;
			camera.Render();
		}
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		texture2D.Apply();
		if ((bool)camera)
		{
			camera.targetTexture = targetTexture;
			RenderTexture.active = null;
		}
		return texture2D.EncodeToPNG();
	}
}
