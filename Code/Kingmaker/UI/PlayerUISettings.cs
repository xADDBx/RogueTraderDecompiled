using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UI;

public class PlayerUISettings : IHashable
{
	[JsonProperty]
	[GameStateIgnore]
	private Vector2 m_LogSize = new Vector2(365f, 200f);

	[JsonProperty]
	[GameStateIgnore]
	private Vector2 m_LogSizeConsole = new Vector2(415f, 200f);

	[JsonProperty]
	private bool m_GlobalMapPartyHide;

	[JsonProperty]
	private bool m_GlobalMapShowLocationName;

	[JsonProperty]
	private bool m_IronManMode = true;

	[JsonProperty]
	private bool m_OnlyActiveCompanionExpMode = true;

	[JsonProperty]
	private bool m_TermsOfUseSeen;

	[JsonProperty]
	[GameStateIgnore]
	private bool m_JournalShowCompletedQuest = true;

	[JsonProperty]
	private bool m_GlobalMapLocalMapPin;

	[JsonProperty]
	private bool m_ShowReviewHint;

	[JsonProperty]
	[GameStateIgnore]
	private bool m_ShowInspect;

	[JsonProperty]
	public bool ShowFailedPerceptionChecks;

	[JsonProperty]
	public Dictionary<string, string> SettingsList = new Dictionary<string, string>();

	[JsonProperty]
	public Dictionary<VendorHelper.SaleOptions, bool> OptionsDictionary = new Dictionary<VendorHelper.SaleOptions, bool>();

	[JsonProperty]
	[GameStateIgnore]
	public ItemsFilterType InventoryFilter;

	[JsonProperty]
	[GameStateIgnore]
	public ItemsSorterType InventorySorter;

	[JsonProperty]
	[GameStateIgnore]
	public bool ShowUnavailableItems = true;

	[JsonProperty]
	[GameStateIgnore]
	public bool ShowUnavailableFeatures = true;

	[JsonProperty]
	public readonly ItemsCollection ItemsForBuy = new ItemsCollection(null)
	{
		IsVendorTable = true
	};

	[JsonProperty]
	public List<EntityRef<CargoEntity>> CargoesToSell = new List<EntityRef<CargoEntity>>();

	[JsonProperty]
	[GameStateIgnore]
	public Dictionary<EntityRef<MechanicEntity>, List<BlueprintFeature.Reference>> UnitToFavoritesMap = new Dictionary<EntityRef<MechanicEntity>, List<BlueprintFeature.Reference>>();

	[JsonProperty]
	[GameStateIgnore]
	public bool LootExtendedView = true;

	[JsonProperty]
	[GameStateIgnore]
	public SavedUnitProgressionWindowData SavedUnitProgressionWindowData;

	[JsonProperty]
	[GameStateIgnore]
	public Quest CurrentQuest;

	[JsonProperty]
	[GameStateIgnore]
	public EncyclopediaData EncyclopediaData = new EncyclopediaData();

	private IPage m_CurrentEncyclopediaPage;

	public UISettingsManager.SettingsScreen LastSettingsMenuType;

	[JsonProperty]
	public bool DelayNotificatioinSeen;

	[JsonProperty]
	[GameStateIgnore]
	public bool HoldCombatLog;

	[JsonProperty]
	[GameStateIgnore]
	public int CombatLogSizeIndex;

	[JsonProperty]
	public bool AutoEnTurnOptionSeen;

	public bool IsTBMSpeedUp;

	public IPage CurrentEncyclopediaPage
	{
		get
		{
			if (m_CurrentEncyclopediaPage != null && !(m_CurrentEncyclopediaPage is BlueprintEncyclopediaChapter) && !(m_CurrentEncyclopediaPage is BlueprintEncyclopediaGlossaryEntry))
			{
				return m_CurrentEncyclopediaPage;
			}
			BlueprintEncyclopediaChapter blueprintEncyclopediaChapter = UIConfig.Instance?.EncyclopediaDefaultPage?.Get();
			bool num = UIConfig.Instance?.EncyclopediaDefaultPage?.Get() != null;
			BlueprintEncyclopediaGlossaryChapter isGlossaryChapter = blueprintEncyclopediaChapter as BlueprintEncyclopediaGlossaryChapter;
			BlueprintEncyclopediaPage blueprintEncyclopediaPage = ((!num) ? blueprintEncyclopediaChapter : ((isGlossaryChapter != null) ? ((BlueprintEncyclopediaPage)(blueprintEncyclopediaChapter.ChildPages?.FirstOrDefault((BlueprintEncyclopediaPageReference cp) => cp?.Get() == isGlossaryChapter.GetChilds()?.FirstOrDefault()?.GetChilds()?.FirstOrDefault()))) : blueprintEncyclopediaChapter?.ChildPages?.FirstOrDefault()?.Get()));
			return m_CurrentEncyclopediaPage = blueprintEncyclopediaPage ?? blueprintEncyclopediaChapter;
		}
		set
		{
			m_CurrentEncyclopediaPage = value;
		}
	}

	public bool GlobalMapPartyHide
	{
		get
		{
			return m_GlobalMapPartyHide;
		}
		set
		{
			m_GlobalMapPartyHide = value;
		}
	}

	public Vector2 LogSize
	{
		get
		{
			return m_LogSize;
		}
		set
		{
			m_LogSize = value;
		}
	}

	public Vector2 LogSizeConsole
	{
		get
		{
			return m_LogSizeConsole;
		}
		set
		{
			m_LogSizeConsole = value;
		}
	}

	public bool GlobalMapShowLocationName
	{
		get
		{
			return m_GlobalMapShowLocationName;
		}
		set
		{
			m_GlobalMapShowLocationName = value;
		}
	}

	public bool JournalShowCompletedQuest
	{
		get
		{
			return m_JournalShowCompletedQuest;
		}
		set
		{
			m_JournalShowCompletedQuest = value;
		}
	}

	public bool GlobalMapLocalMapPin
	{
		get
		{
			return m_GlobalMapLocalMapPin;
		}
		set
		{
			m_GlobalMapLocalMapPin = value;
		}
	}

	public bool ShowInspect
	{
		get
		{
			return m_ShowInspect;
		}
		set
		{
			if (m_ShowInspect != value)
			{
				m_ShowInspect = value;
				EventBus.RaiseEvent(delegate(IShowInspectChanged h)
				{
					h.HandleShowInspect(m_ShowInspect);
				});
			}
		}
	}

	public bool ShowInspectJustToggle
	{
		set
		{
			m_ShowInspect = value;
		}
	}

	public bool ShowReviewHint
	{
		get
		{
			return m_ShowReviewHint;
		}
		set
		{
			m_ShowReviewHint = value;
		}
	}

	public bool IronManMode
	{
		get
		{
			return m_IronManMode;
		}
		set
		{
			m_IronManMode = value;
		}
	}

	public bool OnlyActiveCompanionsReceiveExperience
	{
		get
		{
			return m_OnlyActiveCompanionExpMode;
		}
		set
		{
			m_OnlyActiveCompanionExpMode = value;
		}
	}

	public bool TermsOfUseSeen
	{
		get
		{
			return m_TermsOfUseSeen;
		}
		set
		{
			m_TermsOfUseSeen = value;
		}
	}

	private bool EnableCombatSpeedUp
	{
		get
		{
			if ((SpeedUpMode)SettingsRoot.Game.TurnBased.SpeedUpMode != SpeedUpMode.On)
			{
				if ((SpeedUpMode)SettingsRoot.Game.TurnBased.SpeedUpMode == SpeedUpMode.OnDemand)
				{
					return IsTBMSpeedUp;
				}
				return false;
			}
			return true;
		}
	}

	public bool FastMovement
	{
		get
		{
			if (EnableCombatSpeedUp)
			{
				return SettingsRoot.Game.TurnBased.FastMovement;
			}
			return false;
		}
	}

	public bool FastPartyCast
	{
		get
		{
			if (EnableCombatSpeedUp)
			{
				return SettingsRoot.Game.TurnBased.FastPartyCast;
			}
			return false;
		}
	}

	public float AnimSpeedUpPlayer
	{
		get
		{
			if (!EnableCombatSpeedUp)
			{
				return 1f;
			}
			return SettingsRoot.Game.TurnBased.TimeScaleInPlayerTurn;
		}
	}

	public float AnimSpeedUpNPC
	{
		get
		{
			if (!EnableCombatSpeedUp)
			{
				return 1f;
			}
			return SettingsRoot.Game.TurnBased.TimeScaleInNonPlayerTurn;
		}
	}

	public float TimeScaleAverage => (AnimSpeedUpPlayer + AnimSpeedUpNPC) / 2f;

	public void DoSpeedUp()
	{
		IsTBMSpeedUp = true;
	}

	public void StopSpeedUp()
	{
		IsTBMSpeedUp = false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_GlobalMapPartyHide);
		result.Append(ref m_GlobalMapShowLocationName);
		result.Append(ref m_IronManMode);
		result.Append(ref m_OnlyActiveCompanionExpMode);
		result.Append(ref m_TermsOfUseSeen);
		result.Append(ref m_GlobalMapLocalMapPin);
		result.Append(ref m_ShowReviewHint);
		result.Append(ref ShowFailedPerceptionChecks);
		Dictionary<string, string> settingsList = SettingsList;
		if (settingsList != null)
		{
			int val = 0;
			foreach (KeyValuePair<string, string> item in settingsList)
			{
				Hash128 hash = default(Hash128);
				Hash128 val2 = StringHasher.GetHash128(item.Key);
				hash.Append(ref val2);
				Hash128 val3 = StringHasher.GetHash128(item.Value);
				hash.Append(ref val3);
				val ^= hash.GetHashCode();
			}
			result.Append(ref val);
		}
		Dictionary<VendorHelper.SaleOptions, bool> optionsDictionary = OptionsDictionary;
		if (optionsDictionary != null)
		{
			int val4 = 0;
			foreach (KeyValuePair<VendorHelper.SaleOptions, bool> item2 in optionsDictionary)
			{
				Hash128 hash2 = default(Hash128);
				VendorHelper.SaleOptions obj = item2.Key;
				Hash128 val5 = UnmanagedHasher<VendorHelper.SaleOptions>.GetHash128(ref obj);
				hash2.Append(ref val5);
				bool obj2 = item2.Value;
				Hash128 val6 = UnmanagedHasher<bool>.GetHash128(ref obj2);
				hash2.Append(ref val6);
				val4 ^= hash2.GetHashCode();
			}
			result.Append(ref val4);
		}
		Hash128 val7 = ClassHasher<ItemsCollection>.GetHash128(ItemsForBuy);
		result.Append(ref val7);
		List<EntityRef<CargoEntity>> cargoesToSell = CargoesToSell;
		if (cargoesToSell != null)
		{
			for (int i = 0; i < cargoesToSell.Count; i++)
			{
				EntityRef<CargoEntity> obj3 = cargoesToSell[i];
				Hash128 val8 = StructHasher<EntityRef<CargoEntity>>.GetHash128(ref obj3);
				result.Append(ref val8);
			}
		}
		result.Append(ref DelayNotificatioinSeen);
		result.Append(ref AutoEnTurnOptionSeen);
		return result;
	}
}
