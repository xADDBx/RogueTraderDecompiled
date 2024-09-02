using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.DLC;
using Kingmaker.Localization;
using Kingmaker.Networking;
using Kingmaker.Stores;
using Kingmaker.Utility.StatefulRandom;
using UnityEngine;

namespace Kingmaker.UI.Legacy.LoadingScreen;

public class LoadingScreenHints : StringsContainer
{
	public enum LocationEnum
	{
		LocationHints,
		BridgeHints,
		StarSystemHints,
		GlobalMapHints,
		SpaceCombatHints,
		MainMenuHints
	}

	[Serializable]
	public class DLCLoadingScreenInfoList
	{
		public List<DLCLoadingScreenInfo> DlcList;
	}

	public bool BetaTesting;

	private int m_Index;

	private bool m_DifficultyHintShown;

	[Tooltip("Hints about lore + common hits and game mechanics")]
	public CategoryPlatformHints Hints;

	[Tooltip("Hints about locations and ground fight (location & random encounter)")]
	public CategoryPlatformHints HintsLocation;

	[Tooltip("Hint about exploration and characters")]
	public CategoryPlatformHints HintsBridge;

	[Tooltip("Hints about exploration and colonization")]
	public CategoryPlatformHints HintsStarSystem;

	[Tooltip("Hints about warp, demons and colonization")]
	public CategoryPlatformHints HintsGlobalMap;

	[Tooltip("Hints with tips about space fight tactics")]
	public CategoryPlatformHints HintsSpaceCombat;

	[Tooltip("Hints with tips about cooperative mode")]
	public CategoryPlatformHints HintsCoop;

	private LocationEnum m_HintsEnum;

	private string m_FinalHint;

	public LocalizedString ReviewHint;

	public LocalizedString VsHint;

	public int DifficultyHintStart = 3;

	public int DifficultyHintInterval = 50;

	public LocalizedString DifficultyChangeHint;

	private List<List<LocalizedString>> m_AllHints = new List<List<LocalizedString>>();

	private List<DLCLoadingScreenInfo> m_PlayerPurchasedDLCs = new List<DLCLoadingScreenInfo>();

	public LocalizedString NewPurchasedDLCHint;

	public int MaxShowDLCHintCount = 5;

	public string TakeHint(LocationEnum whatIsHints, StatefulRandom random)
	{
		if (CheckNewDLCs())
		{
			return NewPurchasedDLCHint;
		}
		CategoryPlatformHints[] obj = new CategoryPlatformHints[5] { HintsLocation, HintsBridge, HintsStarSystem, HintsGlobalMap, Hints };
		List<List<LocalizedString>> list = new List<List<LocalizedString>>();
		CategoryPlatformHints[] array = obj;
		foreach (CategoryPlatformHints categoryPlatformHints in array)
		{
			List<LocalizedString> list2 = new List<LocalizedString>();
			list2.AddRange(categoryPlatformHints.FilledGlobalHints);
			list2.AddRange(Game.Instance.IsControllerMouse ? categoryPlatformHints.FilledPCHints : categoryPlatformHints.FilledConsoleHints);
			list.Add(list2);
		}
		m_AllHints.Clear();
		m_AllHints = list;
		int battleDeathCount = Game.Instance.StaticInfoCollector.BattleDeathCount;
		if (battleDeathCount >= DifficultyHintStart && (battleDeathCount - DifficultyHintStart) % DifficultyHintInterval == 0)
		{
			if (!m_DifficultyHintShown)
			{
				m_DifficultyHintShown = true;
				return DifficultyChangeHint;
			}
		}
		else
		{
			m_DifficultyHintShown = false;
		}
		if (BetaTesting)
		{
			return VsHint;
		}
		RandomHint(whatIsHints, random);
		return m_FinalHint;
	}

	public string NextHint(bool direction)
	{
		List<LocalizedString> list = new List<LocalizedString>();
		list.AddRange(Hints.FilledGlobalHints);
		list.AddRange(Game.Instance.IsControllerMouse ? Hints.FilledPCHints : Hints.FilledConsoleHints);
		if (direction)
		{
			if (++m_Index >= list.Count)
			{
				m_Index = 0;
			}
		}
		else if (--m_Index < 0)
		{
			m_Index = list.Count - 1;
		}
		return list[m_Index];
	}

	private void RandomHint(LocationEnum whatIsHintsRandom, StatefulRandom random)
	{
		m_HintsEnum = whatIsHintsRandom;
		int num = random.Range(1, 100);
		if (m_HintsEnum == LocationEnum.MainMenuHints)
		{
			List<LocalizedString> hint = m_AllHints[random.Range(0, m_AllHints.Count)];
			ReturnHints(hint, random);
			return;
		}
		List<LocalizedString> list = new List<LocalizedString>();
		list.AddRange(Hints.FilledGlobalHints);
		list.AddRange(Game.Instance.IsControllerMouse ? Hints.FilledPCHints : Hints.FilledConsoleHints);
		ReturnHints((num >= 1 && num <= 50) ? m_AllHints[(int)m_HintsEnum] : list, random);
	}

	private void ReturnHints(List<LocalizedString> hint, StatefulRandom random)
	{
		if (PhotonManager.Lobby.IsActive)
		{
			hint.AddRange(HintsCoop.FilledGlobalHints);
			hint.AddRange(Game.Instance.IsControllerMouse ? HintsCoop.FilledPCHints : HintsCoop.FilledConsoleHints);
		}
		m_Index = random.Range(0, hint.Count);
		m_FinalHint = hint[m_Index];
	}

	private bool CheckNewDLCs()
	{
		LoadPurchasedDLCs();
		List<string> currentPurchasedDLCs = GetCurrentPurchasedDLCs();
		bool result = false;
		foreach (string dlcName in currentPurchasedDLCs)
		{
			DLCLoadingScreenInfo dLCLoadingScreenInfo = m_PlayerPurchasedDLCs.Find((DLCLoadingScreenInfo d) => d.DlcName == dlcName);
			if (dLCLoadingScreenInfo != null && dLCLoadingScreenInfo.DisplayCount < MaxShowDLCHintCount)
			{
				result = true;
				dLCLoadingScreenInfo.DisplayCount++;
			}
			else if (dLCLoadingScreenInfo == null)
			{
				result = true;
				m_PlayerPurchasedDLCs.Add(new DLCLoadingScreenInfo
				{
					DlcName = dlcName,
					DisplayCount = 1
				});
			}
		}
		SavePurchasedDLCs();
		return result;
	}

	private List<string> GetCurrentPurchasedDLCs()
	{
		return (from dlc in StoreManager.GetPurchasableDLCs().OfType<BlueprintDlc>()
			where dlc.DlcType == DlcTypeEnum.AdditionalContentDlc && dlc.IsAvailable
			select dlc.name).ToList();
	}

	private void LoadPurchasedDLCs()
	{
		DLCLoadingScreenInfoList dLCLoadingScreenInfoList = JsonUtility.FromJson<DLCLoadingScreenInfoList>(PlayerPrefs.GetString("LoadingScreenPurchasedDLCs", "{}"));
		m_PlayerPurchasedDLCs = ((dLCLoadingScreenInfoList != null && dLCLoadingScreenInfoList.DlcList != null) ? dLCLoadingScreenInfoList.DlcList : new List<DLCLoadingScreenInfo>());
	}

	private void SavePurchasedDLCs()
	{
		string value = JsonUtility.ToJson(new DLCLoadingScreenInfoList
		{
			DlcList = m_PlayerPurchasedDLCs
		});
		PlayerPrefs.SetString("LoadingScreenPurchasedDLCs", value);
		PlayerPrefs.Save();
	}
}
