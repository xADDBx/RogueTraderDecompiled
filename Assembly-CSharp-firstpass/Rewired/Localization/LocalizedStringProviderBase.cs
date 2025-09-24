using Rewired.Interfaces;
using Rewired.Utils;
using UnityEngine;

namespace Rewired.Localization;

public abstract class LocalizedStringProviderBase : MonoBehaviour, ILocalizedStringProvider
{
	[SerializeField]
	[Tooltip("Determines if localized strings should be fetched immediately in bulk when available. If false, strings will be fetched when queried.")]
	private bool _prefetch;

	public virtual bool prefetch
	{
		get
		{
			return _prefetch;
		}
		set
		{
			_prefetch = value;
			if (base.gameObject.activeInHierarchy && base.enabled && ReInput.isReady && ReInput.localization.localizedStringProvider == this)
			{
				ReInput.localization.prefetch = value;
			}
		}
	}

	protected abstract bool initialized { get; }

	protected virtual void OnEnable()
	{
		if (!initialized)
		{
			Initialize();
		}
		TrySetLocalizedStringProvider();
	}

	protected virtual void OnDisable()
	{
		if (ReInput.isReady && ReInput.localization.localizedStringProvider == this)
		{
			ReInput.localization.localizedStringProvider = null;
		}
		ReInput.InitializedEvent -= TrySetLocalizedStringProvider;
	}

	protected virtual void Update()
	{
	}

	protected virtual void TrySetLocalizedStringProvider()
	{
		ReInput.InitializedEvent -= TrySetLocalizedStringProvider;
		ReInput.InitializedEvent += TrySetLocalizedStringProvider;
		if (ReInput.isReady)
		{
			if (!UnityTools.IsNullOrDestroyed(ReInput.localization.localizedStringProvider))
			{
				Debug.LogWarning("A localized string provider is already set. Only one localized string provider can exist at a time.");
				return;
			}
			ReInput.localization.localizedStringProvider = this;
			ReInput.localization.prefetch = _prefetch;
		}
	}

	protected abstract bool Initialize();

	public virtual void Reload()
	{
		Initialize();
		if (base.gameObject.activeInHierarchy && base.enabled && ReInput.isReady && ReInput.localization.localizedStringProvider == this)
		{
			ReInput.localization.Reload();
		}
	}

	protected abstract bool TryGetLocalizedString(string key, out string result);

	bool ILocalizedStringProvider.TryGetLocalizedString(string key, out string result)
	{
		return TryGetLocalizedString(key, out result);
	}
}
