using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Sound;

public static class SoundBanksManager
{
	public class BankHandle
	{
		private Coroutine m_WaitCoroutine;

		public string SoundBank { get; }

		public bool Loaded { get; private set; }

		public bool Loading { get; private set; }

		public int RefsCount { get; private set; }

		public BankHandle(string soundBank)
		{
			SoundBank = soundBank;
		}

		public void LoadAsync()
		{
			RefsCount++;
			if (!Loaded && !Loading)
			{
				Loading = true;
				AkBankManager.LoadBankAsync(SoundBank, OnLoaded);
			}
		}

		public void Load(bool decodeBank, bool saveDecodedBank)
		{
			RefsCount++;
			if (!Loaded && !Loading)
			{
				AkBankManager.LoadBank(SoundBank, decodeBank, saveDecodedBank);
				Loaded = true;
			}
		}

		public void Unload()
		{
			RefsCount = Math.Max(0, RefsCount - 1);
			if (RefsCount <= 0)
			{
				Loading = false;
				Loaded = false;
				StopWaiting();
				AkBankManager.UnloadBank(SoundBank);
			}
		}

		private void OnLoaded(uint in_bankId, IntPtr in_inMemoryBankPtr, AKRESULT in_eLoadResult, object in_cookie)
		{
			if (!Loading)
			{
				Unload();
				return;
			}
			Loading = false;
			switch (in_eLoadResult)
			{
			case AKRESULT.AK_BankAlreadyLoaded:
				m_WaitCoroutine = MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(WaitAndLoad());
				break;
			default:
				Loaded = false;
				RefsCount = 0;
				break;
			case AKRESULT.AK_Success:
				Loaded = true;
				break;
			}
		}

		private IEnumerator WaitAndLoad()
		{
			Loading = true;
			yield return null;
			Loading = false;
			if (!Loaded && RefsCount > 0)
			{
				AkBankManager.LoadBank(SoundBank, decodeBank: false, saveDecodedBank: false);
				Loaded = true;
			}
			m_WaitCoroutine = null;
		}

		private void StopWaiting()
		{
			if (m_WaitCoroutine != null)
			{
				MonoSingleton<CoroutineRunner>.Instance.StopCoroutine(m_WaitCoroutine);
				m_WaitCoroutine = null;
			}
		}
	}

	private static float m_UnloadDelay;

	private static bool m_Unloading;

	[NotNull]
	private static readonly Dictionary<string, BankHandle> s_Handles = new Dictionary<string, BankHandle>();

	[NotNull]
	private static readonly List<string> s_BanksToUnload = new List<string>();

	public static BankHandle LoadBank(string soundBank)
	{
		if (string.IsNullOrEmpty(soundBank))
		{
			return null;
		}
		BankHandle bankHandle = s_Handles.Get(soundBank);
		if (bankHandle == null)
		{
			bankHandle = new BankHandle(soundBank);
			s_Handles[soundBank] = bankHandle;
		}
		bankHandle.LoadAsync();
		return bankHandle;
	}

	public static void LoadBankSync(string soundBank)
	{
		if (!string.IsNullOrEmpty(soundBank))
		{
			BankHandle bankHandle = s_Handles.Get(soundBank);
			if (bankHandle == null)
			{
				bankHandle = new BankHandle(soundBank);
				s_Handles[soundBank] = bankHandle;
			}
			bankHandle.Load(decodeBank: false, saveDecodedBank: false);
		}
	}

	public static void UnloadBank(string soundBank)
	{
		if (!string.IsNullOrEmpty(soundBank))
		{
			s_Handles.Get(soundBank)?.Unload();
		}
	}

	public static void LoadBanks(IEnumerable<string> soundBankNames)
	{
		foreach (string soundBankName in soundBankNames)
		{
			LoadBank(soundBankName);
		}
	}

	public static void MarkBanksToUnload(IEnumerable<string> soundBankNames, float delay = 0f)
	{
		foreach (string soundBankName in soundBankNames)
		{
			BankHandle bankHandle = s_Handles.Get(soundBankName);
			if (bankHandle != null && bankHandle.RefsCount > 0)
			{
				s_BanksToUnload.Add(soundBankName);
			}
		}
		m_UnloadDelay = Math.Max(delay, m_UnloadDelay);
	}

	public static void UnloadNotUsed()
	{
		m_Unloading = true;
	}

	private static void UnloadNotUsedImmediately()
	{
		foreach (string item in s_BanksToUnload)
		{
			UnloadBank(item);
		}
		s_BanksToUnload.Clear();
		m_Unloading = false;
	}

	public static void TryToUnloadBanks()
	{
		if (m_Unloading)
		{
			if (m_UnloadDelay > 0f)
			{
				m_UnloadDelay -= Time.unscaledDeltaTime;
			}
			else
			{
				UnloadNotUsedImmediately();
			}
		}
	}

	[NotNull]
	private static IEnumerable<string> CollectVoiceBanks()
	{
		return from sb in (from b in BlueprintRoot.Instance.CharGenRoot.Voices
				select b.GetComponent<UnitAsksComponent>() into c
				where c != null
				select c).SelectMany((UnitAsksComponent c) => c.SoundBanks)
			where !string.IsNullOrEmpty(sb)
			select sb;
	}

	public static void LoadVoiceBanks()
	{
		foreach (string item in CollectVoiceBanks())
		{
			LoadBank(item);
		}
	}

	public static void UnloadVoiceBanks()
	{
		CollectVoiceBanks().ForEach(UnloadBank);
	}
}
