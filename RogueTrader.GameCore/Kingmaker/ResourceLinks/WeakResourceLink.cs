using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ResourceManagement;
using Kingmaker.Utility.BuildModeUtils;
using MemoryPack;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.ResourceLinks;

[Serializable]
public abstract class WeakResourceLink : IEquatable<WeakResourceLink>, IHashable
{
	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("m_AssetId")]
	[FormerlySerializedAs("guid")]
	public string AssetId;

	public abstract UnityEngine.Object LoadObject();

	private static bool Equals(WeakResourceLink l1, WeakResourceLink l2)
	{
		if ((object)l1 == l2)
		{
			return true;
		}
		if ((object)l1 == null || (object)l2 == null)
		{
			return false;
		}
		return l1.AssetId == l2.AssetId;
	}

	public bool Equals(WeakResourceLink obj)
	{
		return Equals(this, obj);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as WeakResourceLink);
	}

	public static bool operator ==(WeakResourceLink obj1, WeakResourceLink obj2)
	{
		return Equals(obj1, obj2);
	}

	public static bool operator !=(WeakResourceLink obj1, WeakResourceLink obj2)
	{
		return !Equals(obj1, obj2);
	}

	public override int GetHashCode()
	{
		return AssetId.GetHashCode();
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
public abstract class WeakResourceLink<T> : WeakResourceLink, IHashable where T : UnityEngine.Object
{
	[MemoryPackIgnore]
	protected BundledResourceHandle<T> m_Handle { get; set; }

	[CanBeNull]
	[MemoryPackIgnore]
	public T Loaded
	{
		get
		{
			if (string.IsNullOrEmpty(AssetId))
			{
				return null;
			}
			if (m_Handle != null && m_Handle.IsHeld && ResourcesLibrary.HasLoadedResource(AssetId))
			{
				return m_Handle.Object;
			}
			return null;
		}
	}

	public override UnityEngine.Object LoadObject()
	{
		return Load();
	}

	public void ForceUnload()
	{
		if (!string.IsNullOrEmpty(AssetId))
		{
			ResourcesLibrary.ForceUnloadResource(AssetId);
			m_Handle = null;
		}
	}

	public void DestroyButDontUnload()
	{
		if (m_Handle != null)
		{
			if (m_Handle.Object != null)
			{
				UnityEngine.Object.Destroy(m_Handle.Object);
			}
			m_Handle?.Dispose();
			m_Handle = null;
		}
	}

	[CanBeNull]
	public T Load(bool ignorePreloadWarning = false, bool hold = false)
	{
		if (string.IsNullOrEmpty(AssetId))
		{
			return null;
		}
		if (m_Handle != null && m_Handle.IsHeld && ResourcesLibrary.HasLoadedResource(AssetId))
		{
			return m_Handle.Object;
		}
		m_Handle?.Dispose();
		m_Handle = BundledResourceHandle<T>.Request(AssetId, hold);
		return m_Handle.Object;
	}

	[CanBeNull]
	public async Task<T> LoadAsync(bool ignorePreloadWarning = false, bool hold = false)
	{
		if (string.IsNullOrEmpty(AssetId))
		{
			return null;
		}
		if (m_Handle != null && m_Handle.IsHeld && ResourcesLibrary.HasLoadedResource(AssetId))
		{
			return m_Handle.Object;
		}
		m_Handle?.Dispose();
		m_Handle = await BundledResourceHandle<T>.RequestAsync(AssetId, hold);
		return m_Handle.Object;
	}

	public void Preload()
	{
		StartupJson data = BuildModeUtility.Data;
		if ((data == null || !data.DisablePreload) && !string.IsNullOrEmpty(AssetId))
		{
			ResourcesLibrary.PreloadResource<T>(AssetId);
		}
	}

	public bool Exists()
	{
		return !string.IsNullOrEmpty(AssetId);
	}

	[CanBeNull]
	public T LoadAsset()
	{
		return Load();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
