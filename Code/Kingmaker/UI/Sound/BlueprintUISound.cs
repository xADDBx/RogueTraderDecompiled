using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[TypeId("ab051f837cfe69b4d989791539b6838c")]
public class BlueprintUISound : BlueprintScriptableObject
{
	[Serializable]
	public class UISoundCharacter
	{
		public UISound NewLevelNotification;

		public UISound LevelUpgradedNotification;

		public UISound CharacterSelectAll;

		public UISound CharacterSelect;

		public UISound CharacterStatsShow;

		public UISound CharacterStatsHide;

		public UISound CharacterInfoShow;

		public UISound CharacterInfoHide;

		public UISound CharacterDollAnimationShow;
	}

	[Serializable]
	public class UISoundShip
	{
		public UISound ShipDollAnimationShow;

		public UISound ShipUpgrade;

		public UISound ShipDowngrade;
	}

	[Serializable]
	public class UISoundInventory
	{
		public UISound InventoryClose;

		public UISound InventoryOpen;

		public UISound ErrorEquip;

		public UISound InventorySlotsShow;

		public UISound InventoryVisualSettingsShow;

		public UISound InventoryVisualSettingsHide;
	}

	[Serializable]
	public class UISoundJournal
	{
		public UISound JournalClose;

		public UISound JournalOpen;

		public UISound NewQuest;

		public UISound NewInformation;
	}

	[Serializable]
	public class UISoundLocalMap
	{
		public UISound MapClose;

		public UISound MapOpen;

		public UISound ShowHideLocalMapLegend;
	}

	[Serializable]
	public class UISoundDialogue
	{
		public UISound DialogueOpen;

		public UISound DialogueClose;

		public UISound BookPageTurn;

		public UISound BookOpen;

		public UISound BookClose;
	}

	[Serializable]
	public class UISoundLoot
	{
		public UISound LootCollectOne;

		public UISound LootCollectAll;

		public UISound LootWindowOpen;

		public UISound LootWindowClose;

		public UISound LootCollectGold;

		public UISound LootLeftPanelShow;

		public UISound LootLeftPanelHide;

		public UISound LootRightPanelShow;

		public UISound LootRightPanelHide;
	}

	[Serializable]
	public class UISoundChargen
	{
		public UISound ChargenPortraitChange;

		public UISound ChargenCompleteClick;
	}

	[Serializable]
	public class UISoundSystems
	{
		public UISound PauseSound;

		public UISound BlinkAttentionMark;

		public UISound FullscreenWindowFadeShow;

		public UISound FullscreenWindowFadeHide;
	}

	[Serializable]
	public class UISoundFormation
	{
		public UISound FormationOpen;

		public UISound FormationClose;
	}

	[Serializable]
	public class UISoundGroupChanger
	{
		public UISound GroupChangerOpen;

		public UISound GroupChangerClose;
	}

	[Serializable]
	public class UISoundEncyclopedia
	{
		public UISound EncyclopediaOpen;

		public UISound EncyclopediaClose;
	}

	[Serializable]
	public class UISoundButtons
	{
		public UISound ButtonHover;

		public UISound ButtonClick;

		public UISound PlastickButtonHover;

		public UISound PlastickButtonClick;

		public UISound ExitToWarpButtonHover;

		public UISound ExitToWarpButtonClick;

		public UISound FinishChargenButtonHover;

		public UISound FinishChargenButtonClick;

		public UISound LootCollectAllButtonHover;

		public UISound LootCollectAllButtonClick;

		public UISound DoctrineNextButtonHover;

		public UISound DoctrineNextButtonClick;

		public UISound PaperComponentSoundHover;

		public UISound PaperComponentSoundClick;

		public UISound AnalogButtonHover;

		public UISound AnalogButtonClick;
	}

	[Serializable]
	public class UISoundConsoleHints
	{
		public UISound ConsoleHintClick;

		public UISound ConsoleHintHoldStart;

		public UISound ConsoleHintHoldStop;
	}

	[Serializable]
	public class UISoundSettings
	{
		public UISound SettingsOpen;

		public UISound SettingsClose;

		public UISound SettingsKeyBindingOpen;

		public UISound SettingsKeyBindingClose;

		public UISound SettingsSliderMove;
	}

	[Serializable]
	public class UISoundCombat
	{
		public UISound NotInCombatSetWaypointClick;

		public UISound ActionBarSlotClick;

		public UISound ActionBarCanNotSlotClick;

		public UISound MomentumHeroicActReached;

		public UISound MomentumDesperateMeasuresReached;

		public UISound MomentumHighlightOn;

		public UISound MomentumHighlightOff;

		public UISound CombatStart;

		public UISound CombatEnd;

		public UISound EndTurn;

		public UISound NewRound;

		public UISound CombatGridHover;

		public UISound CombatGridSetWaypointClick;

		public UISound CombatGridConfirmActionClick;

		public UISound CombatGridCantPerformActionClick;

		public UISound UnitDeath;

		public UISound ExitBattlePopupShow;

		public UISound ExitBattlePopupExperienceGrowStart;

		public UISound ExitBattlePopupExperienceGrowStop;
	}

	[Serializable]
	public class UISoundSelector
	{
		public UISound SelectorStart;

		public UISound SelectorLoopStart;

		public UISound SelectorLoopStop;

		public UISound SelectorStop;
	}

	[Serializable]
	public class UISoundPantograph
	{
		public UISound PantographStart;

		public UISound PantographLoopStart;

		public UISound PantographLoopStop;

		public UISound PantographStop;
	}

	[Serializable]
	public class UISoundTooltip
	{
		public UISound TooltipHide;

		public UISound TooltipShow;
	}

	[Serializable]
	public class UISoundHint
	{
		public UISound HintShow;

		public UISound HintHide;
	}

	[Serializable]
	public class UISoundScrambledText
	{
		public UISound ScrambledTextLoopStart;

		public UISound ScrambledTextLoopStop;
	}

	[Serializable]
	public class UISoundDropdownMenu
	{
		public UISound DropdownMenuShow;

		public UISound DropdownMenuHide;
	}

	[Serializable]
	public class UISoundTutorial
	{
		public UISound ShowBigTutorial;

		public UISound HideBigTutorial;

		public UISound ShowSmallTutorial;

		public UISound HideSmallTutorial;

		public UISound ChangeTutorialPage;

		public UISound BanTutorialType;
	}

	[Serializable]
	public class UISoundActionBar
	{
		public UISound ActionBarShow;

		public UISound ActionBarHide;

		public UISound ActionBarSwitch;

		public UISound WeaponListOpen;

		public UISound WeaponListClose;

		public UISound DPadShow;

		public UISound DPadHide;
	}

	[Serializable]
	public class UISoundCombatLog
	{
		public UISound CombatLogOpen;

		public UISound CombatLogClose;

		public UISound CombatLogFiltersOpen;

		public UISound CombatLogFiltersClose;

		public UISound CombatLogSizeChanged;
	}

	[Serializable]
	public class UISoundInitiativeTracker
	{
		public UISound InitiativeTrackerShow;

		public UISound InitiativeTrackerHide;

		public UISound InitiativeTrackerRoundCount;
	}

	[Serializable]
	public class UISoundGreenMessageLine
	{
		public UISound GreenMessageLineShow;

		public UISound GreenMessageLineHide;
	}

	[Serializable]
	public class UISoundMessageBox
	{
		public UISound MessageBoxShow;

		public UISound MessageBoxHide;
	}

	[Serializable]
	public class UISoundCoop
	{
		public UISound GroundPing;

		public UISound MobPing;

		public UISound DialogVotePing;

		public UISound ActionBarAbilityPing;
	}

	[Serializable]
	public class UISoundSpaceExploration
	{
		public UISound PlanetScanAnimation;

		public UISound PlanetScanPointsAppear;

		public UISound GetLoot;

		public UISound DialogEvent;

		public UISound ExtractuimSet;

		public UISound SystemEvent;

		public UISound ExitToKoronusMap;

		public UISound KoronusRouteBuild;

		public UISound KoronusRouteImprove;

		public UISound KoronusRouteImproveToDangerous;

		public UISound KoronusRouteImproveToUnsafe;

		public UISound KoronusRouteImproveToSafe;

		public UISound KoronusRouteButtonHover;

		public UISound KoronusRouteButtonUnHover;

		public UISound CircleArcsShow;

		public UISound CircleArcsHide;
	}

	[Serializable]
	public class UISoundSpaceColonization
	{
		public UISound PlanetColonized;

		public UISound ProjectWindowOpen;

		public UISound ProjectWindowButtonHover;

		public UISound StartProject;

		public UISound ColonyEvent;

		public UISound WindowOpenFromMap;

		public UISound ColonizationWindowClose;

		public UISound NoConnect;
	}

	[Serializable]
	public class UITitlesSound
	{
		public UISound TitlesBackgroundMusic;
	}

	[Serializable]
	public class UISoundLoadingScreen
	{
		public UISound FinishGlitch;

		public UISound WaitForUserInputShow;

		public UISound WaitForUserInputHide;
	}

	[Serializable]
	public class UISoundMainMenu
	{
		public UISound ButtonsFirstLaunchFxAnimation;

		public UISound ButtonsFxAnimation;

		public UISound MessageOfTheDayShow;
	}

	[Serializable]
	public class UISoundPartySelectorConsole
	{
		public UISound SelectOne;

		public UISound UnselectOne;

		public UISound SelectAll;

		public UISound UnselectAll;
	}

	[Serializable]
	public class UISoundShipHealthAndRepair
	{
		public UISound HowManyHealthWillRepairLineMove;

		public UISound HealthLineMove;

		public UISound ShipRepaired;
	}

	[Serializable]
	public class UISoundRewards
	{
		public UISound ColonyRewardsShowWindow;

		public UISound ColonyRewardsHideWindow;

		public UISound CargoRewardsShowWindow;

		public UISound CargoRewardsHideWindow;
	}

	[Serializable]
	public class UISoundVendor
	{
		public UISound Deal;

		public UISound SellCargo;
	}

	[Serializable]
	public class UISound
	{
		[AkEventReference]
		public string Id;

		public void Play(GameObject gameObject = null)
		{
			if (gameObject == null)
			{
				UISounds.Instance.Play(this);
			}
			else
			{
				UISounds.Instance.Play(this, gameObject);
			}
		}
	}

	[Tooltip("This event will be assigned to missing UISounds when \"Fix Missing Events\" button is clicked.")]
	public UISound DoNothingEvent;

	[Header("Sounds")]
	public UISoundCharacter Character;

	public UISoundShip Ship;

	public UISoundInventory Inventory;

	public UISoundJournal Journal;

	public UISoundLocalMap LocalMap;

	public UISoundDialogue Dialogue;

	public UISoundLoot Loot;

	public UISoundChargen Chargen;

	public UISoundSystems Systems;

	public UISoundFormation Formation;

	public UISoundGroupChanger GroupChanger;

	public UISoundEncyclopedia Encyclopedia;

	public UISoundButtons Buttons;

	public UISoundConsoleHints ConsoleHints;

	public UISoundSettings Settings;

	public UISoundCombat Combat;

	public UISoundSelector Selector;

	public UISoundPantograph Pantograph;

	public UISoundTooltip Tooltip;

	public UISoundHint Hint;

	public UISoundScrambledText ScrambledText;

	public UISoundDropdownMenu DropdownMenu;

	public UISoundTutorial Tutorial;

	public UISoundActionBar ActionBar;

	public UISoundCombatLog CombatLog;

	public UISoundInitiativeTracker InitiativeTracker;

	public UISoundGreenMessageLine GreenMessageLine;

	public UISoundMessageBox MessageBox;

	public UISoundCoop Coop;

	public UISoundSpaceExploration SpaceExploration;

	public UISoundSpaceColonization SpaceColonization;

	public UITitlesSound TitlesSound;

	public UISoundLoadingScreen LoadingScreen;

	public UISoundMainMenu MainMenu;

	public UISoundPartySelectorConsole PartySelectorConsole;

	public UISoundShipHealthAndRepair ShipHealthAndRepair;

	public UISoundRewards Rewards;

	public UISoundVendor Vendor;
}
