using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Async;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.EntitySystem.Persistence;

public class SaveScreenshotManager : MonoBehaviour
{
	private class LoadTextureTaskData
	{
		public SaveInfo Save;

		public bool HighRes;

		public byte[] Bytes;

		public Action Callback;

		public string FileName => Save.FolderName + (HighRes ? "highres.png" : "header.png");
	}

	private const int HighResWidth = 1192;

	private const int HighResHeight = 672;

	private const int LowResWidth = 376;

	private const int LowResHeight = 240;

	private static SaveScreenshotManager s_Instance;

	private readonly Dictionary<string, Texture2D> m_LoadedTextures = new Dictionary<string, Texture2D>();

	private readonly List<Task<byte[]>> m_LoadScreenshotTasks = new List<Task<byte[]>>();

	private readonly List<LoadTextureTaskData> m_LoadsQueue = new List<LoadTextureTaskData>();

	private readonly Queue<LoadTextureTaskData> m_TextureQueue = new Queue<LoadTextureTaskData>();

	public static SaveScreenshotManager Instance
	{
		get
		{
			if (!s_Instance)
			{
				s_Instance = new GameObject("[SaveScreenshots]").AddComponent<SaveScreenshotManager>();
				UnityEngine.Object.DontDestroyOnLoad(s_Instance.gameObject);
				s_Instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
			}
			return s_Instance;
		}
	}

	public bool HasTasksRunning => m_LoadScreenshotTasks.Count > 0;

	public static Texture2D MakeScreenshotLowResOnly()
	{
		RenderTexture renderTexture = CameraStackScreenshoter.TakeScreenshotNoResize();
		(int Width, int Height) lowResScreenshotSize = GetLowResScreenshotSize();
		int item = lowResScreenshotSize.Width;
		int item2 = lowResScreenshotSize.Height;
		RenderTexture renderTexture2 = CreateTemporaryResizeRT(item, item2);
		Texture2D result = MakeResizedTexture(renderTexture, item, item2, renderTexture2);
		RenderTexture.ReleaseTemporary(renderTexture2);
		RenderTexture.ReleaseTemporary(renderTexture);
		return result;
	}

	public static Texture2D MakeScreenshotHighResOnly()
	{
		RenderTexture resizeRT;
		Texture2D result = MakeScreenshotHighResInternal(out resizeRT);
		RenderTexture.ReleaseTemporary(resizeRT);
		return result;
	}

	public static (Texture2D HighRes, Texture2D LowRes) MakeScreenshot()
	{
		RenderTexture resizeRT;
		Texture2D texture2D = MakeScreenshotHighResInternal(out resizeRT);
		(int Width, int Height) lowResScreenshotSize = GetLowResScreenshotSize();
		int item = lowResScreenshotSize.Width;
		int item2 = lowResScreenshotSize.Height;
		Texture2D item3 = MakeResizedTexture(texture2D, item, item2, resizeRT);
		RenderTexture.ReleaseTemporary(resizeRT);
		return (HighRes: texture2D, LowRes: item3);
	}

	private static Texture2D MakeScreenshotHighResInternal(out RenderTexture resizeRT)
	{
		RenderTexture renderTexture = CameraStackScreenshoter.TakeScreenshotNoResize();
		(int Width, int Height) highResScreenshotSize = GetHighResScreenshotSize(renderTexture);
		int item = highResScreenshotSize.Width;
		int item2 = highResScreenshotSize.Height;
		resizeRT = CreateTemporaryResizeRT(item, item2);
		Texture2D result = MakeResizedTexture(renderTexture, item, item2, resizeRT);
		RenderTexture.ReleaseTemporary(renderTexture);
		return result;
	}

	private static (int Width, int Height) GetHighResScreenshotSize(Texture srcTexture)
	{
		if (1192 >= srcTexture.width)
		{
			return (Width: (srcTexture.width / 4 + 1) * 4, Height: (srcTexture.height / 4 + 1) * 4);
		}
		return (Width: 1192, Height: 672);
	}

	private static (int Width, int Height) GetLowResScreenshotSize()
	{
		return (Width: 376, Height: 240);
	}

	private static RenderTexture CreateTemporaryResizeRT(int width, int height)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
		temporary.name = $"Screenshot Resize RT {width}x{height}";
		return temporary;
	}

	private static Texture2D MakeResizedTexture(Texture srcTexture, int width, int height, RenderTexture resizeRT)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = resizeRT;
		srcTexture.filterMode = FilterMode.Bilinear;
		Rect screenRect = CalcRectToFit(width, height, srcTexture.width, srcTexture.height);
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
		{
			screenRect.y = resizeRT.height - height;
		}
		GL.PushMatrix();
		GL.LoadPixelMatrix(0f, resizeRT.width, resizeRT.height, 0f);
		GL.Clear(clearDepth: true, clearColor: true, new Color(0f, 0f, 0f, 0f));
		Graphics.DrawTexture(screenRect, srcTexture);
		GL.PopMatrix();
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false, linear: false, createUninitialized: true);
		texture2D.name = $"Save screenshot {width}x{height}";
		texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, recalculateMipMaps: false);
		texture2D.Apply(updateMipmaps: false);
		RenderTexture.active = active;
		return texture2D;
	}

	private static Rect CalcRectToFit(int dstWidth, int dstHeight, int srcWidth, int srcHeight)
	{
		float num = Mathf.Max((float)dstWidth / (float)srcWidth, (float)dstHeight / (float)srcHeight);
		float num2 = (float)srcWidth * num;
		float num3 = (float)srcHeight * num;
		return new Rect(((float)dstWidth - num2) / 2f, ((float)dstHeight - num3) / 2f, num2, num3);
	}

	public static void CompressScreenshot(Texture2D texture)
	{
		texture.Compress(highQuality: false);
		texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
	}

	public async void LoadScreenshot(SaveInfo save, bool highRes, Action callback)
	{
		if (string.IsNullOrEmpty(save.FolderName))
		{
			return;
		}
		string filename = save.FolderName + (highRes ? "highres.png" : "header.png");
		if (m_LoadedTextures.TryGetValue(filename, out var value) && (bool)value)
		{
			if (highRes)
			{
				save.ScreenshotHighRes = value;
			}
			else
			{
				save.Screenshot = value;
			}
			callback?.Invoke();
			return;
		}
		if (m_LoadsQueue.Contains((LoadTextureTaskData d) => d.FileName == filename))
		{
			LoadTextureTaskData loadTextureTaskData2 = m_LoadsQueue.Single((LoadTextureTaskData d) => d.FileName == filename);
			loadTextureTaskData2.Callback = (Action)Delegate.Combine(loadTextureTaskData2.Callback, callback);
			return;
		}
		LoadTextureTaskData loadTextureTaskData = new LoadTextureTaskData
		{
			Save = save,
			HighRes = highRes,
			Callback = callback
		};
		m_LoadsQueue.Add(loadTextureTaskData);
		Task<byte[]> task = LoadScreenshotRoutine(loadTextureTaskData);
		m_LoadScreenshotTasks.Add(task);
		LoadTextureTaskData loadTextureTaskData3 = loadTextureTaskData;
		loadTextureTaskData3.Bytes = await task;
		m_LoadsQueue.Remove(loadTextureTaskData);
	}

	private async Task<byte[]> LoadScreenshotRoutine(LoadTextureTaskData loadTextureTaskData)
	{
		try
		{
			using ISaver saver = loadTextureTaskData.Save.Saver.Clone();
			loadTextureTaskData.Save.Saver = saver;
			using (loadTextureTaskData.Save.GetReadScope())
			{
				m_TextureQueue.Enqueue(loadTextureTaskData);
				return saver.ReadBytes(loadTextureTaskData.HighRes ? "highres.png" : "header.png");
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return null;
		}
	}

	private void Update()
	{
		if (HasTasksRunning)
		{
			m_LoadScreenshotTasks.RemoveAll((Task<byte[]> t) => t.IsCompleted);
		}
		if (m_TextureQueue.Count > 0)
		{
			LoadTextureTaskData loadTextureTaskData = m_TextureQueue.Dequeue();
			Texture2D texture2D = (loadTextureTaskData.HighRes ? loadTextureTaskData.Save.ScreenshotHighRes : loadTextureTaskData.Save.Screenshot);
			if ((bool)texture2D)
			{
				UnityEngine.Object.Destroy(texture2D);
				texture2D = null;
			}
			if (loadTextureTaskData.Bytes != null)
			{
				texture2D = new Texture2D(4, 4, TextureFormat.DXT1, mipChain: false)
				{
					name = "Save screenshot (Update)"
				};
				texture2D.LoadImage(loadTextureTaskData.Bytes);
				texture2D.Compress(highQuality: false);
				texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			}
			if (loadTextureTaskData.HighRes)
			{
				loadTextureTaskData.Save.ScreenshotHighRes = texture2D;
			}
			else
			{
				loadTextureTaskData.Save.Screenshot = texture2D;
			}
			if (texture2D != null)
			{
				m_LoadedTextures[loadTextureTaskData.FileName] = texture2D;
			}
			loadTextureTaskData.Callback?.Invoke();
		}
	}

	public async void DisposeScreenshotTexture(Texture2D screenshot)
	{
		await Awaiters.UnityThread;
		if (screenshot != null)
		{
			UnityEngine.Object.Destroy(screenshot);
		}
	}

	public void Cleanup()
	{
		m_TextureQueue.Clear();
		m_LoadsQueue.Clear();
		foreach (KeyValuePair<string, Texture2D> loadedTexture in m_LoadedTextures)
		{
			UnityEngine.Object.Destroy(loadedTexture.Value);
		}
		m_LoadedTextures.Clear();
	}
}
