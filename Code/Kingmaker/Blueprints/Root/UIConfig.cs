using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;
using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Interaction;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.UIConfigComponents;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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

	public static UIConfig Instance => BlueprintRoot.Instance.UIConfig;

	public BlueprintUISound BlueprintUISound => m_BlueprintUISound?.Get();

	public BlueprintUINetLobbyTutorial BlueprintUINetLobbyTutorial => m_BlueprintUINetLobbyTutorial?.Get();

	public BlueprintUILocalMapLegend BlueprintUILocalMapLegend => m_BlueprintUILocalMapLegend?.Get();

	public UIViewConfigs ViewConfigs => m_ViewConfigs?.Get();

	public BlueprintAbility ReloadAbility => m_ReloadAbility?.Get();

	public InteractionVariantVisualSetsBlueprint InteractionVariantVisualSetsBlueprint => m_InteractionVariantVisualSetsBlueprint?.Get();
}
