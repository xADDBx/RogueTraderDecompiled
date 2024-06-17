using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Kingmaker.ResourceManagement;

public class SpriteLoadingRequest : IResourceLoadingRequest<Sprite>
{
	private readonly string m_FilePath;

	private IEnumerator m_LoadingCoroutine;

	public Sprite Resource { get; private set; }

	public bool CanLoad => File.Exists(m_FilePath);

	public event Action Loaded;

	public SpriteLoadingRequest(string filePath)
	{
		m_FilePath = filePath;
	}

	public IEnumerator LoadRoutine()
	{
		return m_LoadingCoroutine ?? (m_LoadingCoroutine = GetLoadingRoutine());
	}

	public void Load()
	{
		byte[] data = File.ReadAllBytes(m_FilePath);
		Texture2D texture2D = new Texture2D(2, 2, TextureFormat.BGRA32, mipChain: false);
		texture2D.LoadImage(data);
		Resource = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f), 200f);
		this.Loaded?.Invoke();
	}

	public void SetPriority(ResourceLoadingPriority priority)
	{
	}

	private IEnumerator GetLoadingRoutine()
	{
		Texture2D tex = null;
		using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(m_FilePath))
		{
			yield return uwr.SendWebRequest();
			if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
			{
				PFLog.Resources.Error("Failed to sprite by path: " + m_FilePath + " error: " + uwr.error);
			}
			else
			{
				tex = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
			}
		}
		Resource = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0f, 0f), 200f);
		this.Loaded?.Invoke();
	}
}
