using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Achievements;
using Kingmaker.AI.Learning;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cargo;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Console.PS5.PSNObjects;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.DialogSystem.State;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Formations;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.CombatRandomEncounters;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Inspect;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.Settings.Difficulty;
using Kingmaker.SpaceCombat.Scrap;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Tutorial;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.ModsInfo;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Sound;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker;

[JsonObject]
public sealed class Player : Entity, IDisposable, IHashable
{
	private static class VisitedAreasDataHasher
	{
		public static Hash128 GetHash128(Dictionary<BlueprintArea, List<string>> obj)
		{
			Hash128 result = default(Hash128);
			if (obj == null)
			{
				return result;
			}
			foreach (KeyValuePair<BlueprintArea, List<string>> item in obj)
			{
				Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				result.Append(ref val);
				for (int i = 0; i < item.Value.Count; i++)
				{
					Hash128 val2 = StringHasher.GetHash128(item.Value[i]);
					result.Append(ref val2);
				}
			}
			return result;
		}
	}

	public class CampaignImportSettings : IHashable
	{
		[JsonProperty]
		public bool LetPlayerChooseSave;

		[JsonProperty]
		public bool AutoImportIfOnlyOneSave;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref LetPlayerChooseSave);
			result.Append(ref AutoImportIfOnlyOneSave);
			return result;
		}
	}

	public enum CharactersList
	{
		ActiveUnits = 0,
		Everyone = 1,
		AllDetachedUnits = 3,
		DetachedPartyCharacters = 4,
		PartyCharacters = 5
	}

	public class SavedSceneEntry
	{
		[JsonProperty]
		public BlueprintArea Area;

		[JsonProperty]
		public string SceneName;
	}

	public enum GameOverReasonType
	{
		PartyIsDefeated = 0,
		EssentialUnitIsDead = 2,
		KingdomIsDestroyed = 3,
		Won = 4,
		QuestFailed = 5
	}

	public class WeatherData : IHashable
	{
		[JsonProperty]
		public InclemencyType CurrentWeather;

		[JsonProperty]
		public TimeSpan NextWeatherChange;

		[JsonProperty]
		public VisualStateEffectType? VisualStateEffectType;

		[JsonProperty]
		public InclemencyType? CurrentWeatherEffect;

		public InclemencyType ActualWeather
		{
			get
			{
				if (WeatherController.Instance == null)
				{
					return InclemencyType.Clear;
				}
				return WeatherController.Instance.ActualInclemency;
			}
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref CurrentWeather);
			result.Append(ref NextWeatherChange);
			if (VisualStateEffectType.HasValue)
			{
				VisualStateEffectType val = VisualStateEffectType.Value;
				result.Append(ref val);
			}
			if (CurrentWeatherEffect.HasValue)
			{
				InclemencyType val2 = CurrentWeatherEffect.Value;
				result.Append(ref val2);
			}
			return result;
		}
	}

	private class BlockingCommandData
	{
		private const int TicksDelay = 10;

		private readonly List<(AbstractUnitCommand, int)> m_Queue = new List<(AbstractUnitCommand, int)>();

		public bool LockOn(AbstractUnitCommand command)
		{
			RemoveOldFromQueue();
			int item = Game.Instance.RealTimeController.CurrentNetworkTick + 10;
			int num = FindIndex(command);
			if (num == -1)
			{
				num = m_Queue.Count;
				m_Queue.Add((command, item));
			}
			else
			{
				m_Queue[num] = (command, item);
			}
			return num == 0;
		}

		private int FindIndex(AbstractUnitCommand command)
		{
			for (int i = 0; i < m_Queue.Count; i++)
			{
				if (m_Queue[i].Item1 == command)
				{
					return i;
				}
			}
			return -1;
		}

		private void RemoveOldFromQueue()
		{
			int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
			for (int i = 0; i < m_Queue.Count; i++)
			{
				if (currentNetworkTick >= m_Queue[i].Item2)
				{
					m_Queue.RemoveAt(i);
					i--;
				}
			}
		}
	}

	private AiDataCollector AiDataCollector = new AiDataCollector();

	[JsonProperty]
	private UnitReference m_PlayerShip;

	[JsonProperty]
	public DateTime? StartDate;

	[JsonProperty]
	public BlueprintAreaPreset StartPreset;

	public SceneEntitiesState CrossSceneState = new SceneEntitiesState("[cross-area objects]");

	[JsonProperty]
	private QuestBook m_QuestBook;

	[JsonProperty]
	private readonly UnlockableFlagsManager m_UnlockableFlags = new UnlockableFlagsManager();

	[JsonProperty]
	private readonly DialogState m_Dialog = new DialogState();

	[JsonProperty]
	public readonly CompanionStoriesManager CompanionStories = new CompanionStoriesManager();

	[JsonProperty]
	public EtudesSystem EtudesSystem;

	[NotNull]
	[JsonProperty]
	public readonly HashSet<BlueprintArea> VisitedAreas = new HashSet<BlueprintArea>();

	[GameStateIgnore]
	[JsonProperty("CurrentArea")]
	public BlueprintArea SavedInArea;

	[GameStateIgnore]
	[JsonProperty("CurrentAreaPart")]
	public BlueprintAreaPart SavedInAreaPart;

	[GameStateIgnore]
	[JsonProperty]
	private Vector3 m_CameraPos;

	[GameStateIgnore]
	[JsonProperty]
	private float m_CameraRot;

	[GameStateIgnore]
	[JsonProperty]
	public Dictionary<StatType, Dictionary<string, int>> SkillChecks = new Dictionary<StatType, Dictionary<string, int>>();

	[JsonProperty]
	public readonly PlayerUISettings UISettings = new PlayerUISettings();

	[JsonProperty]
	public Scrap Scrap = new Scrap();

	[JsonProperty]
	public ProfitFactor ProfitFactor = new ProfitFactor();

	[JsonProperty]
	public readonly MinDifficultyController MinDifficultyController = new MinDifficultyController();

	[JsonProperty]
	public int Chapter;

	[JsonProperty]
	public ItemsCollection SharedStash;

	[JsonProperty]
	public readonly SharedVendorTables SharedVendorTables = new SharedVendorTables();

	[JsonProperty]
	public Dictionary<BlueprintItemsStashReference, ItemsCollection> VirtualStashes = new Dictionary<BlueprintItemsStashReference, ItemsCollection>();

	[JsonProperty]
	[GameStateIgnore]
	public readonly AchievementsManager Achievements = new AchievementsManager();

	[JsonProperty]
	public readonly PSNObjectsManager PSNObjects = new PSNObjectsManager();

	[JsonProperty]
	[GameStateIgnore]
	public readonly InspectUnitsManager InspectUnitsManager = new InspectUnitsManager();

	[JsonProperty]
	public readonly List<PlayerUpgradeAction> UpgradeActions = new List<PlayerUpgradeAction>();

	[JsonProperty]
	public readonly List<BlueprintPlayerUpgrader> AppliedPlayerUpgraders = new List<BlueprintPlayerUpgrader>();

	[JsonProperty]
	public readonly List<BlueprintPlayerUpgrader> IgnoredAppliedPlayerUpgraders = new List<BlueprintPlayerUpgrader>();

	[JsonProperty]
	public readonly List<BlueprintPlayerUpgrader> IgnoredNotAppliedPlayerUpgraders = new List<BlueprintPlayerUpgrader>();

	[JsonProperty]
	public readonly CombatRandomEncounterState CombatRandomEncounterState = new CombatRandomEncounterState();

	[JsonProperty]
	public Dictionary<EntityRef<MechanicEntity>, int> RespecUsedByChar = new Dictionary<EntityRef<MechanicEntity>, int>();

	[JsonProperty]
	public readonly WeatherData Weather = new WeatherData();

	[JsonProperty]
	public readonly WeatherData Wind = new WeatherData();

	[JsonProperty]
	public HashSet<BlueprintBarkBanter> PlayedBanters = new HashSet<BlueprintBarkBanter>();

	[JsonProperty]
	public BlueprintArea PreviousVisitedArea;

	[JsonProperty]
	public bool IsForceOpenVoidshipUpgrade;

	[JsonProperty]
	[GameStateIgnore]
	public bool IsShowBlockedColonyProjects = true;

	[JsonProperty]
	[GameStateIgnore]
	public bool IsShowFinishedColonyProjects = true;

	[JsonProperty]
	public Vector3? LastPositionOnPreviousVisitedArea;

	[JsonProperty]
	public Dictionary<FactionType, int> FractionsReputation = new Dictionary<FactionType, int>();

	[JsonProperty]
	public readonly UnitDataStorage AiCollectedDataStorage = new UnitDataStorage();

	[JsonProperty]
	public BlueprintStarSystemMap CurrentStarSystem;

	[JsonProperty]
	public CargoState CargoState = new CargoState();

	[JsonProperty]
	public List<EntityReference> ActivatedSpawners = new List<EntityReference>();

	[JsonProperty]
	public bool IsShowConsoleTooltip = true;

	[JsonProperty]
	[GameStateIgnore]
	public bool IsCameraRotateMode = true;

	[JsonProperty]
	public readonly VendorsData VendorsData = new VendorsData();

	[JsonProperty]
	public TraumasModification TraumasModification = new TraumasModification();

	[JsonProperty]
	public bool CanAccessStarshipInventory = true;

	[JsonProperty]
	public CountableFlag CannotAccessContracts = new CountableFlag();

	[JsonProperty]
	public readonly HashSet<BlueprintItem> ItemsToCargo = new HashSet<BlueprintItem>();

	[JsonProperty]
	private readonly Dictionary<BlueprintDlc, bool> m_StartNewGameAdditionalContentDlcStatus = new Dictionary<BlueprintDlc, bool>();

	[JsonProperty]
	public readonly List<BlueprintDlcReward> UsedDlcRewards = new List<BlueprintDlcReward>();

	[JsonProperty]
	public readonly List<BlueprintDlcReward> ClaimedDlcRewards = new List<BlueprintDlcReward>();

	[JsonProperty]
	public readonly List<string> ClaimedTwitchDrops = new List<string>();

	[JsonProperty]
	public readonly HashSet<BlueprintCampaign> ImportedCampaigns = new HashSet<BlueprintCampaign>();

	[JsonProperty]
	public readonly Dictionary<BlueprintCampaign, CampaignImportSettings> CampaignsToOfferImport = new Dictionary<BlueprintCampaign, CampaignImportSettings>();

	[JsonProperty]
	private readonly HashSet<string> m_ClaimedAchievementRewards = new HashSet<string>();

	private bool m_CharacterListsValid;

	private readonly List<BaseUnitEntity> m_Party = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_PartyAndPets = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_PartyAndPetsDetached = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_ActiveCompanions = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_RemoteCompanions = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_AllCharacters = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_AllStarships = new List<BaseUnitEntity>();

	private readonly List<BaseUnitEntity> m_AllCharactersAndStarships = new List<BaseUnitEntity>();

	private ItemsCollection m_Inventory;

	private readonly BlockingCommandData m_BlockingCommandData = new BlockingCommandData();

	[JsonProperty]
	public List<string> BrokenEntities = new List<string>();

	[JsonProperty]
	public StarshipEntity PlayerShip => (StarshipEntity)(((BaseUnitEntity)m_PlayerShip.Entity) ?? CrossSceneState?.AllEntityData.FirstItem((Entity v) => v is StarshipEntity));

	[JsonProperty]
	public TimeSpan GameTime { get; set; } = TimeSpan.Zero;


	[JsonProperty]
	public TimeSpan RealTime { get; set; } = TimeSpan.Zero;


	[GameStateIgnore]
	[JsonProperty]
	public TutorialSystem Tutorial { get; private set; }

	[JsonProperty]
	public WarpTravelState WarpTravelState { get; private set; } = new WarpTravelState();


	[JsonProperty]
	public ColoniesState ColoniesState { get; private set; } = new ColoniesState();


	[JsonProperty]
	public GlobalMapRandomGenerationState GlobalMapRandomGenerationState { get; private set; } = new GlobalMapRandomGenerationState();


	[JsonProperty]
	public StarSystemsState StarSystemsState { get; private set; } = new StarSystemsState();


	[JsonProperty]
	public int ExperienceRatePercent { get; set; } = 100;


	[JsonProperty]
	public List<UnitReference> PartyCharacters { get; private set; } = new List<UnitReference>();


	[JsonProperty]
	public long Money { get; private set; }

	[JsonProperty]
	[CanBeNull]
	public BlueprintUnit Stalker { get; set; }

	public Encumbrance Encumbrance => Encumbrance.Light;

	[JsonProperty]
	public UnitReference MainCharacter { get; private set; }

	public BaseUnitEntity MainCharacterEntity => MainCharacter.Entity.ToBaseUnitEntity();

	[JsonProperty]
	public UnitReference MainCharacterOriginal { get; private set; }

	public BaseUnitEntity MainCharacterOriginalEntity => MainCharacterOriginal.Entity?.ToBaseUnitEntity() ?? MainCharacter.Entity.ToBaseUnitEntity();

	[JsonProperty]
	public int MythicExperience { get; private set; }

	[Obsolete]
	[JsonProperty]
	private Dictionary<string, object> SettingsList
	{
		set
		{
			Game.Instance.State.InGameSettings.List = value;
		}
	}

	[GameStateIgnore]
	public IEnumerable<BlueprintDlcReward> DlcRewardsToSave => from reward in UsedDlcRewards.Concat(ClaimedDlcRewards)
		where reward.IsRequiredInSaves
		select reward;

	public GameOverReasonType? GameOverReason { get; private set; }

	public InGameSettings Settings => Game.Instance.State.InGameSettings;

	[NotNull]
	public ItemsCollection Inventory => m_Inventory ?? (m_Inventory = MainCharacterOriginalEntity?.Inventory.Collection ?? new ItemsCollection(this));

	public PartyFormationManager FormationManager => GetOrCreate<PartyFormationManager>();

	public PartyStrategistManager StrategistManager => GetOrCreate<PartyStrategistManager>();

	public AreaCROverrideManager AreaCROverrideManager => GetOrCreate<AreaCROverrideManager>();

	public UnitGroup Group => ((BaseUnitEntity)PartyCharacters[0].Entity).CombatGroup.Group;

	public bool CapitalPartyMode => Game.Instance.LoadedAreaState?.Settings.CapitalPartyMode ?? false;

	public bool ModsUser => UserModsData.Instance.PlayingWithMods;

	public IEnumerable<BaseUnitEntity> RemoteCompanions
	{
		get
		{
			UpdateCharacterLists();
			return m_RemoteCompanions;
		}
	}

	public IEnumerable<BaseUnitEntity> AllCrossSceneUnits => CrossSceneState.AllEntityData.OfType<BaseUnitEntity>();

	public List<BaseUnitEntity> Party
	{
		get
		{
			UpdateCharacterLists();
			return m_Party;
		}
	}

	public List<BaseUnitEntity> PartyAndPets
	{
		get
		{
			UpdateCharacterLists();
			return m_PartyAndPets;
		}
	}

	public List<BaseUnitEntity> PartyAndPetsDetached
	{
		get
		{
			UpdateCharacterLists();
			return m_PartyAndPetsDetached;
		}
	}

	public List<BaseUnitEntity> ActiveCompanions
	{
		get
		{
			UpdateCharacterLists();
			return m_ActiveCompanions;
		}
	}

	public List<BaseUnitEntity> AllCharacters
	{
		get
		{
			UpdateCharacterLists();
			return m_AllCharacters;
		}
	}

	public List<BaseUnitEntity> AllStarships
	{
		get
		{
			UpdateCharacterLists();
			return m_AllStarships;
		}
	}

	public List<BaseUnitEntity> AllCharactersAndStarships
	{
		get
		{
			UpdateCharacterLists();
			return m_AllCharactersAndStarships;
		}
	}

	public int PartyLevel
	{
		get
		{
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < PartyCharacters.Count; i++)
			{
				IAbstractUnitEntity entity = PartyCharacters[i].Entity;
				if (entity != null && entity.ToAbstractUnitEntity().IsInGame)
				{
					num += (float)entity.ToBaseUnitEntity().Progression.CharacterLevel;
					num2++;
				}
			}
			return (int)Math.Round(num / (float)num2);
		}
	}

	public BlueprintCampaign Campaign => SimpleBlueprintExtendAsObject.Or(StartPreset, null)?.Campaign;

	public UnlockableFlagsManager UnlockableFlags => m_UnlockableFlags;

	public QuestBook QuestBook => m_QuestBook;

	public DialogState Dialog => m_Dialog;

	public bool IsInCombat { get; private set; }

	public bool LastCombatLeaveIgnoreLeaveTimer { get; private set; }

	public bool PlayerIsKing
	{
		get
		{
			BlueprintUnlockableFlag kingFlag = BlueprintRoot.Instance.KingFlag;
			if (kingFlag != null)
			{
				return UnlockableFlags.IsUnlocked(kingFlag);
			}
			return false;
		}
	}

	public string GameId { get; set; }

	[JsonProperty]
	public BlueprintAreaEnterPoint NextEnterPoint { get; set; }

	public bool AchievementRewardIsClaimed(AchievementData achievementData)
	{
		if (achievementData != null && m_ClaimedAchievementRewards != null)
		{
			return m_ClaimedAchievementRewards.Contains(achievementData.AssetGuid);
		}
		return false;
	}

	public void ClaimAchievementReward(AchievementData achievementData)
	{
		if (achievementData != null && m_ClaimedAchievementRewards != null && !m_ClaimedAchievementRewards.Contains(achievementData.AssetGuid))
		{
			m_ClaimedAchievementRewards.Add(achievementData.AssetGuid);
		}
	}

	public void UpdateIsInCombat()
	{
		UpdateCharacterLists();
		LastCombatLeaveIgnoreLeaveTimer = false;
		for (int i = 0; i < PartyAndPets.Count; i++)
		{
			LastCombatLeaveIgnoreLeaveTimer = LastCombatLeaveIgnoreLeaveTimer || PartyAndPets[i].CombatState.LastCombatLeaveIgnoreLeaveTimer;
			if ((bool)PartyAndPets[i].CombatGroup.IsInCombat)
			{
				IsInCombat = true;
				return;
			}
		}
		IsInCombat = PlayerShip.IsInCombat;
	}

	[JsonConstructor]
	protected Player(JsonConstructorMark _)
		: base(_)
	{
		UnlockableFlagsManagerWrapper.Instance.Setup(m_UnlockableFlags);
	}

	public Player()
		: base("player", isInGame: true)
	{
		UnlockableFlagsManagerWrapper.Instance.Setup(m_UnlockableFlags);
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		m_QuestBook = Entity.Initialize(new QuestBook());
		SharedStash = new ItemsCollection(this);
		EtudesSystem = Entity.Initialize(new EtudesSystem());
		Tutorial = Entity.Initialize(new TutorialSystem());
	}

	public void InitializeHack()
	{
		GameTime = BlueprintRoot.Instance.InitialDate.GetTime();
		Achievements.Activate();
		PSNObjects.Activate();
		InitializeReputation();
		GlobalMapRandomGenerationState.Initialize();
		Scrap.Initialize();
		ProfitFactor.Initialize();
		AppliedPlayerUpgraders.AddRange(BlueprintRoot.Instance.PlayerUpgradeActions.Upgraders);
		IgnoredNotAppliedPlayerUpgraders.AddRange(BlueprintRoot.Instance.PlayerUpgradeActions.IgnoreUpgraders);
	}

	protected override void OnDispose()
	{
		CrossSceneState.Dispose();
		m_QuestBook.Dispose();
		Inventory.Dispose();
		SharedStash.Dispose();
		Achievements.Deactivate();
		Tutorial.Dispose();
		EtudesSystem.Dispose();
		CargoState.Dispose();
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.Dispose();
		});
		VirtualStashes.Clear();
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (Tutorial == null)
		{
			Tutorial = new TutorialSystem();
		}
		CrossSceneState.PostLoad();
		m_QuestBook.PostLoad();
		m_Inventory = MainCharacterOriginalEntity.Inventory.Collection;
		GetOrCreate<TurnDataPart>();
		GetOrCreate<PartyFormationManager>();
		GetOrCreate<PartyStrategistManager>();
		Inventory.PostLoad();
		SharedStash.PostLoad();
		MinDifficultyController.PostLoad();
		Tutorial.PostLoad();
		EtudesSystem.PostLoad();
		CargoState.PostLoad();
		SharedVendorTables?.PostLoad();
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.PrePostLoad();
		});
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.PostLoad();
		});
		ColoniesState.PostLoad();
		CrossSceneState.AllEntityData.OfType<DroppedLoot.EntityData>().ForEach(Game.Instance.EntityDestroyer.Destroy);
		CameraRig instance = CameraRig.Instance;
		if ((bool)instance)
		{
			instance.SavedPosition = m_CameraPos;
			instance.SavedRotation = m_CameraRot;
		}
		Achievements.Activate();
		PSNObjects.Activate();
		if (StartPreset == null)
		{
			StartPreset = BlueprintRoot.Instance.NewGamePreset;
		}
		GlobalMapRandomGenerationState.Initialize();
	}

	protected override void OnSubscribe()
	{
		base.OnSubscribe();
		QuestBook?.Subscribe();
		Inventory.Subscribe();
		SharedStash?.Subscribe();
		SharedVendorTables?.Subscribe();
		Tutorial?.Subscribe();
		EtudesSystem?.Subscribe();
		AiDataCollector.Subscribe();
		CargoState?.Subscribe();
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.Subscribe();
		});
	}

	protected override void OnUnsubscribe()
	{
		base.OnUnsubscribe();
		Tutorial.Unsubscribe();
		QuestBook.Unsubscribe();
		Inventory.Unsubscribe();
		SharedStash.Unsubscribe();
		SharedVendorTables.Unsubscribe();
		EtudesSystem.Unsubscribe();
		AiDataCollector.Unsubscribe();
		CargoState.Unsubscribe();
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.Unsubscribe();
		});
	}

	public void ApplyUpgrades()
	{
		foreach (PlayerUpgradeAction upgradeAction in UpgradeActions)
		{
			try
			{
				upgradeAction.Apply();
			}
			catch (Exception ex)
			{
				PFLog.Default.Error($"Exception while applying upgrade action: {upgradeAction.Type} ({upgradeAction.Blueprint})");
				PFLog.Default.Exception(ex);
			}
		}
		UpgradeActions.Clear();
		if ((bool)BlueprintRoot.Instance.PlayerUpgradeActions)
		{
			BlueprintRoot.Instance.PlayerUpgradeActions.ApplyUpgrades();
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		CrossSceneState.PreSave();
		m_QuestBook.PreSave();
		Inventory.PreSave();
		SharedStash.PreSave();
		MinDifficultyController.PreSave();
		SharedVendorTables.PreSave();
		Tutorial.PreSave();
		EtudesSystem.PreSave();
		CargoState.PreSave();
		VirtualStashes.ForEach(delegate(KeyValuePair<BlueprintItemsStashReference, ItemsCollection> vs)
		{
			vs.Value.PreSave();
		});
		CameraRig instance = CameraRig.Instance;
		m_CameraPos = (instance ? instance.transform.position : Vector3.zero);
		m_CameraRot = (instance ? instance.transform.eulerAngles.y : 0f);
	}

	public void SetMainCharacter(BaseUnitEntity unit)
	{
		if (MainCharacter != null)
		{
			RemovePartyCharacter(MainCharacter);
			if (MainCharacterEntity != null)
			{
				MainCharacterEntity.Remove<UnitPartMainCharacter>();
				EventBus.RaiseEvent(MainCharacter.Entity.ToIBaseUnitEntity(), delegate(IPartyHandler h)
				{
					h.HandleCompanionRemoved(stayInGame: false);
				});
			}
		}
		if (unit != null)
		{
			AddPartyCharacter(unit.FromAbstractUnitEntity());
			unit.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.InParty);
			EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IPartyHandler>)delegate(IPartyHandler h)
			{
				h.HandleAddCompanion();
			}, isCheckRuntime: true);
		}
		MainCharacter = unit.FromAbstractUnitEntity();
		m_CharacterListsValid = false;
		MainCharacter.Entity.ToAbstractUnitEntity().GetOrCreate<UnitPartMainCharacter>();
		MainCharacterOriginal = MainCharacter;
	}

	public void SetCompanionToMainCharacter(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
		if (MainCharacter.Entity == null)
		{
			throw new InvalidOperationException();
		}
		if (MainCharacterOriginal == null)
		{
			MainCharacterOriginal = MainCharacter;
		}
		MainCharacterEntity.Parts.RemoveAll((UnitPartMainCharacter i) => i.Temporary);
		MainCharacter = unit.FromAbstractUnitEntity();
		unit.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.InParty);
		unit.GetOrCreate<UnitPartMainCharacter>().Temporary = MainCharacterOriginalEntity != unit;
		m_CharacterListsValid = false;
	}

	public void SetMainStarship(BaseUnitEntity ship)
	{
		if (PlayerShip != null)
		{
			PlayerShip.Remove<UnitPartCompanion>();
		}
		ship?.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.InParty);
		m_PlayerShip = ship.FromBaseUnitEntity();
	}

	public void InitMainStarship(BlueprintAreaPreset preset)
	{
		if (preset.Area.AreaStatGameMode != GameModeType.SpaceCombat)
		{
			PlayerShip.IsInGame = false;
		}
		BlueprintDlcReward blueprintDlcReward = PlayerShip?.Blueprint?.GetComponent<DlcCondition>()?.DlcReward;
		if (blueprintDlcReward != null && !ClaimedDlcRewards.Contains(blueprintDlcReward))
		{
			ClaimedDlcRewards.Add(blueprintDlcReward);
		}
	}

	public void InitializeReputation()
	{
		if (FractionsReputation.Count == ReputationHelper.Factions.Count)
		{
			return;
		}
		foreach (FactionType faction in ReputationHelper.Factions)
		{
			if (!FractionsReputation.ContainsKey(faction))
			{
				FractionsReputation.Add(faction, 0);
			}
		}
	}

	public void MoveCharacters([NotNull] AreaEnterPoint areaEnterPoint, bool moveFollowers, bool moveCamera)
	{
		areaEnterPoint.PositionCharacters();
		if (moveFollowers)
		{
			foreach (BaseUnitEntity item in Party)
			{
				UnitPartFollowedByUnits optional = item.GetOptional<UnitPartFollowedByUnits>();
				if (optional == null)
				{
					continue;
				}
				foreach (var (abstractUnitEntity2, followerAction2) in Game.Instance.FollowersFormationController.CalculateTeleportToLeaderDestinations(optional))
				{
					if ((bool)abstractUnitEntity2.View)
					{
						abstractUnitEntity2.View.StopMoving();
					}
					abstractUnitEntity2.Position = followerAction2.Position;
					abstractUnitEntity2.DesiredOrientation = followerAction2.Orientation;
				}
			}
		}
		if (moveCamera)
		{
			if (Game.Instance.CurrentlyLoadedArea.IsPartyArea)
			{
				CameraRig.Instance.ScrollToImmediately(MainCharacter.Entity.Position);
			}
			else if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.SpaceCombat)
			{
				CameraRig.Instance.ScrollToImmediately(PlayerShip.Position);
			}
		}
		EventBus.RaiseEvent(delegate(ITeleportHandler h)
		{
			h.HandlePartyTeleport(areaEnterPoint);
		});
	}

	public void GainPartyExperience(int gained, bool isExperienceForDeath = false)
	{
		if (gained < 0)
		{
			PFLog.LevelUp.ErrorWithReport($"Party received invalid amount of experience: {gained}");
			return;
		}
		foreach (BaseUnitEntity item in (Game.Instance.CurrentMode != GameModeType.SpaceCombat) ? AllCharacters.Where((BaseUnitEntity u) => u.Master == null).Distinct() : AllStarships.Distinct())
		{
			item.Progression.GainExperience(gained, log: false);
		}
		EventBus.RaiseEvent(delegate(IPartyGainExperienceHandler h)
		{
			h.HandlePartyGainExperience(gained, isExperienceForDeath);
		});
	}

	public bool SpendMoney(long amount)
	{
		if (Money < amount)
		{
			return false;
		}
		Money -= amount;
		return true;
	}

	public void GainMoney(long amount)
	{
		Money += amount;
	}

	public void OnAreaLoaded()
	{
		foreach (BaseUnitEntity allCrossSceneUnit in AllCrossSceneUnits)
		{
			if ((allCrossSceneUnit is StarshipEntity && Game.Instance.CurrentlyLoadedArea.IsShipArea) || (allCrossSceneUnit is UnitEntity && !Game.Instance.CurrentlyLoadedArea.IsShipArea))
			{
				allCrossSceneUnit.Buffs.SpawnBuffsFxs();
			}
		}
	}

	public void AddCompanion(BaseUnitEntity value, bool remote = false)
	{
		value.GetOrCreate<UnitPartCompanion>().SetState(remote ? CompanionState.Remote : CompanionState.InParty);
		value.Faction.Set(BlueprintRoot.Instance.PlayerFaction);
		TryUpdateLevel(value);
		if (!remote)
		{
			AddPartyCharacter(value.FromBaseUnitEntity());
		}
		InvalidateCharacterLists();
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		if (currentlyLoadedArea != null && !currentlyLoadedArea.IsPartyArea)
		{
			value.IsInGame = false;
		}
		if (!remote)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)value, (Action<IPartyHandler>)delegate(IPartyHandler h)
			{
				h.HandleCompanionActivated();
			}, isCheckRuntime: true);
		}
	}

	private void TryUpdateLevel(BaseUnitEntity value)
	{
		int experience = MainCharacter.Entity.ToBaseUnitEntity().Progression.Experience;
		if (value.Progression.Experience != experience)
		{
			value.Progression.AdvanceExperienceTo(experience, log: false);
		}
	}

	public void RemoveCompanion(BaseUnitEntity value, bool stayInGame = false)
	{
		if (MainCharacter == value)
		{
			PFLog.Default.Error("Trying to remove Main Character from party");
		}
		else
		{
			RemoveCompanionInternal(value, stayInGame);
		}
	}

	private void RemoveCompanionInternal(BaseUnitEntity value, bool stayInGame = false)
	{
		RemovePartyCharacter(value.FromBaseUnitEntity());
		value.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.Remote);
		m_CharacterListsValid = false;
		if (!stayInGame)
		{
			value.IsInGame = false;
			if ((bool)UIAccess.SelectionManager)
			{
				UIAccess.SelectionManager.UpdateSelectedUnits();
			}
		}
		EventBus.RaiseEvent((IBaseUnitEntity)value, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleCompanionRemoved(stayInGame);
		}, isCheckRuntime: true);
	}

	private static void ReplaceCompanionInList(BaseUnitEntity remove, BaseUnitEntity add, List<UnitReference> list)
	{
		int num = list.IndexOf(remove.FromAbstractUnitEntity());
		if (num >= 0)
		{
			list[num] = add.FromAbstractUnitEntity();
		}
	}

	public void DismissCompanion([NotNull] BaseUnitEntity value)
	{
		if (!value.State.CanRemoveFromParty)
		{
			PFLog.Default.Error("Trying to remove story companion from party");
			return;
		}
		RemovePartyCharacter(value.FromAbstractUnitEntity());
		value.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.ExCompanion);
		m_CharacterListsValid = false;
		value.IsInGame = false;
		foreach (ItemEntity item in Inventory.Items)
		{
			if (item.Owner == value && !item.HoldingSlot.RemoveItem())
			{
				PFLog.Default.Error("Unable to unequip item {0} while dismissing a custom companion: item will disappear!", item.Blueprint);
			}
		}
		Game.Instance.EntityDestroyer.Destroy(value);
		EventBus.RaiseEvent((IBaseUnitEntity)value, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleCompanionRemoved(stayInGame: false);
		}, isCheckRuntime: true);
	}

	public void DetachPartyMember(BaseUnitEntity unit)
	{
		if (!PartyCharacters.Contains(unit.FromBaseUnitEntity()))
		{
			throw new Exception($"Unit {unit} is not in party or already detached");
		}
		if (PartyCharacters.Count < 2)
		{
			throw new Exception("Can't detach all party members");
		}
		RemovePartyCharacter(unit.FromBaseUnitEntity());
		unit.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.InPartyDetached);
		m_CharacterListsValid = false;
		UIAccess.SelectionManager.UpdateSelectedUnits();
		EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleCompanionRemoved(stayInGame: false);
		}, isCheckRuntime: true);
	}

	public void AttachPartyMember(BaseUnitEntity unit)
	{
		if (!unit.IsDetached)
		{
			throw new Exception($"Unit {unit} is not detached");
		}
		unit.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.InParty);
		AddPartyCharacter(unit.FromBaseUnitEntity());
		m_CharacterListsValid = false;
		EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IPartyHandler>)delegate(IPartyHandler h)
		{
			h.HandleAddCompanion();
		}, isCheckRuntime: true);
	}

	public void SwapAttachedAndDetachedPartyMembers()
	{
		List<BaseUnitEntity> list = AllCrossSceneUnits.Where((BaseUnitEntity u) => !u.IsPet && u.IsDetached).ToTempList();
		if (list.Count < 1)
		{
			throw new Exception("Has no detached party members");
		}
		List<UnitReference> list2 = PartyCharacters.ToList();
		list.ForEach(delegate(BaseUnitEntity u)
		{
			AttachPartyMember(u);
		});
		list2.ForEach(delegate(UnitReference u)
		{
			DetachPartyMember(u.ToBaseUnitEntity());
		});
		InvalidateCharacterLists();
	}

	public void InvalidateCharacterLists()
	{
		m_CharacterListsValid = false;
	}

	public void UpdateCharacterLists()
	{
		if (m_CharacterListsValid)
		{
			return;
		}
		m_Party.Clear();
		m_ActiveCompanions.Clear();
		m_PartyAndPets.Clear();
		m_AllCharacters.Clear();
		m_RemoteCompanions.Clear();
		m_AllStarships.Clear();
		m_AllCharactersAndStarships.Clear();
		foreach (UnitReference item in PartyCharacters.ToTempList())
		{
			AddCharacterToLists(item.Entity.ToBaseUnitEntity());
		}
		foreach (Entity allEntityDatum in CrossSceneState.AllEntityData)
		{
			if (allEntityDatum is BaseUnitEntity baseUnitEntity && !m_AllCharactersAndStarships.Contains(baseUnitEntity))
			{
				m_AllCharactersAndStarships.Add(baseUnitEntity);
				if (baseUnitEntity.IsStarship())
				{
					AddStarshipToLists(baseUnitEntity);
				}
				else
				{
					AddCharacterToLists(baseUnitEntity);
				}
			}
		}
		m_CharacterListsValid = true;
	}

	private void AddCharacterToLists(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			PFLog.Default.Error("Unit is null! (maybe you load old save)");
			return;
		}
		if (m_AllCharacters.Contains(unit))
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = unit.Master ?? unit;
		UnitReference unitReference = baseUnitEntity.FromBaseUnitEntity();
		CompanionState? obj = baseUnitEntity.GetOptional<UnitPartCompanion>()?.State;
		bool flag = obj == CompanionState.InParty;
		bool num = obj == CompanionState.InPartyDetached;
		bool isPet = unit.IsPet;
		if (flag && !PartyCharacters.Contains(unitReference))
		{
			LogChannel.Default.Warning($"Unit {unitReference} in party, but not in party list. Fixing.");
			AddPartyCharacter(unitReference);
		}
		if (!flag && RemovePartyCharacter(unitReference))
		{
			LogChannel.Default.Warning($"Unit {unitReference} not in party, but in party list. Fixing.");
		}
		if (CapitalPartyMode)
		{
			flag = baseUnitEntity.IsMainCharacter;
		}
		if (!isPet && flag)
		{
			m_Party.Add(unit);
		}
		if (!isPet && flag && unitReference != MainCharacter)
		{
			m_ActiveCompanions.Add(unit);
		}
		UnitPartCompanion optional = baseUnitEntity.GetOptional<UnitPartCompanion>();
		if (optional == null || optional.State != CompanionState.Remote)
		{
			UnitPartCompanion optional2 = baseUnitEntity.GetOptional<UnitPartCompanion>();
			if (optional2 == null || optional2.State != CompanionState.InParty || flag)
			{
				goto IL_0163;
			}
		}
		m_RemoteCompanions.Add(unit);
		goto IL_0163;
		IL_0163:
		if (flag && unit.IsInGame)
		{
			m_PartyAndPets.Add(unit);
		}
		if (num && unit.IsInGame)
		{
			m_PartyAndPetsDetached.Add(unit);
		}
		m_AllCharacters.Add(unit);
	}

	private void AddStarshipToLists(BaseUnitEntity starship)
	{
		if (starship == null)
		{
			PFLog.Default.Error("Starship is null! (maybe you load old save)");
		}
		else if (!m_AllStarships.Contains(starship))
		{
			m_AllStarships.Add(starship);
		}
	}

	public IEnumerable<BaseUnitEntity> GetCharactersList(CharactersList type)
	{
		return type switch
		{
			CharactersList.ActiveUnits => PartyAndPets, 
			CharactersList.Everyone => AllCharacters, 
			CharactersList.AllDetachedUnits => AllCharacters.Where((BaseUnitEntity u) => u.IsDetached), 
			CharactersList.DetachedPartyCharacters => AllCharacters.Where((BaseUnitEntity u) => !u.IsPet && u.IsDetached), 
			CharactersList.PartyCharacters => Party, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public IEnumerable<BaseUnitEntity> GetPartyCharactersForGroupCommand(Vector3 approachPoint, bool checkArea)
	{
		uint area = ObstacleAnalyzer.GetArea(approachPoint);
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			return new StarshipEntity[1] { Game.Instance.Player.PlayerShip };
		}
		return from u in PartyAndPets
			where u.IsDirectlyControllable
			where u.IsMyNetRole()
			where u.State.CanMove
			where !checkArea || ObstacleAnalyzer.GetArea(u.Position) == area
			where u.Parts.GetOptional<UnitPartSaddled>() == null
			where u.Blueprint is BlueprintStarship == Game.Instance.CurrentlyLoadedArea.IsShipArea
			select u;
	}

	public void FixPartyAfterChange()
	{
		if (!Game.Instance.CurrentlyLoadedArea.IsPartyArea)
		{
			return;
		}
		foreach (UnitReference partyCharacter in PartyCharacters)
		{
			BaseUnitEntity baseUnitEntity = partyCharacter.Entity.ToBaseUnitEntity();
			if (baseUnitEntity == null)
			{
				break;
			}
			if (!baseUnitEntity.IsInGame || baseUnitEntity.Suppressed || !AreaService.IsInMechanicBounds(baseUnitEntity.Position))
			{
				baseUnitEntity.IsInGame = true;
				Vector3 vector = Game.Instance.Player.MainCharacter.Entity.Position;
				if ((bool)AstarPath.active)
				{
					FreePlaceSelector.PlaceSpawnPlaces(2, baseUnitEntity.Corpulence, vector);
					vector = FreePlaceSelector.GetRelaxedPosition(1, projectOnGround: true);
				}
				baseUnitEntity.Position = vector;
			}
			if (!baseUnitEntity.Faction.IsDirectlyControllable)
			{
				baseUnitEntity.Faction.Set(BlueprintRoot.Instance.PlayerFaction);
			}
			if (baseUnitEntity.CombatGroup.Id != Game.Instance.Player.MainCharacter.Entity.ToBaseUnitEntity().CombatGroup.Id)
			{
				baseUnitEntity.CombatGroup.Id = Game.Instance.Player.MainCharacter.Entity.ToBaseUnitEntity().CombatGroup.Id;
			}
		}
	}

	public void CreateCustomCompanion(Action<BaseUnitEntity> successCallback = null, int? xp = null, CharGenConfig.CharGenCompanionType companionType = CharGenConfig.CharGenCompanionType.Common)
	{
		int characterLevel = MainCharacter.Entity.ToBaseUnitEntity().Progression.CharacterLevel;
		int targetExp = xp ?? BlueprintRoot.Instance.Progression.XPTable.GetBonus(characterLevel);
		BaseUnitEntity newCompanion = BlueprintRoot.Instance.CustomCompanion.CreateEntity();
		newCompanion.State.CanRemoveFromParty = true;
		newCompanion.Progression.AdvanceExperienceTo(targetExp, log: false);
		UpdateSoundState(MusicStateHandler.MusicState.Chargen);
		CharGenConfig.Create(newCompanion, CharGenConfig.CharGenMode.NewCompanion, companionType, isCustomCompanionChargen: true).SetOnComplete(OnComplete).SetOnClose(OnClose)
			.SetOnCloseSoundAction(delegate
			{
				UpdateSoundState(MusicStateHandler.MusicState.Setting);
			})
			.OpenUI();
		void OnClose()
		{
			newCompanion.Dispose();
		}
		void OnComplete(BaseUnitEntity newUnit)
		{
			successCallback?.Invoke(newUnit);
		}
		static void UpdateSoundState(MusicStateHandler.MusicState state)
		{
			SoundState.Instance.OnMusicStateChange(state);
		}
	}

	public int GetCustomCompanionCost()
	{
		return BlueprintRoot.Instance.CustomCompanionBaseCost;
	}

	public int GetMinimumRespecCost()
	{
		return (from ch in AllCrossSceneUnits.Where(delegate(BaseUnitEntity u)
			{
				UnitPartCompanion optional = u.GetOptional<UnitPartCompanion>();
				if (optional == null || optional.State != CompanionState.InParty)
				{
					UnitPartCompanion optional2 = u.GetOptional<UnitPartCompanion>();
					if (optional2 == null)
					{
						return false;
					}
					return optional2.State == CompanionState.Remote;
				}
				return true;
			})
			where ch.Progression != null && PartUnitProgression.CanRespec(ch)
			select ch).Min((BaseUnitEntity ch) => ch.Progression.GetRespecCost());
	}

	public void GameOver(GameOverReasonType reason)
	{
		if (reason == GameOverReasonType.Won)
		{
			Game.Instance.ResetToMainMenu();
		}
		else
		{
			GameOverReason = reason;
			Game.Instance.StartMode(GameModeType.GameOver);
		}
		EventBus.RaiseEvent(delegate(IGameOverHandler h)
		{
			h.HandleGameOver(reason);
		});
	}

	public void ReInitPartyCharacters(List<UnitReference> newCompanions)
	{
		foreach (UnitReference item in PartyCharacters.ToList())
		{
			if (!newCompanions.Contains(item))
			{
				RemoveCompanionInternal(item.Entity.ToBaseUnitEntity());
			}
		}
		foreach (UnitReference newCompanion in newCompanions)
		{
			if (!PartyCharacters.Contains(newCompanion))
			{
				AddCompanion(newCompanion.ToBaseUnitEntity());
			}
		}
		PartyCharacters.Clear();
		foreach (UnitReference newCompanion2 in newCompanions)
		{
			AddPartyCharacter(newCompanion2);
		}
	}

	private void AddPartyCharacter(UnitReference unitReference)
	{
		PartyCharacters.Add(unitReference);
	}

	private bool RemovePartyCharacter(UnitReference unitReference)
	{
		return PartyCharacters.Remove(unitReference);
	}

	public int GetMillennium()
	{
		return BlueprintRoot.Instance.InitialDate.Millenniums;
	}

	public int GetAMRCYears()
	{
		return (BlueprintRoot.Instance.InitialDate.GetAMRCDate() + GameTime).TotalWarhammerYears();
	}

	public int GetVVYears()
	{
		return (BlueprintRoot.Instance.InitialDate.GetVVDate() + GameTime).TotalWarhammerYears();
	}

	public int GetSegments()
	{
		return (BlueprintRoot.Instance.InitialDate.Segments.Segments() + GameTime).Segments();
	}

	public void ChangeChapter(int chapter)
	{
		int chapter2 = Chapter;
		Chapter = Math.Max(Chapter, chapter);
		if (chapter2 != Chapter)
		{
			EventBus.RaiseEvent(delegate(IChangeChapterHandler h)
			{
				h.HandleChangeChapter();
			});
		}
	}

	public bool CheckDlcAvailable()
	{
		using (ContextData<DlcExtension.LoadSaveDlcCheck>.Request())
		{
			return DlcRewardsToSave.All((BlueprintDlcReward dlcReward) => dlcReward.IsAvailable);
		}
	}

	public void UpdateAdditionalContentDlcStatus(BlueprintDlc dlc, bool status)
	{
		if (dlc != null && !m_StartNewGameAdditionalContentDlcStatus.TryAdd(dlc, status))
		{
			m_StartNewGameAdditionalContentDlcStatus[dlc] = status;
		}
	}

	public void SyncAdditionalContentDlcRewards()
	{
		foreach (KeyValuePair<BlueprintDlc, bool> item in m_StartNewGameAdditionalContentDlcStatus)
		{
			foreach (IBlueprintDlcReward reward in item.Key.Rewards)
			{
				BlueprintDlcRewardCampaignAdditionalContent bpAc = reward as BlueprintDlcRewardCampaignAdditionalContent;
				if (bpAc == null)
				{
					continue;
				}
				if (item.Value)
				{
					if (!UsedDlcRewards.Any((BlueprintDlcReward r) => r.AssetGuid == bpAc.AssetGuid))
					{
						UsedDlcRewards.Add(bpAc);
					}
				}
				else
				{
					UsedDlcRewards.RemoveAll((BlueprintDlcReward r) => r.AssetGuid == bpAc.AssetGuid);
				}
			}
		}
	}

	public bool GetAdditionalContentDlcStatus(BlueprintDlc dlc)
	{
		if (dlc != null)
		{
			return m_StartNewGameAdditionalContentDlcStatus.GetValueOrDefault(dlc, defaultValue: false);
		}
		return false;
	}

	public bool HasAdditionalContentDlc(BlueprintDlc dlc)
	{
		if (dlc != null)
		{
			return m_StartNewGameAdditionalContentDlcStatus.ContainsKey(dlc);
		}
		return false;
	}

	public void RemoveAdditionalContentDlc(BlueprintDlc dlc)
	{
		if (dlc != null)
		{
			m_StartNewGameAdditionalContentDlcStatus.Remove(dlc);
		}
	}

	public void RemoveAllAdditionalContentDlc()
	{
		m_StartNewGameAdditionalContentDlcStatus.Clear();
	}

	public IEnumerable<BlueprintDlc> GetStartNewGameAdditionalContentDlc()
	{
		foreach (KeyValuePair<BlueprintDlc, bool> item in m_StartNewGameAdditionalContentDlcStatus)
		{
			yield return item.Key;
		}
	}

	public IEnumerable<IBlueprintDlc> GetAvailableAdditionalContentDlcForCurrentCampaign()
	{
		foreach (IBlueprintDlc item in StoreManager.GetAllAvailableAdditionalContentDlc())
		{
			if (Campaign != null && item != null && item.Rewards.Any((IBlueprintDlcReward dlcReward) => dlcReward is BlueprintDlcRewardCampaignAdditionalContent blueprintDlcRewardCampaignAdditionalContent && blueprintDlcRewardCampaignAdditionalContent.Campaign == Campaign))
			{
				yield return item;
			}
		}
	}

	public void ApplySwitchOnDlc(List<BlueprintDlc> dlcList)
	{
		try
		{
			foreach (BlueprintDlc dlc in dlcList)
			{
				UpdateAdditionalContentDlcStatus(dlc, status: true);
			}
			SyncAdditionalContentDlcRewards();
			Game.Instance.MakeQuickSave(ApplySwitchOnDlcLoad);
		}
		catch (Exception ex)
		{
			PFLog.Default.Error("Exception while applying switch on dlc`s");
			PFLog.Default.Exception(ex);
		}
	}

	private void ApplySwitchOnDlcLoad()
	{
		Game.Instance.QuickLoadGame();
	}

	public bool IsBlockedOn(AbstractUnitCommand command)
	{
		return m_BlockingCommandData.LockOn(command);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		UnitReference obj = m_PlayerShip;
		Hash128 val2 = UnitReferenceHasher.GetHash128(ref obj);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<StarshipEntity>.GetHash128(PlayerShip);
		result.Append(ref val3);
		TimeSpan val4 = GameTime;
		result.Append(ref val4);
		TimeSpan val5 = RealTime;
		result.Append(ref val5);
		if (StartDate.HasValue)
		{
			DateTime val6 = StartDate.Value;
			result.Append(ref val6);
		}
		Hash128 val7 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(StartPreset);
		result.Append(ref val7);
		Hash128 val8 = ClassHasher<QuestBook>.GetHash128(m_QuestBook);
		result.Append(ref val8);
		Hash128 val9 = ClassHasher<UnlockableFlagsManager>.GetHash128(m_UnlockableFlags);
		result.Append(ref val9);
		Hash128 val10 = ClassHasher<DialogState>.GetHash128(m_Dialog);
		result.Append(ref val10);
		Hash128 val11 = ClassHasher<CompanionStoriesManager>.GetHash128(CompanionStories);
		result.Append(ref val11);
		Hash128 val12 = ClassHasher<EtudesSystem>.GetHash128(EtudesSystem);
		result.Append(ref val12);
		Hash128 val13 = ClassHasher<WarpTravelState>.GetHash128(WarpTravelState);
		result.Append(ref val13);
		Hash128 val14 = ClassHasher<ColoniesState>.GetHash128(ColoniesState);
		result.Append(ref val14);
		Hash128 val15 = ClassHasher<GlobalMapRandomGenerationState>.GetHash128(GlobalMapRandomGenerationState);
		result.Append(ref val15);
		Hash128 val16 = ClassHasher<StarSystemsState>.GetHash128(StarSystemsState);
		result.Append(ref val16);
		HashSet<BlueprintArea> visitedAreas = VisitedAreas;
		if (visitedAreas != null)
		{
			int num = 0;
			foreach (BlueprintArea item in visitedAreas)
			{
				num ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		int val17 = ExperienceRatePercent;
		result.Append(ref val17);
		List<UnitReference> partyCharacters = PartyCharacters;
		if (partyCharacters != null)
		{
			for (int i = 0; i < partyCharacters.Count; i++)
			{
				UnitReference obj2 = partyCharacters[i];
				Hash128 val18 = UnitReferenceHasher.GetHash128(ref obj2);
				result.Append(ref val18);
			}
		}
		Hash128 val19 = ClassHasher<PlayerUISettings>.GetHash128(UISettings);
		result.Append(ref val19);
		long val20 = Money;
		result.Append(ref val20);
		Hash128 val21 = ClassHasher<Scrap>.GetHash128(Scrap);
		result.Append(ref val21);
		Hash128 val22 = ClassHasher<ProfitFactor>.GetHash128(ProfitFactor);
		result.Append(ref val22);
		Hash128 val23 = ClassHasher<MinDifficultyController>.GetHash128(MinDifficultyController);
		result.Append(ref val23);
		Hash128 val24 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Stalker);
		result.Append(ref val24);
		result.Append(ref Chapter);
		Hash128 val25 = ClassHasher<ItemsCollection>.GetHash128(SharedStash);
		result.Append(ref val25);
		Hash128 val26 = ClassHasher<SharedVendorTables>.GetHash128(SharedVendorTables);
		result.Append(ref val26);
		Dictionary<BlueprintItemsStashReference, ItemsCollection> virtualStashes = VirtualStashes;
		if (virtualStashes != null)
		{
			int val27 = 0;
			foreach (KeyValuePair<BlueprintItemsStashReference, ItemsCollection> item2 in virtualStashes)
			{
				Hash128 hash = default(Hash128);
				Hash128 val28 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(item2.Key);
				hash.Append(ref val28);
				Hash128 val29 = ClassHasher<ItemsCollection>.GetHash128(item2.Value);
				hash.Append(ref val29);
				val27 ^= hash.GetHashCode();
			}
			result.Append(ref val27);
		}
		Hash128 val30 = ClassHasher<PSNObjectsManager>.GetHash128(PSNObjects);
		result.Append(ref val30);
		List<PlayerUpgradeAction> upgradeActions = UpgradeActions;
		if (upgradeActions != null)
		{
			for (int j = 0; j < upgradeActions.Count; j++)
			{
				Hash128 val31 = ClassHasher<PlayerUpgradeAction>.GetHash128(upgradeActions[j]);
				result.Append(ref val31);
			}
		}
		List<BlueprintPlayerUpgrader> appliedPlayerUpgraders = AppliedPlayerUpgraders;
		if (appliedPlayerUpgraders != null)
		{
			for (int k = 0; k < appliedPlayerUpgraders.Count; k++)
			{
				Hash128 val32 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(appliedPlayerUpgraders[k]);
				result.Append(ref val32);
			}
		}
		List<BlueprintPlayerUpgrader> ignoredAppliedPlayerUpgraders = IgnoredAppliedPlayerUpgraders;
		if (ignoredAppliedPlayerUpgraders != null)
		{
			for (int l = 0; l < ignoredAppliedPlayerUpgraders.Count; l++)
			{
				Hash128 val33 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(ignoredAppliedPlayerUpgraders[l]);
				result.Append(ref val33);
			}
		}
		List<BlueprintPlayerUpgrader> ignoredNotAppliedPlayerUpgraders = IgnoredNotAppliedPlayerUpgraders;
		if (ignoredNotAppliedPlayerUpgraders != null)
		{
			for (int m = 0; m < ignoredNotAppliedPlayerUpgraders.Count; m++)
			{
				Hash128 val34 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(ignoredNotAppliedPlayerUpgraders[m]);
				result.Append(ref val34);
			}
		}
		Hash128 val35 = ClassHasher<CombatRandomEncounterState>.GetHash128(CombatRandomEncounterState);
		result.Append(ref val35);
		UnitReference obj3 = MainCharacter;
		Hash128 val36 = UnitReferenceHasher.GetHash128(ref obj3);
		result.Append(ref val36);
		UnitReference obj4 = MainCharacterOriginal;
		Hash128 val37 = UnitReferenceHasher.GetHash128(ref obj4);
		result.Append(ref val37);
		Dictionary<EntityRef<MechanicEntity>, int> respecUsedByChar = RespecUsedByChar;
		if (respecUsedByChar != null)
		{
			int val38 = 0;
			foreach (KeyValuePair<EntityRef<MechanicEntity>, int> item3 in respecUsedByChar)
			{
				Hash128 hash2 = default(Hash128);
				EntityRef<MechanicEntity> obj5 = item3.Key;
				Hash128 val39 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj5);
				hash2.Append(ref val39);
				int obj6 = item3.Value;
				Hash128 val40 = UnmanagedHasher<int>.GetHash128(ref obj6);
				hash2.Append(ref val40);
				val38 ^= hash2.GetHashCode();
			}
			result.Append(ref val38);
		}
		Hash128 val41 = ClassHasher<WeatherData>.GetHash128(Weather);
		result.Append(ref val41);
		Hash128 val42 = ClassHasher<WeatherData>.GetHash128(Wind);
		result.Append(ref val42);
		int val43 = MythicExperience;
		result.Append(ref val43);
		HashSet<BlueprintBarkBanter> playedBanters = PlayedBanters;
		if (playedBanters != null)
		{
			int num2 = 0;
			foreach (BlueprintBarkBanter item4 in playedBanters)
			{
				num2 ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item4).GetHashCode();
			}
			result.Append(num2);
		}
		Hash128 val44 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(PreviousVisitedArea);
		result.Append(ref val44);
		result.Append(ref IsForceOpenVoidshipUpgrade);
		if (LastPositionOnPreviousVisitedArea.HasValue)
		{
			Vector3 val45 = LastPositionOnPreviousVisitedArea.Value;
			result.Append(ref val45);
		}
		Dictionary<FactionType, int> fractionsReputation = FractionsReputation;
		if (fractionsReputation != null)
		{
			int val46 = 0;
			foreach (KeyValuePair<FactionType, int> item5 in fractionsReputation)
			{
				Hash128 hash3 = default(Hash128);
				FactionType obj7 = item5.Key;
				Hash128 val47 = UnmanagedHasher<FactionType>.GetHash128(ref obj7);
				hash3.Append(ref val47);
				int obj8 = item5.Value;
				Hash128 val48 = UnmanagedHasher<int>.GetHash128(ref obj8);
				hash3.Append(ref val48);
				val46 ^= hash3.GetHashCode();
			}
			result.Append(ref val46);
		}
		Hash128 val49 = ClassHasher<UnitDataStorage>.GetHash128(AiCollectedDataStorage);
		result.Append(ref val49);
		Hash128 val50 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(CurrentStarSystem);
		result.Append(ref val50);
		Hash128 val51 = ClassHasher<CargoState>.GetHash128(CargoState);
		result.Append(ref val51);
		List<EntityReference> activatedSpawners = ActivatedSpawners;
		if (activatedSpawners != null)
		{
			for (int n = 0; n < activatedSpawners.Count; n++)
			{
				Hash128 val52 = ClassHasher<EntityReference>.GetHash128(activatedSpawners[n]);
				result.Append(ref val52);
			}
		}
		result.Append(ref IsShowConsoleTooltip);
		Hash128 val53 = ClassHasher<VendorsData>.GetHash128(VendorsData);
		result.Append(ref val53);
		Hash128 val54 = ClassHasher<TraumasModification>.GetHash128(TraumasModification);
		result.Append(ref val54);
		result.Append(ref CanAccessStarshipInventory);
		Hash128 val55 = ClassHasher<CountableFlag>.GetHash128(CannotAccessContracts);
		result.Append(ref val55);
		HashSet<BlueprintItem> itemsToCargo = ItemsToCargo;
		if (itemsToCargo != null)
		{
			int num3 = 0;
			foreach (BlueprintItem item6 in itemsToCargo)
			{
				num3 ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item6).GetHashCode();
			}
			result.Append(num3);
		}
		Dictionary<BlueprintDlc, bool> startNewGameAdditionalContentDlcStatus = m_StartNewGameAdditionalContentDlcStatus;
		if (startNewGameAdditionalContentDlcStatus != null)
		{
			int val56 = 0;
			foreach (KeyValuePair<BlueprintDlc, bool> item7 in startNewGameAdditionalContentDlcStatus)
			{
				Hash128 hash4 = default(Hash128);
				Hash128 val57 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item7.Key);
				hash4.Append(ref val57);
				bool obj9 = item7.Value;
				Hash128 val58 = UnmanagedHasher<bool>.GetHash128(ref obj9);
				hash4.Append(ref val58);
				val56 ^= hash4.GetHashCode();
			}
			result.Append(ref val56);
		}
		List<BlueprintDlcReward> usedDlcRewards = UsedDlcRewards;
		if (usedDlcRewards != null)
		{
			for (int num4 = 0; num4 < usedDlcRewards.Count; num4++)
			{
				Hash128 val59 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(usedDlcRewards[num4]);
				result.Append(ref val59);
			}
		}
		List<BlueprintDlcReward> claimedDlcRewards = ClaimedDlcRewards;
		if (claimedDlcRewards != null)
		{
			for (int num5 = 0; num5 < claimedDlcRewards.Count; num5++)
			{
				Hash128 val60 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(claimedDlcRewards[num5]);
				result.Append(ref val60);
			}
		}
		List<string> claimedTwitchDrops = ClaimedTwitchDrops;
		if (claimedTwitchDrops != null)
		{
			for (int num6 = 0; num6 < claimedTwitchDrops.Count; num6++)
			{
				Hash128 val61 = StringHasher.GetHash128(claimedTwitchDrops[num6]);
				result.Append(ref val61);
			}
		}
		HashSet<BlueprintCampaign> importedCampaigns = ImportedCampaigns;
		if (importedCampaigns != null)
		{
			int num7 = 0;
			foreach (BlueprintCampaign item8 in importedCampaigns)
			{
				num7 ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item8).GetHashCode();
			}
			result.Append(num7);
		}
		Dictionary<BlueprintCampaign, CampaignImportSettings> campaignsToOfferImport = CampaignsToOfferImport;
		if (campaignsToOfferImport != null)
		{
			int val62 = 0;
			foreach (KeyValuePair<BlueprintCampaign, CampaignImportSettings> item9 in campaignsToOfferImport)
			{
				Hash128 hash5 = default(Hash128);
				Hash128 val63 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item9.Key);
				hash5.Append(ref val63);
				Hash128 val64 = ClassHasher<CampaignImportSettings>.GetHash128(item9.Value);
				hash5.Append(ref val64);
				val62 ^= hash5.GetHashCode();
			}
			result.Append(ref val62);
		}
		HashSet<string> claimedAchievementRewards = m_ClaimedAchievementRewards;
		if (claimedAchievementRewards != null)
		{
			int num8 = 0;
			foreach (string item10 in claimedAchievementRewards)
			{
				num8 ^= StringHasher.GetHash128(item10).GetHashCode();
			}
			result.Append(num8);
		}
		Hash128 val65 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(NextEnterPoint);
		result.Append(ref val65);
		List<string> brokenEntities = BrokenEntities;
		if (brokenEntities != null)
		{
			for (int num9 = 0; num9 < brokenEntities.Count; num9++)
			{
				Hash128 val66 = StringHasher.GetHash128(brokenEntities[num9]);
				result.Append(ref val66);
			}
		}
		return result;
	}
}
