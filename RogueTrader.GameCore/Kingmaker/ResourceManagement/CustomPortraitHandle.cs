using Kingmaker.Enums;
using UnityEngine;

namespace Kingmaker.ResourceManagement;

public class CustomPortraitHandle
{
	private readonly string m_FilePath;

	private readonly ResourceStorage<Sprite> m_Storage;

	public PortraitType PortraitType { get; }

	public Sprite Sprite
	{
		get
		{
			if (Request == null)
			{
				Load();
			}
			return Request?.Resource;
		}
	}

	public SpriteLoadingRequest Request { get; private set; }

	public CustomPortraitHandle(string path, PortraitType type, ResourceStorage<Sprite> storage)
	{
		PortraitType = type;
		m_FilePath = path;
		m_Storage = storage;
	}

	public Sprite Load()
	{
		Request = m_Storage.Load(m_FilePath) as SpriteLoadingRequest;
		return Sprite;
	}

	public void LoadAsync()
	{
		Request = m_Storage.LoadAsync(m_FilePath) as SpriteLoadingRequest;
	}
}
