using System;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using UnityEngine;

namespace Kingmaker.ResourceManagement;

public class BundledResourceHandle<T> : IDisposable where T : UnityEngine.Object
{
	private string m_AssetId;

	private bool m_Held;

	private WeakReference<T> m_Object;

	public T Object
	{
		get
		{
			T target = null;
			if (m_Object != null && !m_Object.TryGetTarget(out target))
			{
				PFLog.Default.Error($"Trying to get Object from BundledResourceHandle, but object already unloaded. AssedId={m_AssetId}, m_Held={m_Held}");
				return null;
			}
			return target;
		}
	}

	public bool IsHeld => m_Held;

	public string AssetId => m_AssetId;

	private BundledResourceHandle()
	{
	}

	public static BundledResourceHandle<T> Request(string assetId, bool hold = false)
	{
		BundledResourceHandle<T> bundledResourceHandle = new BundledResourceHandle<T>();
		bundledResourceHandle.m_AssetId = assetId;
		bundledResourceHandle.m_Held = hold;
		if (string.IsNullOrEmpty(assetId))
		{
			bundledResourceHandle.m_Object = null;
		}
		else
		{
			T val = ResourcesLibrary.TryGetResource<T>(assetId, ignorePreloadWarning: true, hold);
			bundledResourceHandle.m_Object = ((val != null) ? new WeakReference<T>(val) : null);
		}
		return bundledResourceHandle;
	}

	public static async Task<BundledResourceHandle<T>> RequestAsync(string assetId, bool hold = false)
	{
		BundledResourceHandle<T> handle = new BundledResourceHandle<T>
		{
			m_AssetId = assetId,
			m_Held = hold
		};
		BundledResourceHandle<T> bundledResourceHandle = handle;
		WeakReference<T> @object = ((!string.IsNullOrEmpty(assetId)) ? new WeakReference<T>(await ResourcesLibrary.TryGetResourceAsync<T>(assetId, ignorePreloadWarning: true, hold)) : null);
		bundledResourceHandle.m_Object = @object;
		return handle;
	}

	public void Dispose()
	{
		m_Object = null;
		ResourcesLibrary.FreeResourceRequest(m_AssetId, m_Held);
		m_Held = false;
	}
}
