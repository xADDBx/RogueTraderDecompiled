using System;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UICharGen
{
	public LocalizedString Skills;

	public LocalizedString ChooseName;

	public LocalizedString Complete;

	public LocalizedString Next;

	public LocalizedString Back;

	public LocalizedString Voice;

	public LocalizedString BodyType;

	public LocalizedString BodyConstitution;

	public LocalizedString Face;

	public LocalizedString SkinTone;

	public LocalizedString HairStyle;

	public LocalizedString HairColor;

	public LocalizedString TattooColor;

	public LocalizedString PrimaryClothColor;

	public LocalizedString SecondaryClothColor;

	public LocalizedString HitPoints;

	public LocalizedString Beard;

	public LocalizedString BeardColor;

	public LocalizedString Appearance;

	public LocalizedString Eyebrows;

	public LocalizedString EyebrowsColor;

	public LocalizedString Scars;

	public LocalizedString FacePaint;

	public LocalizedString Implant;

	public LocalizedString SoulMark;

	public LocalizedString Homeworld;

	public LocalizedString ImperialHomeworldChildSelection;

	public LocalizedString ForgeHomeworldChildSelection;

	public LocalizedString SanctionedPsykerSelection;

	public LocalizedString Occupation;

	public LocalizedString Navigator;

	public LocalizedString DarkestHour;

	public LocalizedString MomentOfTriumph;

	public LocalizedString Careers;

	public LocalizedString Attributes;

	public LocalizedString Hair;

	public LocalizedString Tattoo;

	public LocalizedString Implants;

	public LocalizedString Servoskull;

	public LocalizedString NavigatorMutations;

	public LocalizedString Ship;

	public LocalizedString Summary;

	public LocalizedString SureWantClose;

	public LocalizedString CloseCoopChargenNotRt;

	public LocalizedString EnterSearchTextHere;

	public LocalizedString Pregen;

	public LocalizedString CustomCharacterPregen;

	public LocalizedString CreateCustomCharacter;

	public LocalizedString AvailableStatsPointLeft;

	public LocalizedString NoAvailableStatsPointLeft;

	public LocalizedString CannotAdvanceStatHint;

	public LocalizedString ShowVisualSettings;

	public LocalizedString HideVisualSettings;

	public LocalizedString Background;

	public LocalizedString BackgroundFeatures;

	public LocalizedString BackgroundStatsBonuses;

	public LocalizedString BackgroundSkillsBonuses;

	public LocalizedString BackgroundUnlockedFeaturesForLevelUp;

	public LocalizedString BackgroundStatsForLevelUp;

	public LocalizedString BackgroundSkillsForLevelUp;

	public LocalizedString BackgroundTalentsForLevelUp;

	public LocalizedString EditName;

	public LocalizedString SetRandomName;

	public LocalizedString EditNameButton;

	public LocalizedString SetRandomNameButton;

	public LocalizedString PhaseNotCompleted;

	public LocalizedString InspectCareer;

	public LocalizedString RespecWindowHeader;

	public LocalizedString RespecSelectCharacter;

	public LocalizedString RespecWindowWarning;

	public LocalizedString RespecCostPF;

	public LocalizedString SwitchPageSet;

	public LocalizedString PlayVoicePreview;

	public LocalizedString SwitchPortraitsCategoryTab;

	public LocalizedString ShouldSetAllAttributesPointsWarning;

	public LocalizedString CharacterSkillPoints;

	public LocalizedString SwitchToPantograph;

	public LocalizedString SwitchToAppearance;

	public LocalizedString NothingToChoose;

	[Header("Portrait")]
	public LocalizedString Portrait;

	public LocalizedString UploadPortraitManual;

	public LocalizedString PortraitCategoryDefault;

	public LocalizedString PortraitCategoryWarhammer;

	public LocalizedString PortraitCategoryCustom;

	public LocalizedString PortraitCategoryNavigator;

	public LocalizedString ChangePortrait;

	public LocalizedString ChangePortraitDescription;

	public LocalizedString ChangePortraitDescriptionConsole;

	public LocalizedString CustomPortraitHeader;

	public LocalizedString OpenPortraitFolder;

	public LocalizedString RefreshPortrait;

	public LocalizedString AddPortrait;

	public LocalizedString WaitForDownloadingPortraits;

	[Header("Buttons Hints")]
	public LocalizedString SelectDoctrineHint;

	public LocalizedString SpreadOutPointsHint;

	public LocalizedString SkillPointsContainerHint;

	[Header("Pregens")]
	public LocalizedString CreateNewCompanion;

	public LocalizedString CreateNewCompanionDescription;

	public LocalizedString CreateNewNavigator;

	public LocalizedString CreateNewNavigatorDescription;

	public string GetPageLabelByType(CharGenAppearancePageType pageType)
	{
		return pageType switch
		{
			CharGenAppearancePageType.Portrait => Portrait, 
			CharGenAppearancePageType.General => Appearance, 
			CharGenAppearancePageType.Hair => Hair, 
			CharGenAppearancePageType.Tattoo => Tattoo, 
			CharGenAppearancePageType.Implants => Implants, 
			CharGenAppearancePageType.Voice => Voice, 
			CharGenAppearancePageType.Servoskull => Servoskull, 
			CharGenAppearancePageType.NavigatorMutations => NavigatorMutations, 
			_ => string.Empty, 
		};
	}

	public string GetPhaseName(CharGenPhaseType type)
	{
		return type switch
		{
			CharGenPhaseType.Pregen => Pregen, 
			CharGenPhaseType.Appearance => Appearance, 
			CharGenPhaseType.SoulMark => SoulMark, 
			CharGenPhaseType.Homeworld => Homeworld, 
			CharGenPhaseType.Occupation => Occupation, 
			CharGenPhaseType.MomentOfTriumph => MomentOfTriumph, 
			CharGenPhaseType.DarkestHour => DarkestHour, 
			CharGenPhaseType.Career => Careers, 
			CharGenPhaseType.Attributes => Attributes, 
			CharGenPhaseType.Ship => Ship, 
			CharGenPhaseType.Summary => Summary, 
			CharGenPhaseType.ImperialHomeworldChild => ImperialHomeworldChildSelection, 
			CharGenPhaseType.ForgeHomeworldChild => ForgeHomeworldChildSelection, 
			CharGenPhaseType.SanctionedPsyker => SanctionedPsykerSelection, 
			CharGenPhaseType.Navigator => Navigator, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
