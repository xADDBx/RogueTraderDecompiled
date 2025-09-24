using System;
using System.Collections.Generic;
using Rewired.Utils.Libraries.TinyJson;
using UnityEngine;

namespace Rewired.Localization;

[AddComponentMenu("Rewired/Localization/Localized String Provider")]
public class LocalizedStringProvider : LocalizedStringProviderBase
{
	[SerializeField]
	[Tooltip("A JSON file containing localizied string key value pairs.")]
	private TextAsset _localizedStringsFile;

	[NonSerialized]
	private Dictionary<string, string> _dictionary = new Dictionary<string, string>();

	[NonSerialized]
	private bool _initialized;

	protected virtual Dictionary<string, string> dictionary
	{
		get
		{
			return _dictionary;
		}
		set
		{
			_dictionary = value;
		}
	}

	public virtual TextAsset localizedStringsFile
	{
		get
		{
			return _localizedStringsFile;
		}
		set
		{
			_localizedStringsFile = value;
			Reload();
		}
	}

	protected override bool initialized => _initialized;

	protected override bool Initialize()
	{
		_initialized = TryLoadLocalizedStringData();
		return _initialized;
	}

	protected virtual bool TryLoadLocalizedStringData()
	{
		_dictionary.Clear();
		if (_localizedStringsFile != null)
		{
			try
			{
				_dictionary = JsonParser.FromJson<Dictionary<string, string>>(_localizedStringsFile.text);
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		return _dictionary.Count > 0;
	}

	protected override bool TryGetLocalizedString(string key, out string result)
	{
		if (!_initialized)
		{
			result = null;
			return false;
		}
		return _dictionary.TryGetValue(key, out result);
	}
}
