using System;
using Core.Async;
using JetBrains.Annotations;
using Kingmaker.Localization.Enums;
using Kingmaker.Localization.Shared;
using Kingmaker.TextTools.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Localization;

[Serializable]
public class LocalizedString
{
	private const string JsonExt = ".json";

	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Localization");

	[SerializeField]
	private string m_Key = "";

	private bool? m_ShouldProcess;

	private Locale m_ShouldProcessLocale;

	private bool m_IsReportedAsMissingString;

	[CanBeNull]
	public SharedStringAsset Shared;

	public string Key
	{
		get
		{
			return m_Key;
		}
		set
		{
			m_Key = value;
		}
	}

	public string Text => GetText();

	private string GetText()
	{
		using (ProfileScope.New("Localized string"))
		{
			if (IsEmpty())
			{
				return string.Empty;
			}
			if (!LoadImpl(out var txt) && !m_IsReportedAsMissingString)
			{
				m_IsReportedAsMissingString = true;
				Logger.Error("missing string: {0}", m_Key);
			}
			if (!UnitySyncContextHolder.IsInUnity || !Application.isPlaying)
			{
				return txt;
			}
			if (!m_ShouldProcess.HasValue || m_ShouldProcessLocale != LocalizationManager.Instance.CurrentLocale)
			{
				m_ShouldProcess = txt.Contains("{");
				m_ShouldProcessLocale = LocalizationManager.Instance.CurrentLocale;
			}
			using (ProfileScope.New("Process templates"))
			{
				return TextTemplateEngineProxy.Instance.Process(txt);
			}
		}
	}

	private bool LoadImpl(out string txt)
	{
		if (LocalizationManager.Instance.CurrentPack != null && TryGetText(LocalizationManager.Instance.CurrentPack, out txt))
		{
			return true;
		}
		LocalizationPack backupPack = LocalizationManager.Instance.BackupPack;
		if (backupPack != null && TryGetText(backupPack, out txt))
		{
			txt = $"[{backupPack.Locale}] {txt}";
			return true;
		}
		txt = string.Empty;
		return false;
	}

	public static implicit operator string(LocalizedString localizedString)
	{
		if (localizedString != null)
		{
			return localizedString.Text;
		}
		return "<null>";
	}

	public static LocalizedString Dereference(LocalizedString ls)
	{
		int num = 0;
		while ((bool)ls.Shared)
		{
			if (num++ > 50)
			{
				return null;
			}
			ls = ls.Shared.String;
		}
		return ls;
	}

	private bool TryGetText([NotNull] LocalizationPack pack, out string text)
	{
		LocalizedString localizedString = Dereference(this);
		if (localizedString == null)
		{
			Logger.Error("Cyclic reference in string {0}", this);
			text = "";
			return false;
		}
		string key = localizedString.m_Key;
		if (key == "")
		{
			text = "";
			return false;
		}
		return pack.TryGetText(key, out text);
	}

	public string GetVoiceOverSound()
	{
		if (LocalizationManager.Instance.SoundPack != null && TryGetText(LocalizationManager.Instance.SoundPack, out var text))
		{
			return text;
		}
		return "";
	}

	public string ToString(Action scope)
	{
		using (GameLogContext.Scope)
		{
			try
			{
				scope();
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
			}
			return this;
		}
	}

	public bool IsSet()
	{
		if (LocalizationManager.Instance.CurrentPack != null && TryGetText(LocalizationManager.Instance.CurrentPack, out var text) && !string.IsNullOrEmpty(text))
		{
			return true;
		}
		return false;
	}

	public bool IsEmpty()
	{
		if (!Shared)
		{
			return m_Key == "";
		}
		return Shared.String.IsEmpty();
	}
}
