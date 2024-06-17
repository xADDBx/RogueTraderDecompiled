using System;
using System.Collections.Generic;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Area;

[TypeId("700954073a6aeac449fc2fe45dee75d5")]
public class BlueprintAreaPreset : BlueprintScriptableObject
{
	[InfoBox(Text = "Stop the game if this preset contains errors")]
	public bool IsNewGamePreset;

	[ValidateNotNull]
	[SerializeField]
	private BlueprintAreaReference m_Area;

	[ValidateNotNull]
	[DependenciesFilter]
	[SerializeField]
	private BlueprintAreaEnterPointReference m_EnterPoint;

	[ValidateNoNullEntries]
	public List<BlueprintAreaMechanicsReference> AlsoLoadMechanics;

	public bool MakeAutosave;

	[SerializeField]
	private DifficultyPresetAsset m_OverrideGameDifficulty;

	[Header("Party")]
	[ValidateNotNull]
	[SerializeField]
	private BlueprintUnitReference m_PlayerCharacter;

	public bool CharGen;

	public Alignment Alignment = Alignment.TrueNeutral;

	public int PartyXp;

	[ValidateNoNullEntries]
	public List<BlueprintUnitReference> Companions = new List<BlueprintUnitReference>();

	[ValidateNoNullEntries]
	public List<BlueprintUnitReference> CompanionsRemote = new List<BlueprintUnitReference>();

	[ValidateNoNullEntries]
	public List<BlueprintUnitReference> ExCompanions = new List<BlueprintUnitReference>();

	public ActionList StartGameActions;

	[SerializeField]
	[Header("State")]
	private BlueprintDlcRewardCampaignReference m_CampaignDlc;

	[SerializeField]
	private BlueprintUnitReference m_PlayerShip;

	[SerializeField]
	private BlueprintStarSystemMapReference m_OverrideStartingStarSystemArea;

	public List<UnlockValuePair> UnlockedFlags = new List<UnlockValuePair>();

	[ValidateNoNullEntries]
	public List<BlueprintQuestObjectiveReference> StartedQuests = new List<BlueprintQuestObjectiveReference>();

	[ValidateNoNullEntries]
	public List<BlueprintQuestObjectiveReference> FinishedQuests = new List<BlueprintQuestObjectiveReference>();

	[ValidateNoNullEntries]
	public List<BlueprintQuestObjectiveReference> FailedQuests = new List<BlueprintQuestObjectiveReference>();

	[HideInInspector]
	public List<BlueprintEtudeReference> StartEtudesNonRecursively = new List<BlueprintEtudeReference>();

	[ValidateNoNullEntries]
	public List<BlueprintEtudeReference> StartEtudes = new List<BlueprintEtudeReference>();

	[ValidateNoNullEntries]
	public List<BlueprintEtudeReference> ForceCompleteEtudes = new List<BlueprintEtudeReference>();

	[Header("Dialogs State")]
	[DependenciesFilter]
	[ValidateNoNullEntries]
	public List<BlueprintCueBaseReference> CuesSeen = new List<BlueprintCueBaseReference>();

	[DependenciesFilter]
	[ValidateNoNullEntries]
	public List<BlueprintAnswerReference> AnswersSelected = new List<BlueprintAnswerReference>();

	public bool RevealGlobalmap;

	[Header("Settings")]
	public bool SetLocale;

	[ShowIf("SetLocale")]
	public Locale Locale;

	[NonSerialized]
	public object DependenciesInfo;

	public BlueprintDlcRewardCampaign CampaignDlc => m_CampaignDlc;

	public BlueprintArea Area
	{
		get
		{
			return m_Area?.Get();
		}
		set
		{
			m_Area = value.ToReference<BlueprintAreaReference>();
		}
	}

	public BlueprintAreaEnterPoint EnterPoint
	{
		get
		{
			return m_EnterPoint?.Get();
		}
		set
		{
			m_EnterPoint = value.ToReference<BlueprintAreaEnterPointReference>();
		}
	}

	public BlueprintUnit PlayerCharacter => m_PlayerCharacter?.Get();

	public BlueprintUnit PlayerShip => m_PlayerShip?.Get();

	public BlueprintStarSystemMap OverrideStartingStarSystemArea => m_OverrideStartingStarSystemArea?.Get();

	public DifficultyPresetAsset OverrideGameDifficulty
	{
		get
		{
			return m_OverrideGameDifficulty;
		}
		set
		{
			m_OverrideGameDifficulty = value;
		}
	}

	public virtual void SetupState()
	{
		Game.Instance.Player.StartPreset = this;
		if (CampaignDlc != null)
		{
			Game.Instance.Player.StartPreset = CampaignDlc.Campaign.StartGamePreset;
		}
		if (m_OverrideGameDifficulty?.Preset != null)
		{
			SettingsController.Instance.DifficultyPresetsController.SetDifficultyPreset(m_OverrideGameDifficulty?.Preset, confirm: true);
		}
		foreach (UnlockValuePair item in UnlockedFlags.EmptyIfNull())
		{
			if ((bool)item.Flag)
			{
				Game.Instance.Player.UnlockableFlags.Unlock(item.Flag);
				Game.Instance.Player.UnlockableFlags.SetFlagValue(item.Flag, item.Value);
			}
		}
		foreach (BlueprintQuestObjectiveReference item2 in StartedQuests.EmptyIfNull().NotNull())
		{
			Game.Instance.Player.QuestBook.GiveObjective(item2.Get());
		}
		foreach (BlueprintQuestObjectiveReference item3 in FinishedQuests.EmptyIfNull().NotNull())
		{
			Game.Instance.Player.QuestBook.CompleteObjective(item3.Get());
		}
		foreach (BlueprintQuestObjectiveReference item4 in FailedQuests.EmptyIfNull().NotNull())
		{
			Game.Instance.Player.QuestBook.FailObjective(item4.Get());
		}
		foreach (BlueprintEtudeReference item5 in ForceCompleteEtudes.EmptyIfNull().NotNull())
		{
			if (!item5.IsEmpty())
			{
				Game.Instance.Player.EtudesSystem.MarkEtudeCompleted(item5.Get());
				StartParentEtudeRecursively(item5.Get());
			}
		}
		foreach (BlueprintEtudeReference item6 in StartEtudesNonRecursively.EmptyIfNull().NotNull())
		{
			if (!item6.IsEmpty())
			{
				Game.Instance.Player.EtudesSystem.StartEtude(item6.Get());
			}
		}
		foreach (BlueprintEtudeReference item7 in StartEtudes.EmptyIfNull().NotNull())
		{
			if (!item7.IsEmpty())
			{
				StartParentEtudeRecursively(item7.Get());
			}
		}
		foreach (BlueprintCueBaseReference item8 in CuesSeen.EmptyIfNull().NotNull())
		{
			Game.Instance.Player.Dialog.ShownCues.Add(item8.Get());
		}
		foreach (BlueprintAnswerReference item9 in AnswersSelected.EmptyIfNull().NotNull())
		{
			Game.Instance.Player.Dialog.SelectedAnswers.Add(item9.Get());
		}
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			allCharacter.Progression.AdvanceExperienceTo(PartyXp, log: false);
		}
		foreach (ItemEntity item10 in Game.Instance.Player.Inventory)
		{
			item10.Identify();
		}
		if (OverrideStartingStarSystemArea != null)
		{
			Game.Instance.Player.PreviousVisitedArea = OverrideStartingStarSystemArea;
		}
		if (SetLocale)
		{
			LocalizationManager.Instance.CurrentLocale = Locale;
		}
	}

	private void StartParentEtudeRecursively(BlueprintEtude etude)
	{
		if (!etude.Parent.IsEmpty())
		{
			StartParentEtudeRecursively(etude.Parent.Get());
		}
		Game.Instance.Player.EtudesSystem.StartEtude(etude);
	}

	public void FillPlayerCharacter()
	{
		m_PlayerCharacter = Game.Instance.Player.MainCharacterEntity.Blueprint.ToReference<BlueprintUnitReference>();
	}
}
