using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;
using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Interaction;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UI.Common.UIConfigComponents;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using TMPro;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Blueprints.Root;

[TypeId("352f2c3d37d66f64eaf3a156026c8882")]
public class UIConfig : BlueprintScriptableObject
{
	[Serializable]
	public class CoverHighlightConfig
	{
		[Serializable]
		public class CoverHighlightColorEntry
		{
			public DestructionStage Stage;

			public Color StageColor;
		}

		public Color DefaultColor = Color.white;

		public CoverHighlightColorEntry[] Colors;

		public Color GetHighlightColor(DestructionStage stage)
		{
			return Colors.FirstOrDefault((CoverHighlightColorEntry i) => i.Stage == stage)?.StageColor ?? DefaultColor;
		}
	}

	[Serializable]
	public class SpaceCombatConfig
	{
		[Header("Ship Doll Module Colors")]
		public Color ShipDollModuleNormal;

		public Color ShipDollModuleWarning;

		public Color ShipDollModuleCritical;

		[Header("Crew Panel Bar Colors")]
		public Color CrewPanelBarColorNormal;

		public Color CrewPanelBarColorCritical;

		public Color GetShipDollModuleColor(ShipCrewModuleState state)
		{
			return state switch
			{
				ShipCrewModuleState.FullyStaffed => ShipDollModuleNormal, 
				ShipCrewModuleState.UnderStaffed => ShipDollModuleWarning, 
				ShipCrewModuleState.Unmanned => ShipDollModuleCritical, 
				_ => Color.black, 
			};
		}
	}

	[Serializable]
	public class FeedbackPopupConfig
	{
		public string ConfigUrl;

		public FeedbackPopupItem[] FallbackItems;

		[Header("Icons")]
		public Sprite Survey;

		public Sprite Discord;

		public Sprite Twitter;

		public Sprite Facebook;

		public Sprite Website;

		public Sprite GetIconByPopupItemType(FeedbackPopupItemType type)
		{
			return type switch
			{
				FeedbackPopupItemType.Survey => Survey, 
				FeedbackPopupItemType.Discord => Discord, 
				FeedbackPopupItemType.Twitter => Twitter, 
				FeedbackPopupItemType.Facebook => Facebook, 
				FeedbackPopupItemType.Website => Website, 
				_ => null, 
			};
		}
	}

	[Serializable]
	public class EquipSlotTypeIcons
	{
		[Serializable]
		public class TypeIconsPair
		{
			public EquipSlotType Type;

			public EquipSlotSubtype Subtype;

			public Sprite Icon;
		}

		public List<TypeIconsPair> Icons;

		public Sprite GetTypeIcon(EquipSlotType type, EquipSlotSubtype subtype = EquipSlotSubtype.None)
		{
			return Icons.FirstOrDefault((TypeIconsPair i) => i.Type == type && i.Subtype == subtype)?.Icon;
		}
	}

	[Serializable]
	public class LootTypeIcons
	{
		public Sprite Default;

		public Sprite Chest;

		public Sprite GetIcon(LootContainerType type)
		{
			if (type == LootContainerType.Chest || type == LootContainerType.PlayerChest)
			{
				return Chest;
			}
			return Default;
		}
	}

	[Serializable]
	public class UnitPortraits
	{
		[Header("Placeholder Portraits")]
		[SerializeField]
		private BlueprintPortraitReference m_MalePlaceholderPortrait;

		[SerializeField]
		private BlueprintPortraitReference m_FemalePlaceholderPortrait;

		[SerializeField]
		private BlueprintPortraitReference m_LeaderPlaceholderPortrait;

		[Header("Unit Subtype Icons")]
		[SerializeField]
		private EnumUnitSubtypeIconsReference m_UnitSubtypePortrait;

		[SerializeField]
		private EnumUnitSubtypeIconsReference m_UnitSubtypeIcons;

		public BlueprintPortrait MalePlaceholderPortrait => m_MalePlaceholderPortrait?.Get();

		public BlueprintPortrait FemalePlaceholderPortrait => m_FemalePlaceholderPortrait?.Get();

		public BlueprintPortrait LeaderPlaceholderPortrait => m_LeaderPlaceholderPortrait?.Get();

		public EnumUnitSubtypeIcons UnitSubtypePortrait => m_UnitSubtypePortrait?.Get();

		public EnumUnitSubtypeIcons UnitSubtypeIcons => m_UnitSubtypeIcons?.Get();
	}

	[Serializable]
	public class AnomalyTypeIcons
	{
		public Sprite Default;

		public Sprite ShipSignature;

		public Sprite Enemy;

		public Sprite Gas;

		public Sprite WarpHton;

		public Sprite Loot;

		public Sprite GetAnomalyIcon(BlueprintAnomaly.AnomalyObjectType type)
		{
			return type switch
			{
				BlueprintAnomaly.AnomalyObjectType.ShipSignature => ShipSignature, 
				BlueprintAnomaly.AnomalyObjectType.Enemy => Enemy, 
				BlueprintAnomaly.AnomalyObjectType.Gas => Gas, 
				BlueprintAnomaly.AnomalyObjectType.WarpHton => WarpHton, 
				BlueprintAnomaly.AnomalyObjectType.Loot => Loot, 
				_ => Default, 
			};
		}
	}

	[Serializable]
	public class AnomalyTypeColor
	{
		public Color Default;

		public Color ShipSignature;

		public Color Enemy;

		public Color Gas;

		public Color WarpHton;

		public Color Loot;

		public Color GetAnomalyColor(BlueprintAnomaly.AnomalyObjectType type)
		{
			return type switch
			{
				BlueprintAnomaly.AnomalyObjectType.ShipSignature => ShipSignature, 
				BlueprintAnomaly.AnomalyObjectType.Enemy => Enemy, 
				BlueprintAnomaly.AnomalyObjectType.Gas => Gas, 
				BlueprintAnomaly.AnomalyObjectType.WarpHton => WarpHton, 
				BlueprintAnomaly.AnomalyObjectType.Loot => Loot, 
				_ => Default, 
			};
		}
	}

	[Serializable]
	public class QuestNotificationStateColor
	{
		public Color Failed;

		public Color Completed;

		public Color New;

		public Color Updated;

		public Color Postponed;

		public Color GetQuestStateColor(QuestNotificationState state)
		{
			return state switch
			{
				QuestNotificationState.Failed => Failed, 
				QuestNotificationState.Completed => Completed, 
				QuestNotificationState.Updated => Updated, 
				QuestNotificationState.Postponed => Postponed, 
				_ => New, 
			};
		}
	}

	[Serializable]
	public class IconAndTextCustomColorsForTooltip
	{
		[Header("Colors")]
		public Color LightGrey;

		public Color LightGreen;

		public Color LightRed;

		[Header("Icons")]
		public Sprite MagnifyingGlass;
	}

	[Serializable]
	public class CreditsGroups
	{
		public List<BlueprintCreditsGroupReference> Groups = new List<BlueprintCreditsGroupReference>();

		public List<BlueprintCreditsGroupReference> EndTitlesGroups = new List<BlueprintCreditsGroupReference>();

		public List<SpriteLink> BackgroundSprites = new List<SpriteLink>();
	}

	[Serializable]
	public class AugmentationsSlotsReferences
	{
		public BlueprintAugmentSlotReference ForgeworldSlot;

		public BlueprintAugmentSlotReference AugmentsSystems;

		public BlueprintAugmentSlotReference AugmentsEye;

		public BlueprintAugmentSlotReference AugmentsArmsLeft;

		public BlueprintAugmentSlotReference AugmentsArmsRight;

		public BlueprintAugmentSlotReference AugmentsTorso;

		public BlueprintAugmentSlotReference AugmentsLegs;

		public BlueprintAugmentSlotReference AugmentsManipulus1;

		public BlueprintAugmentSlotReference AugmentsManipulus2;

		public BlueprintAugmentSlotReference AugmentsManipulus3;

		public BlueprintAugmentSlotReference AugmentsPasqal1;

		public BlueprintAugmentSlotReference AugmentsPasqal2;

		public BlueprintAugmentSlotReference AugmentsPasqal3;
	}

	[Serializable]
	public class QuestDlcConfig
	{
		[SerializeField]
		private Sprite Dlc1New;

		[SerializeField]
		private Sprite Dlc1Default;

		[SerializeField]
		private Sprite Dlc2New;

		[SerializeField]
		private Sprite Dlc2Default;

		[SerializeField]
		private Sprite Dlc3New;

		[SerializeField]
		private Sprite Dlc3Default;

		[SerializeField]
		private Sprite Dlc4New;

		[SerializeField]
		private Sprite Dlc4Default;

		[SerializeField]
		private Sprite Dlc1ListIcon;

		[SerializeField]
		private Sprite Dlc2ListIcon;

		[SerializeField]
		private Sprite Dlc3ListIcon;

		[SerializeField]
		private Sprite Dlc4ListIcon;

		public Sprite GetListIcon(Dlc dlc)
		{
			return dlc switch
			{
				Dlc.Dlc1 => Dlc1ListIcon, 
				Dlc.Dlc2 => Dlc2ListIcon, 
				Dlc.Dlc3 => Dlc3ListIcon, 
				Dlc.Dlc4 => Dlc4ListIcon, 
				Dlc.None => null, 
				_ => null, 
			};
		}

		public Sprite GetNew(Dlc dlc)
		{
			return dlc switch
			{
				Dlc.Dlc1 => Dlc1New, 
				Dlc.Dlc2 => Dlc2New, 
				Dlc.Dlc3 => Dlc3New, 
				Dlc.Dlc4 => Dlc4New, 
				Dlc.None => null, 
				_ => null, 
			};
		}

		public Sprite GetDefault(Dlc dlc)
		{
			return dlc switch
			{
				Dlc.Dlc1 => Dlc1Default, 
				Dlc.Dlc2 => Dlc2Default, 
				Dlc.Dlc3 => Dlc3Default, 
				Dlc.Dlc4 => Dlc4Default, 
				Dlc.None => null, 
				_ => null, 
			};
		}
	}

	[Serializable]
	public class AugmentationsSpaceBarkBanters
	{
		public BlueprintBarkBanter FirstLaunchBarkBanter;

		public BlueprintBarkBanterList BanterList;

		public bool IsFirstLaunchInSpace()
		{
			bool result = Game.Instance.Player.PartyAugmentManager.ShouldShowAttentionMarker;
			EventBus.RaiseEvent(delegate(IAugmentationsButtonAttentionMarkerHandler h)
			{
				h.HandleAttentionMarker(result);
			});
			return result;
		}

		public void SetFirstAugmentationsLaunchInSpace()
		{
			EventBus.RaiseEvent(delegate(IAugmentationsButtonAttentionMarkerHandler h)
			{
				h.HandleAttentionMarker(state: false);
			});
			Game.Instance.Player.PartyAugmentManager.MarkAttentionMarkerSeen();
		}
	}

	[Serializable]
	public class AugmentationsSlotDefaultIcons
	{
		public Sprite Default;

		public Sprite IconEye;

		public Sprite IconCranial;

		public Sprite IconTorso;

		public Sprite IconArms;

		public Sprite IconLegs;

		public Sprite IconInternal;

		public Sprite GetIconBySlotType(ItemsFilterType augmentType)
		{
			return augmentType switch
			{
				ItemsFilterType.AugmentationsSystems => IconCranial, 
				ItemsFilterType.AugmentationsArms => IconArms, 
				ItemsFilterType.AugmentationsLegs => IconLegs, 
				ItemsFilterType.AugmentationsEyes => IconEye, 
				ItemsFilterType.AugmentationsTorso => IconTorso, 
				_ => Default, 
			};
		}
	}

	[Serializable]
	public class NecronTimerConfigurations
	{
		public Color TimerMilestoneOff;

		public Color TimerMilestoneOn;

		public Color Slider;

		public Color SliderHandle;

		[Range(8f, 15f)]
		public int MaxTimerValue = 12;

		public int MiddleMilestoneIndex;

		public BlueprintUnlockableFlagReference TimeLoopBlueprintReference;
	}

	[Serializable]
	public class AugmentationsDefaultAugmentsReferences
	{
		[SerializeField]
		private List<BlueprintItemAugmentReference> DefaultAugments;

		public bool IsDefaultAugment(BlueprintItem itemBlueprint)
		{
			BlueprintItemAugment itemAugment = itemBlueprint as BlueprintItemAugment;
			if (itemAugment != null)
			{
				return DefaultAugments.Any((BlueprintItemAugmentReference i) => i.Get() == itemAugment);
			}
			return false;
		}
	}

	[Serializable]
	public class AugmentationsSlotTraumaLockerReferences
	{
		[SerializeField]
		private List<SlotToTraumaMap> TraumaMap;

		public List<BlueprintBuff> GetTraumaList(BlueprintAugmentSlot slotBlueprint)
		{
			return TraumaMap.FirstOrDefault((SlotToTraumaMap t) => t.AugmentSlot.Get() == slotBlueprint)?.Traumas.Select((BlueprintBuffReference b) => b.Get()).ToList();
		}
	}

	[Serializable]
	public class SlotToTraumaMap
	{
		public BlueprintAugmentSlotReference AugmentSlot;

		public List<BlueprintBuffReference> Traumas;
	}

	[SerializeField]
	private UIViewConfigs.Reference m_ViewConfigs;

	[SerializeField]
	private BlueprintUISoundReference m_BlueprintUISound;

	[SerializeField]
	private BlueprintUINetLobbyTutorial.Reference m_BlueprintUINetLobbyTutorial;

	[SerializeField]
	private BlueprintUILocalMapLegend.Reference m_BlueprintUILocalMapLegend;

	public Color PaperInterfacesLetter = Color.red;

	public Color PaperSaberColor = Color.red;

	public Sprite KeyArt;

	public Sprite DlcEntityKeyArt;

	public VideoLink KeyVideoMainMenu;

	public BlueprintDebugInformationBubble DebugBubble;

	public LogColors LogColors;

	public DialogColors DialogColors;

	public GlossaryColors PaperGlossaryColors;

	public GlossaryColors DigitalGlossaryColors;

	public TooltipColors TooltipColors;

	public SpellBookColors SpellBookColors;

	public TutorialColors TutorialColors;

	public OvertipSystemObjectColorConfig OvertipSystemObjectColors;

	public CharScreenColors CharSheet;

	public UIIcons UIIcons;

	public CombatTextColors CombatTextColors;

	[Header("CharGenColors")]
	public Color StatDefaultColor;

	public Color StatPositiveColor;

	public Color StatNegativeColor;

	[Header("Highlight Colors")]
	public Color EnemyHighlightColor = Color.red;

	public Color AllyHighlightColor = Color.green;

	public Color NeutralHighlightColor = Color.yellow;

	public Color NaturalHighlightColor = Color.white;

	public Color StandartUnitLootColor = Color.cyan;

	public Color VisitedLootColor = Color.cyan;

	public Color StandartLootColor = Color.cyan;

	public Color StandartLootColorPercepted = Color.yellow;

	public Color PerceptedLootColor = Color.magenta;

	public Color HighlightedTrapedLoot = Color.red;

	public Color TrapedLoot = Color.red;

	public Color DefaultTrapHighlight = Color.red;

	public Color DefaultHighlight = Color.white;

	public Color InteractionHighlight = Color.yellow;

	public Sprite TransparentImage;

	public Sprite DefaultNetAvatar;

	[Header("Coop Colors")]
	public List<Color> CoopPlayersPingsColors = new List<Color>();

	public List<Material> CoopPlayersPingsMaterials = new List<Material>();

	public CoverHighlightConfig CoverHighlight;

	[Header("Items Description")]
	public LocalizedString ItemOriginOwnerDescription;

	public LocalizedString ItemVendorDescription;

	[Header("Text Formats")]
	[Tooltip("{0} - Cargo Volume, {1} - Label ({10%} {of Melee Weaponry Cargo})")]
	public string TooltipItemFooterFormat;

	[Tooltip("{0} - Current Value, {1} - Max Value ({10}| max{12})")]
	public string WeaponSetTextFormat;

	[Tooltip("{0} - answer id, will be set automatically")]
	public string UIDialogExchangeLinkFormat;

	[Tooltip("{0} - answer id, {1} - sprite name, will be set automatically")]
	public string UIDialogConditionsLinkFormat;

	[Tooltip("{0} - max range label, {1} range value")]
	public string UITooltipMaxRangeFormat;

	public PercentHelper PercentHelper;

	public int SubTextPercentSize = 70;

	[Space]
	[Header("RandomColors")]
	public Color32[] RandomColors = new Color32[10];

	public SpaceCombatConfig SpaceCombat;

	public FeedbackPopupConfig FeedbackConfig;

	public EquipSlotTypeIcons TypeIcons;

	public UnitPortraits Portraits;

	public AnomalyTypeIcons AnomalyIcons;

	public AnomalyTypeColor AnomalyColor;

	public QuestNotificationStateColor QuestStateColor;

	public ChapterList ChapterList;

	public BlueprintEncyclopediaChapterReference EncyclopediaDefaultPage;

	public BlueprintEncyclopediaChapterReference BookEventsChapter;

	public BlueprintEncyclopediaChapterReference PlanetTypeChapter;

	public BlueprintEncyclopediaChapterReference AstropathBriefsChapter;

	public BlueprintCareerPathReference HunterCareerPath;

	[SerializeField]
	private BlueprintAbilityReference m_ReloadAbility;

	[SerializeField]
	private InteractionVariantVisualSetsBlueprintReference m_InteractionVariantVisualSetsBlueprint;

	public int DefaultConsoleHintScaleInText = 150;

	public const float OvertipDistanceReveal = 6.35f;

	public TMP_FontAsset DefaultTMPFontAsset;

	public TMP_SpriteAsset DefaultTMPSriteAsset;

	public CameraFlyAnimationParams GlobalMapWarpTravelCameraSpeed;

	public IconAndTextCustomColorsForTooltip IconAndTextCustomColors;

	public CreditsGroups Credits;

	public AcronymsConfig AcronymsConfig;

	public FeatureFiltersIcons FiltersIcons;

	public LevelupColors LevelupColors;

	public float DialogCameraYCorrection = -0.15f;

	[Header("Talent Groups")]
	public Color SingleAcronymColor;

	public Color GroupAcronymColor;

	public TalentGroups TalentGroups = new TalentGroups();

	[SerializeField]
	private BlueprintDlcRewardReference m_DlcRewardForVoidshipArsenalAvailable;

	public List<BlueprintUnitReference> UnitReferencesNoAugmentations;

	public List<BlueprintRaceReference> UnitRaceNoAugmentations;

	public BlueprintFeatureReference ManipulusOccupationReference;

	public BlueprintFeatureReference PasqalOccupationReference;

	public BlueprintFeatureReference ForgeworldHomeworldReference;

	public BlueprintAreaReference MedicareAreaReference;

	public BlueprintAreaReference VoidshipBridgeAreaReference;

	public BlueprintResourceReference CombativityReference;

	public List<BlueprintAbilityReference> ManipulusMagnarailAbilities = new List<BlueprintAbilityReference>();

	public QuestDlcConfig DlcIconConfig;

	public AugmentationsSlotsReferences UIAugmentationsSlotsReferences;

	public AugmentationsSlotDefaultIcons UIAugmentationsSlotDefaultIcons;

	public AugmentationsSpaceBarkBanters UIAugmentationsBarkBanters;

	public AugmentationsDefaultAugmentsReferences UIAugmentationsDefaultAugmentsReferences;

	public AugmentationsSlotTraumaLockerReferences UIAugmentationsTraumaSlotReferences;

	[Header("Augmentation Atlas")]
	public CharacterAtlasData AugmentationAtlasData;

	public NecronTimerConfigurations NecronTimer;

	public static UIConfig Instance => BlueprintRoot.Instance.UIConfig;

	public BlueprintUISound BlueprintUISound => m_BlueprintUISound?.Get();

	public BlueprintUINetLobbyTutorial BlueprintUINetLobbyTutorial => m_BlueprintUINetLobbyTutorial?.Get();

	public BlueprintUILocalMapLegend BlueprintUILocalMapLegend => m_BlueprintUILocalMapLegend?.Get();

	public UIViewConfigs ViewConfigs => m_ViewConfigs?.Get();

	public BlueprintAbility ReloadAbility => m_ReloadAbility?.Get();

	public InteractionVariantVisualSetsBlueprint InteractionVariantVisualSetsBlueprint => m_InteractionVariantVisualSetsBlueprint?.Get();

	public bool IsVoidshipArsenalAvailable
	{
		get
		{
			if (!m_DlcRewardForVoidshipArsenalAvailable.IsEmpty())
			{
				return m_DlcRewardForVoidshipArsenalAvailable.Get().IsAvailable;
			}
			return false;
		}
	}

	public bool IsManipulusMagnarail(BlueprintAbility ability)
	{
		return ManipulusMagnarailAbilities.Any((BlueprintAbilityReference a) => a.Get() == ability);
	}
}
