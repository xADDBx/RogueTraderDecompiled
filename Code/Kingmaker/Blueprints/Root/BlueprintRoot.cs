using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Blueprints;
using Code.GameCore.Blueprints.Workarounds;
using Code.GameCore.Mics;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Console.PS5.PSNObjects;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Interaction;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root;

[TypeId("bdfe642a3da11b04a80bc782b3ddea00")]
public class BlueprintRoot : BlueprintScriptableObject
{
	private static BlueprintRoot s_EditorInstance;

	[SerializeField]
	private BlueprintWarhammerRootReference m_WarhammerRoot;

	[SerializeField]
	[FormerlySerializedAs("DefaultPlayerCharacter")]
	private BlueprintUnitReference m_DefaultPlayerCharacter;

	[SerializeField]
	private BlueprintUnitReference m_DefaultPlayerShip;

	[SerializeField]
	[FormerlySerializedAs("SelectablePlayerCharacters")]
	private BlueprintUnitReference[] m_SelectablePlayerCharacters;

	[SerializeField]
	[FormerlySerializedAs("PlayerFaction")]
	private BlueprintFactionReference m_PlayerFaction;

	[SerializeField]
	private bool m_UseLightweightUnit;

	public bool CompanionsAI;

	[SerializeField]
	[FormerlySerializedAs("KingFlag")]
	private BlueprintUnlockableFlagReference m_KingFlag;

	public float MinProjectileMissDeviation = 3f;

	public float MaxProjectileMissDeviation = 8f;

	public AnimationSet HumanAnimationSet;

	[SerializeField]
	[FormerlySerializedAs("NewGamePreset")]
	private BlueprintAreaPresetReference m_NewGamePreset;

	public ActionList StartGameActions;

	public DialogRoot Dialog;

	public CheatRoot Cheats;

	[SerializeField]
	private BlueprintAreaReference m_SectorMapArea;

	public BlueprintStarSystemReference[] StarSystems;

	[SerializeField]
	private BlueprintAreaReference[] m_VoidshipBridges;

	public ProgressionRoot Progression;

	public Prefabs Prefabs;

	public OccludedCharacterColors OccludedCharacterColors;

	[SerializeField]
	private UIConfigReference m_UIConfig;

	public QuestsRoot Quests;

	public VendorsRoot Vendors;

	public SystemMechanicsRoot SystemMechanics;

	public StatusBuffsRoot StatusBuffs;

	public CursorRoot Cursors;

	public WeatherRoot WeatherSettings;

	public DlcRoot DlcSettings;

	public NewGameRoot NewGameSettings;

	[SerializeField]
	private CameraRoot.Reference m_CameraRoot;

	[SerializeField]
	private BlueprintCameraFollowSettings.Reference m_CameraFollowSettings;

	[SerializeField]
	private BlueprintCameraFollowSettings.Reference m_SpaceCameraFollowSettings;

	[SerializeField]
	private BlueprintActionCameraSettings.Reference m_ActionCameraSettings;

	public SurfaceTypeData SurfaceTypeData;

	[SerializeField]
	[FormerlySerializedAs("InvisibleKittenUnit")]
	private BlueprintUnitReference m_InvisibleKittenUnit;

	public LocalizedTexts LocalizedTexts;

	public UISettingsRoot UISettingsRoot;

	[SerializeField]
	[FormerlySerializedAs("DifficultyList")]
	private DifficultyPresetsList m_DifficultyList;

	public SettingsValues SettingsValues;

	public GameObject StealthEffectPrefab;

	public GameObject ExitStealthEffectPrefab;

	public WeaponModelSizeSettings WeaponModelSizing;

	public SoundRoot Sound;

	[SerializeField]
	public SoundRagdollSettings.Reference SoundRagdollSettings;

	public CalendarRoot Calendar;

	[SerializeField]
	public WarhammerDate InitialDate;

	[SerializeField]
	[FormerlySerializedAs("Formations")]
	private FormationsRootReference m_Formations;

	public AnimationRoot Animation;

	[SerializeField]
	[FormerlySerializedAs("FxRoot")]
	private FxRootReference m_FxRoot;

	[SerializeField]
	private CharGenRootReference m_CharGenRoot;

	[SerializeField]
	[FormerlySerializedAs("HitSystemRoot")]
	private HitSystemRootReference m_HitSystemRoot;

	[SerializeField]
	private PlayerUpgradeActionsRoot.Reference m_PlayerUpgradeActions;

	[SerializeField]
	[FormerlySerializedAs("CustomCompanion")]
	private BlueprintUnitReference m_CustomCompanion;

	[SerializeField]
	private BlueprintFeatureReference m_NavigatorOccupation;

	public int CustomCompanionBaseCost = 1;

	public int StandartPerceptionRadius = 5;

	public int AreaEffectAutoDestroySeconds = 30;

	public int MinSprintDistance = 10;

	public int MaxWalkDistance = 2;

	public int MinSprintDistanceInCombatCells = 10;

	public int MaxWalkDistanceInCombatCells = 2;

	public Texture2D DefaultDissolveTexture;

	[SerializeField]
	private BlueprintAchievementsRoot.Reference m_Achievements;

	[SerializeField]
	[FormerlySerializedAs("UnitTypes")]
	private BlueprintUnitTypeReference[] m_UnitTypes = new BlueprintUnitTypeReference[0];

	public TestUIStylesRoot TestUIStyles;

	[ValidateNotNull]
	[SerializeField]
	private ConsoleRootReference m_ConsoleRoot;

	[SerializeField]
	private BlueprintTrapSettingsRootReference m_BlueprintTrapSettingsRoot;

	[SerializeField]
	private BlueprintInteractionRoot.Referense m_InteractionRoot;

	[ValidateNotNull]
	[SerializeField]
	private FamiliarsRoot.Reference m_FamiliarsRoot;

	[SerializeField]
	private BlueprintMythicsSettingsReference m_BlueprintMythicsSettingsReference;

	[Header("SpaceCombat")]
	public GameObject BackgroundComposer;

	public GameObject SkyDome;

	public GameObject SkyDomeStarSystemFader;

	public int DeadEndTurnCost = 4;

	[Header("PlayStation")]
	[SerializeField]
	private BlueprintPSNObjectsRootReference m_PSNObjects;

	[Header("AssassinLethality")]
	[ValidateNotNull]
	[SerializeField]
	private BlueprintEntityPropertyReference m_AssassinLethalityPropertyRef;

	[ValidateNotNull]
	[SerializeField]
	private BlueprintFeatureReference m_AssassinCareerPathRef;

	public static BlueprintRoot Instance
	{
		get
		{
			if (Game.HasInstance || !Application.isEditor || Application.isPlaying)
			{
				return Game.Instance.BlueprintRoot;
			}
			if (s_EditorInstance == null)
			{
				s_EditorInstance = BlueprintRootInstanceHelper.GetInstance<BlueprintRoot>();
			}
			return s_EditorInstance ?? Game.Instance.BlueprintRoot;
		}
	}

	public BlueprintWarhammerRoot WarhammerRoot => m_WarhammerRoot?.Get();

	public BlueprintUnit DefaultPlayerCharacter => m_DefaultPlayerCharacter?.Get();

	public BlueprintUnit DefaultPlayerShip => m_DefaultPlayerShip?.Get();

	public ReferenceArrayProxy<BlueprintUnit> SelectablePlayerCharacters
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] selectablePlayerCharacters = m_SelectablePlayerCharacters;
			return selectablePlayerCharacters;
		}
	}

	public BlueprintFaction PlayerFaction => m_PlayerFaction?.Get();

	public bool UseLightweightUnit => m_UseLightweightUnit;

	public BlueprintUnlockableFlag KingFlag => m_KingFlag?.Get();

	public BlueprintAreaPreset NewGamePreset
	{
		get
		{
			return m_NewGamePreset?.Get();
		}
		set
		{
			m_NewGamePreset = value.ToReference<BlueprintAreaPresetReference>();
		}
	}

	public BlueprintSectorMapArea SectorMapArea => m_SectorMapArea.Get() as BlueprintSectorMapArea;

	public IEnumerable<BlueprintArea> VoidshipBridges => from x in m_VoidshipBridges.EmptyIfNull()
		where x?.Get() != null
		select x.Get();

	public UIConfig UIConfig => m_UIConfig?.Get();

	public CameraRoot CameraRoot => m_CameraRoot;

	public BlueprintCameraFollowSettings CameraFollowSettings => m_CameraFollowSettings;

	public BlueprintCameraFollowSettings SpaceCameraFollowSettings => m_SpaceCameraFollowSettings;

	public BlueprintActionCameraSettings ActionCameraSettings => m_ActionCameraSettings;

	public BlueprintUnit InvisibleKittenUnit => m_InvisibleKittenUnit?.Get();

	public DifficultyPresetsList DifficultyList => m_DifficultyList;

	public FormationsRoot Formations => m_Formations?.Get();

	public FxRoot FxRoot => m_FxRoot?.Get();

	public BlueprintCharGenRoot CharGenRoot => m_CharGenRoot?.Get();

	public HitSystemRoot HitSystemRoot => m_HitSystemRoot?.Get();

	public PlayerUpgradeActionsRoot PlayerUpgradeActions => m_PlayerUpgradeActions?.Get();

	public BlueprintUnit CustomCompanion => m_CustomCompanion?.Get();

	public BlueprintFeature NavigatorOccupation => m_NavigatorOccupation?.Get();

	public BlueprintAchievementsRoot Achievements => m_Achievements;

	public ReferenceArrayProxy<BlueprintUnitType> UnitTypes
	{
		get
		{
			BlueprintReference<BlueprintUnitType>[] unitTypes = m_UnitTypes;
			return unitTypes;
		}
	}

	public ConsoleRoot ConsoleRoot => m_ConsoleRoot.Get();

	public BlueprintTrapSettingsRoot BlueprintTrapSettingsRoot => m_BlueprintTrapSettingsRoot?.Get();

	public BlueprintInteractionRoot Interaction => m_InteractionRoot?.Get();

	public FamiliarsRoot FamiliarsRoot => m_FamiliarsRoot.Get();

	public BlueprintPSNObjectsRoot PSNObjects => m_PSNObjects.Get();

	public PropertyCalculator AssassinLethalityProperty => m_AssassinLethalityPropertyRef?.Get().Value;

	public string AssassinCareerPathGuid => m_AssassinCareerPathRef?.Guid;

	public override void OnEnable()
	{
		base.OnEnable();
		if (DlcSettings != null)
		{
			InterfaceServiceLocator.UnregisterService(typeof(IDlcRootService));
			InterfaceServiceLocator.RegisterService(DlcSettings, typeof(IDlcRootService));
		}
	}

	[BlueprintButton]
	public void CollectAchievements()
	{
	}

	[BlueprintButton]
	public void CollectUnitsTypes()
	{
	}
}
