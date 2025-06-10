using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.GameCommands;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;

public class CareerPathVM : SelectionGroupEntityVM, ILevelUpManagerUIHandler, ISubscriber, IUnitGainExperienceHandler, ISubscriber<IBaseUnitEntity>, IRankEntryFocusHandler, ISetTooltipHandler
{
	public readonly BaseUnitEntity Unit;

	public readonly ReactiveProperty<BaseUnitEntity> TooltipsPreviewUnit = new ReactiveProperty<BaseUnitEntity>();

	public readonly BlueprintCareerPath CareerPath;

	public readonly string Name;

	public readonly string Description;

	private List<BlueprintSkillAdvancement> m_SkillsLabels;

	private List<BlueprintAttributeAdvancement> m_AttributesLabels;

	public readonly int MaxRank;

	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<int> CurrentRank = new ReactiveProperty<int>();

	public readonly AutoDisposingList<CareerPathRankEntryVM> RankEntries = new AutoDisposingList<CareerPathRankEntryVM>();

	public readonly ReactiveProperty<bool> ReadOnly = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> CanUpgrade = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> CanCommit = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> AllVisited = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveProperty<float> Progress = new ReactiveProperty<float>();

	public readonly ReactiveProperty<bool> HasNewValidSelections = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsRecommended = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsHighlighted = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<int> CareerUpgrades = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CareerExp = new ReactiveProperty<int>();

	public readonly ReactiveProperty<bool> IsCurrentRankEntryItem = new ReactiveProperty<bool>();

	public readonly BoolReactiveProperty IsDescriptionShowed = new BoolReactiveProperty();

	public readonly ReactiveCommand OnUpdateData = new ReactiveCommand();

	public readonly ReactiveCommand OnUpdateSelected = new ReactiveCommand();

	public readonly ReactiveCommand OnCommit = new ReactiveCommand();

	public readonly ReactiveProperty<CareerItemState> ItemState = new ReactiveProperty<CareerItemState>();

	public readonly ReactiveProperty<CharGenChangeNameMessageBoxVM> PetChangeNameVM = new ReactiveProperty<CharGenChangeNameMessageBoxVM>();

	private TooltipBaseTemplate m_CareerTooltip;

	private TooltipBaseTemplate m_CareerProgressionTooltip;

	private TooltipBaseTemplate m_CareerProgressionDesc;

	public List<BlueprintCareerPath> PrerequisiteCareerPaths = new List<BlueprintCareerPath>();

	public List<BlueprintFeature> PrerequisiteFeatures = new List<BlueprintFeature>();

	public readonly CharInfoAvailableRanksVM AvailableRanksVM;

	public readonly InfoSectionVM SelectedItemInfoSectionVM;

	public readonly InfoSectionVM TabInfoSectionVM;

	public readonly IntReactiveProperty CurrentProgress = new IntReactiveProperty();

	public readonly List<IRankEntrySelectItem> RankEntriesScan = new List<IRankEntrySelectItem>();

	public readonly ReactiveCollection<IRankEntrySelectItem> FeaturesToVisit = new ReactiveCollection<IRankEntrySelectItem>();

	public readonly ReactiveProperty<IRankEntrySelectItem> PointerItem = new ReactiveProperty<IRankEntrySelectItem>();

	public readonly Sprite PlayerShipSprite;

	private AddedOnLevelUpFeatures m_AddedOnLevelUpFeatures;

	public List<BlueprintSkillAdvancement> AdvancementSkills => m_SkillsLabels ?? (m_SkillsLabels = GetStatAdvancements<BlueprintSkillAdvancement>().ToList());

	public List<BlueprintAttributeAdvancement> AdvancementAttributes => m_AttributesLabels ?? (m_AttributesLabels = GetStatAdvancements<BlueprintAttributeAdvancement>().ToList());

	public TooltipBaseTemplate CareerHintTemplate => new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.CareerPathHeader, UIStrings.Instance.CharacterSheet.CareerPathDescription);

	public TooltipBaseTemplate CareerTooltip => m_CareerTooltip ?? (m_CareerTooltip = new TooltipTemplateCareer(this, isScreenView: true));

	private TooltipBaseTemplate CareerProgressionTooltip => m_CareerProgressionTooltip ?? (m_CareerProgressionTooltip = new TooltipTemplateCareerProgression(this));

	private TooltipBaseTemplate CareerProgressionDesc => m_CareerProgressionDesc ?? (m_CareerProgressionDesc = new TooltipTemplateCareerProgressionDesc(this));

	private LevelUpManager LevelUpManager => GetLevelupManager();

	public CareerPathUIMetaData CareerPathUIMetaData { get; }

	public CalculatedPrerequisite Prerequisite { get; private set; }

	public BaseUnitProgressionVM UnitProgressionVM { get; }

	public IEnumerable<RankEntrySelectionVM> AllSelections => RankEntries.SelectMany((CareerPathRankEntryVM i) => i.Selections);

	public List<IRankEntrySelectItem> VisitedFeatures => RankEntriesScan.Except(FeaturesToVisit.ToList()).ToList();

	public IEnumerable<RankEntrySelectionVM> AvailableSelections
	{
		get
		{
			(int Min, int Max) levelRange = GetCurrentLevelupRange();
			return AllSelections.Where((RankEntrySelectionVM i) => levelRange.Min <= i.Rank && i.Rank <= levelRange.Max);
		}
	}

	public IRankEntrySelectItem LastEntryToUpgrade { get; private set; }

	public IRankEntrySelectItem FirstEntryToUpgrade { get; private set; }

	public bool IsInProgress => IsCareerInProgress(CareerPath, canUsePreviewUnit: true);

	public bool IsSelectedAndInProgress => IsCareerInProgress(CareerPath, canUsePreviewUnit: false);

	public bool IsFinished => IsCareerFinished(CareerPath);

	public bool IsAvailableToUpgrade => CanUpgrade.Value;

	public RankEntrySelectionVM FirstSelectable => AllSelections.FirstOrDefault((RankEntrySelectionVM i) => i.NeedToSelect || !i.IsValidSelection);

	private bool IsPreviewUnitDisposed => (GetLevelupManager()?.PreviewUnit?.IsDisposed).GetValueOrDefault();

	public bool IsUnlocked
	{
		get
		{
			if (!IsAvailableToUpgrade && !IsInProgress)
			{
				return IsFinished;
			}
			return true;
		}
	}

	public AddedOnLevelUpFeatures AddedOnLevelUpFeatures => m_AddedOnLevelUpFeatures;

	public bool HasSelectionsToUpgrade { get; private set; }

	public bool IsInLevelupProcess
	{
		get
		{
			if (!HasSelectionsToUpgrade)
			{
				return CanCommit.Value;
			}
			return true;
		}
	}

	public bool AlreadyInLevelupOther
	{
		get
		{
			if (UnitProgressionVM?.LevelUpManager != null)
			{
				return UnitProgressionVM?.LevelUpManager?.TargetUnit != Unit;
			}
			return false;
		}
	}

	public bool HasDifferentFirstSelectable => UnitProgressionVM.CurrentRankEntryItem.Value != FirstSelectable;

	public CareerPathVM(BaseUnitEntity unit, BlueprintCareerPath careerPath, BaseUnitProgressionVM progressionVM)
		: base(allowSwitchOff: true)
	{
		Unit = unit;
		CareerPath = careerPath;
		UnitProgressionVM = progressionVM;
		Name = careerPath.Name;
		Description = careerPath.Description;
		MaxRank = careerPath.Ranks;
		Icon.Value = CareerPath.Icon;
		ItemState.Value = ((IsUnlocked || CanShowToAnotherCoopPlayer()) ? CareerItemState.Unlocked : CareerItemState.Locked);
		Prerequisite = CalculatedPrerequisite.Calculate(CareerPath, Unit);
		CareerPathUIMetaData = careerPath.GetComponent<CareerPathUIMetaData>();
		AddDisposable(AvailableRanksVM = new CharInfoAvailableRanksVM(this));
		AddDisposable(SelectedItemInfoSectionVM = new InfoSectionVM());
		AddDisposable(TabInfoSectionVM = new InfoSectionVM());
		if (progressionVM != null)
		{
			AddDisposable(progressionVM.CurrentRankEntryItem.Subscribe(delegate(IRankEntrySelectItem item)
			{
				IsCurrentRankEntryItem.Value = item == null;
			}));
		}
		AddDisposable(RefreshView.Subscribe(delegate
		{
			OnUpdateSelected.Execute();
		}));
		AddDisposable(IsDescriptionShowed.Skip(1).Subscribe(delegate(bool value)
		{
			if (!value)
			{
				EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
				{
					h.SetFocusOn(null);
				});
				UpdateSelectedItemInfoSection(UnitProgressionVM.CurrentRankEntryItem.Value);
			}
		}));
		AddDisposable(FeaturesToVisit.ObserveCountChanged().Subscribe(delegate(int value)
		{
			AllVisited.Value = value == 0;
		}));
		AddDisposable(EventBus.Subscribe(this));
		PlayerShipSprite = Game.Instance?.Player?.PlayerShip?.Blueprint?.Icon;
	}

	protected override void DisposeImplementation()
	{
		RankEntries.Clear();
		SetTooltipsDirty();
		IsDescriptionShowed.Value = false;
		m_AddedOnLevelUpFeatures = null;
		TooltipsPreviewUnit.Value?.Dispose();
		TooltipsPreviewUnit.Value = null;
	}

	public void SetupFeaturesFromSelected()
	{
		m_AddedOnLevelUpFeatures = new AddedOnLevelUpFeatures(this);
		for (int i = 0; i < AllSelections.Count(); i++)
		{
			RankEntrySelectionVM rankEntrySelectionVM = AllSelections.ElementAt(i);
			int entryId = RankEntriesScan.IndexOf(rankEntrySelectionVM);
			m_AddedOnLevelUpFeatures.RefreshSelectedFeatureAtRank(rankEntrySelectionVM.SelectedFeature.Value, entryId);
		}
	}

	private LevelUpManager GetLevelupManager()
	{
		LevelUpManager levelUpManager = UnitProgressionVM?.LevelUpManager;
		if (levelUpManager != null)
		{
			if (levelUpManager.TargetUnit != Unit)
			{
				return null;
			}
			return levelUpManager;
		}
		return null;
	}

	public (int Min, int Max) GetCurrentLevelupRange()
	{
		PartUnitProgression progression = Unit.Progression;
		int characterLevel = progression.CharacterLevel;
		int experienceLevel = progression.ExperienceLevel;
		int rank = progression.Features.GetRank(CareerPath);
		int max = Math.Max(0, experienceLevel - characterLevel);
		int num = Math.Clamp(CareerPath.Ranks - rank, 0, max);
		if (characterLevel == 0)
		{
			if (num <= 0 || CareerPath.Tier != 0)
			{
				return (Min: -1, Max: -1);
			}
			return (Min: rank + 1, Max: rank + num);
		}
		if (num <= 0)
		{
			return (Min: -1, Max: -1);
		}
		return (Min: rank + 1, Max: rank + num);
	}

	public void SetCareerPath()
	{
		UnitProgressionVM.SetCareerPath(this);
		UpdateCareerPath();
		SetupFeaturesFromSelected();
	}

	public void UpdateCareerPath()
	{
		UpdateState(updateRanks: true);
		UpdateCareerTooltip();
		UpdateRankEntriesScan();
		CreateFeaturesToVisit();
		RefreshTooltipUnit();
	}

	private void UpdateRankEntriesScan()
	{
		RankEntriesScan.Clear();
		foreach (CareerPathRankEntryVM rankEntry in RankEntries)
		{
			RankEntriesScan.AddRange(rankEntry.GetRankSlice());
		}
	}

	private void CreateFeaturesToVisit()
	{
		IRankEntrySelectItem nextRankItem = GetNextRankItem(null, skipSelected: false);
		FeaturesToVisit.Clear();
		(int, int) currentLevelupRange = GetCurrentLevelupRange();
		if (!IsInProgress || AlreadyInLevelupOther || (currentLevelupRange.Item1 == -1 && currentLevelupRange.Item2 == -1))
		{
			PointerItem.Value = null;
			return;
		}
		while (nextRankItem != null)
		{
			FeaturesToVisit.Add(nextRankItem);
			nextRankItem = GetNextRankItem(nextRankItem, skipSelected: false);
		}
		PointerItem.Value = FeaturesToVisit.FirstOrDefault();
		FirstEntryToUpgrade = FeaturesToVisit.FirstOrDefault();
		LastEntryToUpgrade = FeaturesToVisit.LastOrDefault();
		UpdateRanks();
	}

	public bool IsVisited(IRankEntrySelectItem item)
	{
		return !FeaturesToVisit.Contains(item);
	}

	public void SetRankEntry(IRankEntrySelectItem rankEntryItem)
	{
		UnitProgressionVM.SetRankEntry(rankEntryItem);
		UpdateState(updateRanks: true);
		UpdateSelectedItemInfoSection(rankEntryItem);
		rankEntryItem?.UpdateReadOnlyState();
		FeatureGroup? featureGroup = rankEntryItem?.GetFeatureGroup();
		if (m_AddedOnLevelUpFeatures != null && featureGroup.HasValue && m_AddedOnLevelUpFeatures.NeedUpdateFor(featureGroup.Value))
		{
			rankEntryItem.UpdateFeatures();
		}
		EventBus.RaiseEvent(delegate(IRankEntryConfirmClickHandler h)
		{
			h.OnRankEntryConfirmClick();
		});
	}

	public void SetFirstSelectableRankEntry()
	{
		FirstSelectable?.HandleClick();
	}

	public void SelectNextItem(bool skipSelected = true)
	{
		IRankEntrySelectItem nextRankItem = GetNextRankItem(skipSelected);
		if (nextRankItem is RankEntrySelectionVM rankEntrySelectionVM)
		{
			rankEntrySelectionVM.HandleClick();
		}
		else
		{
			SetRankEntry(nextRankItem);
		}
		EventBus.RaiseEvent(delegate(IRankEntryConfirmClickHandler h)
		{
			h.OnRankEntryConfirmClick();
		});
	}

	public void SelectPreviousItem()
	{
		IRankEntrySelectItem previousRankItem = GetPreviousRankItem();
		if (previousRankItem is RankEntrySelectionVM rankEntrySelectionVM)
		{
			rankEntrySelectionVM.HandleClick();
		}
		else
		{
			SetRankEntry(previousRankItem);
		}
	}

	private IRankEntrySelectItem GetPreviousRankItem()
	{
		return GetPreviousRankItem(UnitProgressionVM.CurrentRankEntryItem.Value);
	}

	private IRankEntrySelectItem GetPreviousRankItem(IRankEntrySelectItem currentItem)
	{
		(int Min, int Max) levelsRange = GetCurrentLevelupRange();
		IRankEntrySelectItem firstSelectable;
		if (currentItem == null)
		{
			firstSelectable = FirstSelectable;
			IRankEntrySelectItem rankEntrySelectItem = firstSelectable;
			if (rankEntrySelectItem == null)
			{
				CareerPathRankEntryVM careerPathRankEntryVM = RankEntries.FirstOrDefault((CareerPathRankEntryVM re) => re.Rank == levelsRange.Max);
				if (careerPathRankEntryVM == null)
				{
					return null;
				}
				rankEntrySelectItem = careerPathRankEntryVM.GetLastItem();
			}
			return rankEntrySelectItem;
		}
		IEnumerable<CareerPathRankEntryVM> source = RankEntries.Where((CareerPathRankEntryVM re) => re.Rank >= levelsRange.Min && re.Rank <= levelsRange.Max);
		CareerPathRankEntryVM careerPathRankEntryVM2 = source.FirstOrDefault((CareerPathRankEntryVM re) => re.Selections.Contains(currentItem)) ?? source.FirstOrDefault((CareerPathRankEntryVM re) => re.Features.Contains(currentItem));
		if (careerPathRankEntryVM2 == null)
		{
			firstSelectable = FirstSelectable;
			IRankEntrySelectItem rankEntrySelectItem2 = firstSelectable;
			if (rankEntrySelectItem2 == null)
			{
				CareerPathRankEntryVM careerPathRankEntryVM3 = source.FirstOrDefault((CareerPathRankEntryVM re) => re.Rank == levelsRange.Max);
				if (careerPathRankEntryVM3 == null)
				{
					return null;
				}
				rankEntrySelectItem2 = careerPathRankEntryVM3.GetLastItem();
			}
			return rankEntrySelectItem2;
		}
		int currentRank = careerPathRankEntryVM2.Rank;
		RankEntrySelectionVM firstSelectable2 = FirstSelectable;
		if (firstSelectable2 != null && firstSelectable2.Rank < currentRank)
		{
			return FirstSelectable;
		}
		firstSelectable = source.FirstOrDefault((CareerPathRankEntryVM re) => re.Rank == currentRank)?.GetPreviousFor(currentItem);
		IRankEntrySelectItem rankEntrySelectItem3 = firstSelectable;
		if (rankEntrySelectItem3 == null)
		{
			CareerPathRankEntryVM careerPathRankEntryVM4 = source.FirstOrDefault((CareerPathRankEntryVM re) => re.Rank == currentRank - 1);
			if (careerPathRankEntryVM4 == null)
			{
				return null;
			}
			rankEntrySelectItem3 = careerPathRankEntryVM4.GetLastItem();
		}
		return rankEntrySelectItem3;
	}

	private IRankEntrySelectItem GetNextRankItem(bool skipSelected)
	{
		return GetNextRankItem(UnitProgressionVM.CurrentRankEntryItem.Value, skipSelected);
	}

	private IRankEntrySelectItem GetNextRankItem(IRankEntrySelectItem currentItem, bool skipSelected)
	{
		(int Min, int Max) levelsRange = GetCurrentLevelupRange();
		if (currentItem == null)
		{
			return RankEntries.FirstOrDefault((CareerPathRankEntryVM re) => re.Rank == levelsRange.Min)?.GetFirstItem();
		}
		IEnumerable<CareerPathRankEntryVM> source = RankEntries.Where((CareerPathRankEntryVM re) => re.Rank >= levelsRange.Min && re.Rank <= levelsRange.Max);
		CareerPathRankEntryVM careerPathRankEntryVM = source.FirstOrDefault((CareerPathRankEntryVM re) => re.Selections.Contains(currentItem)) ?? source.FirstOrDefault((CareerPathRankEntryVM re) => re.Features.Contains(currentItem));
		if (careerPathRankEntryVM == null)
		{
			if (!skipSelected)
			{
				return RankEntries.FirstOrDefault((CareerPathRankEntryVM re) => re.Rank == levelsRange.Min)?.GetFirstItem();
			}
			return FirstSelectable;
		}
		int currentRank = careerPathRankEntryVM.Rank;
		IRankEntrySelectItem rankEntrySelectItem = source.FirstOrDefault((CareerPathRankEntryVM re) => re.Rank == currentRank)?.GetNextFor(currentItem);
		IRankEntrySelectItem rankEntrySelectItem2 = rankEntrySelectItem;
		if (rankEntrySelectItem2 == null)
		{
			CareerPathRankEntryVM careerPathRankEntryVM2 = source.FirstOrDefault((CareerPathRankEntryVM re) => re.Rank == currentRank + 1);
			if (careerPathRankEntryVM2 == null)
			{
				return null;
			}
			rankEntrySelectItem2 = careerPathRankEntryVM2.GetFirstItem();
		}
		return rankEntrySelectItem2;
	}

	private void UpdateFirstLastEntriesToUpgrade()
	{
		(int Min, int Max) levelsRange = GetCurrentLevelupRange();
		LastEntryToUpgrade = RankEntries.FirstOrDefault((CareerPathRankEntryVM re) => re.Rank == levelsRange.Max)?.GetLastItem();
		FirstEntryToUpgrade = RankEntries.FirstOrDefault((CareerPathRankEntryVM re) => re.Rank == levelsRange.Min)?.GetFirstItem();
	}

	protected override void DoSelectMe()
	{
	}

	public void ResetNewSelections()
	{
		AvailableSelections.ForEach(delegate(RankEntrySelectionVM i)
		{
			i.ClearSelectedFeature();
		});
	}

	public void Commit()
	{
		if (!CanCommitChanges())
		{
			EventBus.RaiseEvent(delegate(IUIHighlighter h)
			{
				h.HighlightOnce(RankEntrySelectionVM.SelectableHighlighterKey);
			});
			return;
		}
		HasSelectionsToUpgrade = AvailableSelections.Any((RankEntrySelectionVM s) => s.NeedToSelect && !s.SelectionMade);
		if (AvailableSelections.Any((RankEntrySelectionVM s) => s.FeatureGroup == FeatureGroup.PetKeystone))
		{
			PetKeystoneInfoComponent petInfoComponent = AvailableSelections.FirstOrDefault((RankEntrySelectionVM s) => s.FeatureGroup == FeatureGroup.PetKeystone)?.SelectedFeature.Value.Feature.GetComponent<PetKeystoneInfoComponent>();
			if (petInfoComponent == null)
			{
				return;
			}
			if (UnitProgressionVM.Unit.Value.IsCustomCompanion() || UnitProgressionVM.Unit.Value.IsPregenCustomCompanion() || UnitProgressionVM.Unit.Value.IsMainCharacter)
			{
				PetChangeNameVM.Value = new CharGenChangeNameMessageBoxVM(UIStrings.Instance.CharGen.ChooseName, UIStrings.Instance.SettingsUI.DialogApply, delegate(string text)
				{
					UnitProgressionVM.Unit.Value.Description.CustomPetName = text;
				}, delegate(DialogMessageBoxBase.BoxButton value)
				{
					if (value == DialogMessageBoxBase.BoxButton.Yes)
					{
						UnitProgressionVM.Commit();
						OnCommit.Execute();
					}
				}, GetRandomPetName(petInfoComponent.PetType), () => GetRandomPetName(petInfoComponent.PetType), delegate
				{
					PetChangeNameVM?.Value.Dispose();
				});
				return;
			}
		}
		UnitProgressionVM.Commit();
		OnCommit.Execute();
	}

	private string GetRandomPetName(PetType petType, string exceptName = "")
	{
		return BlueprintCharGenRoot.Instance.PregenCharacterNames.GetRandomPetName(petType, exceptName);
	}

	private bool IsCareerInProgress(BlueprintCareerPath careerPath, bool canUsePreviewUnit)
	{
		BaseUnitEntity baseUnitEntity = GetLevelupManager()?.PreviewUnit;
		int num = ((baseUnitEntity != null && canUsePreviewUnit) ? baseUnitEntity.Progression.Features.GetRank(careerPath) : Unit.Progression.Features.GetRank(careerPath));
		if (num != 0)
		{
			if (num == careerPath.Ranks)
			{
				return !IsFinished;
			}
			return true;
		}
		return false;
	}

	private bool IsCareerFinished(BlueprintCareerPath careerPath)
	{
		return Unit.Progression.Features.GetRank(careerPath) == careerPath.Ranks;
	}

	private bool CanUpgradeCareer()
	{
		if (GetLevelupManager() == null && Unit.Progression.CanUpgradePath(CareerPath))
		{
			return !Unit.IsInCombat;
		}
		return false;
	}

	public bool CanShowToAnotherCoopPlayer()
	{
		if (!Unit.CanEditCareer())
		{
			return CanUpgradeCareer();
		}
		return false;
	}

	private bool CanCommitChanges()
	{
		if (!ReadOnly.Value)
		{
			LevelUpManager levelupManager = GetLevelupManager();
			if (levelupManager != null && levelupManager.Path != null && !levelupManager.IsCommitted)
			{
				return levelupManager.IsAllSelectionsMadeAndValid;
			}
			return false;
		}
		return false;
	}

	public void InitializeRankEntries()
	{
		if (RankEntries.Count == 0)
		{
			List<CareerPathRankEntryVM> list = new List<CareerPathRankEntryVM>();
			for (int i = 0; i < CareerPath.RankEntries.Length; i++)
			{
				BlueprintPath.RankEntry rankEntry = CareerPath.RankEntries[i];
				CareerPathRankEntryVM careerPathRankEntryVM = new CareerPathRankEntryVM(i + 1, this, rankEntry);
				list.Add(careerPathRankEntryVM);
				AddDisposable(careerPathRankEntryVM);
			}
			int num = list.Count - 1;
			while (num > 0 && list[num].IsEmpty)
			{
				list.RemoveAt(num);
				num--;
			}
			RankEntries.AddRange(list);
		}
		UpdateFirstLastEntriesToUpgrade();
	}

	public void UpdateState(bool updateRanks)
	{
		UpdateBaseState();
		UpdateIcon();
		if (updateRanks)
		{
			UpdateRanks();
		}
		SetTooltipsDirty();
		OnUpdateData.Execute();
	}

	private void SetTooltipsDirty()
	{
		m_CareerTooltip = null;
		m_CareerProgressionTooltip = null;
	}

	private void UpdateBaseState()
	{
		Prerequisite = CalculatedPrerequisite.Calculate(CareerPath, Unit);
		CurrentRank.Value = ((LevelUpManager == null) ? Unit.Progression.Features.GetRank(CareerPath) : LevelUpManager.PreviewUnit.Progression.Features.GetRank(CareerPath));
		Progress.Value = (float)CurrentRank.Value / (float)MaxRank;
		bool flag = Unit.CanEditCareer();
		ReadOnly.Value = LevelUpManager == null || LevelUpManager.TargetUnit != Unit || !flag;
		CanUpgrade.Value = CanUpgradeCareer() && flag;
		CanCommit.Value = CanCommitChanges();
	}

	private void UpdateRanks()
	{
		RankEntries.ForEach(delegate(CareerPathRankEntryVM vm)
		{
			vm.UpdateState(LevelUpManager);
		});
		HasNewValidSelections.Value = AvailableSelections.Any((RankEntrySelectionVM i) => i.SelectionMade);
		int num = CareerPath.RankEntries.SelectMany((BlueprintPath.RankEntry i) => i.Features).Count();
		int num2 = Unit.Progression.GetSelectionsByPath(CareerPath).Count();
		CareerUpgrades.Value = num + num2;
		CurrentProgress.Value = AvailableSelections.FirstOrDefault((RankEntrySelectionVM s) => !s.SelectionMade)?.Rank ?? CurrentProgress.Value;
		IRankEntrySelectItem currentEntry = UnitProgressionVM?.CurrentRankEntryItem.Value;
		if (currentEntry != null)
		{
			if ((currentEntry is RankEntryFeatureItemVM && !FeaturesToVisit.Any((IRankEntrySelectItem f) => f is RankEntrySelectionVM { SelectionMade: false } && f.EntryRank < currentEntry.EntryRank)) || currentEntry is RankEntrySelectionVM { SelectionMade: not false } || currentEntry == FirstSelectable)
			{
				FeaturesToVisit.RemoveAll((IRankEntrySelectItem f) => (f is RankEntryFeatureItemVM && FeaturesToVisit.IndexOf(f) <= FeaturesToVisit.IndexOf(UnitProgressionVM.CurrentRankEntryItem.Value)) || (f is RankEntrySelectionVM rankEntrySelectionVM2 && rankEntrySelectionVM2.SelectionMade));
				AllVisited.Value = FeaturesToVisit.Count == 0;
			}
			PointerItem.Value = FeaturesToVisit.FirstOrDefault();
		}
		UpdateRecommendation();
		HasSelectionsToUpgrade = AvailableSelections.Any((RankEntrySelectionVM s) => s.NeedToSelect && !s.SelectionMade);
	}

	private void UpdateIcon()
	{
		Sprite value = CareerPath.Icon;
		if (IsInProgress)
		{
			value = CareerPath.InProgressIcon;
		}
		else if (IsFinished)
		{
			value = CareerPath.FinishedIcon;
		}
		else if (!IsAvailableToUpgrade)
		{
			value = CareerPath.NotAvailableIcon;
		}
		Icon.Value = value;
	}

	private void UpdateRecommendation()
	{
		if (CareerPathUIMetaData == null)
		{
			return;
		}
		BlueprintOriginPath unitOriginPath = CharGenUtility.GetUnitOriginPath(Unit);
		if (unitOriginPath != null)
		{
			BlueprintSelectionFeature blueprintSelectionFeature = CharGenUtility.GetFeatureSelectionsByGroup(unitOriginPath, FeatureGroup.ChargenOccupation, Unit).FirstOrDefault();
			BlueprintFeature blueprintFeature = ((blueprintSelectionFeature == null) ? null : Unit.Progression.GetSelectedFeature(unitOriginPath, 0, blueprintSelectionFeature)?.Feature);
			if (blueprintFeature != null)
			{
				IsRecommended.Value = CareerPathUIMetaData.RecommendedByOccupations.Contains(blueprintFeature);
			}
		}
	}

	private IEnumerable<T> GetStatAdvancements<T>() where T : BlueprintStatAdvancement
	{
		return (from selectionItem in CareerPath.RankEntries.SelectMany((BlueprintPath.RankEntry rankEntry) => rankEntry.Selections).Cast<BlueprintSelectionFeature>().SelectMany(delegate(BlueprintSelectionFeature selectionFeature)
			{
				if (selectionFeature != null)
				{
					return selectionFeature.GetSelectionItems(Unit, CareerPath);
				}
				throw new ArgumentNullException(CareerPath.name, "Selections contains NULL");
			})
			select selectionItem.Feature into blueprintFeature
			where blueprintFeature is T
			select blueprintFeature).Cast<T>().Distinct();
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
		if (UnitProgressionVM?.CurrentCareer.Value == this)
		{
			RefreshTooltipUnit();
		}
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
		UpdateState(updateRanks: true);
	}

	public void HandleUICommitChanges()
	{
		UpdateState(updateRanks: true);
	}

	public void HandleUISelectionChanged()
	{
		if (UnitProgressionVM?.CurrentCareer.Value == this)
		{
			UpdateState(updateRanks: true);
			RefreshTooltipUnit();
			UpdateSelectedItemInfoSection(UnitProgressionVM?.CurrentRankEntryItem.Value);
		}
	}

	public void RefreshTooltipUnit()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			using (ContextData<UnitHelper.DoNotCreateItems>.Request())
			{
				using (ContextData<UnitHelper.PreviewUnit>.Request())
				{
					TooltipsPreviewUnit.Value?.Dispose();
					using (ContextData<ItemSlot.IgnoreLock>.Request())
					{
						using (ContextData<GameCommandHelper.PreviewItem>.Request())
						{
							TooltipsPreviewUnit.Value = ((LevelUpManager?.PreviewUnit != null) ? LevelUpManager.PreviewUnit.CreatePreview(createView: false) : Unit.CreatePreview(createView: false));
						}
					}
				}
			}
		}
	}

	public void HandleUnitGainExperience(int gained, bool withSound = false)
	{
	}

	private void UpdateTabSectionTooltip(TooltipBaseTemplate template)
	{
		TabInfoSectionVM.SetTemplate(template);
	}

	private void UpdateSelectedItemInfoSection(IHasTooltipTemplates tooltipTemplates)
	{
		UpdateSelectedItemInfoSection(tooltipTemplates?.TooltipTemplates());
	}

	private void UpdateSelectedItemInfoSection(List<TooltipBaseTemplate> templates)
	{
		if (templates != null)
		{
			TooltipBaseTemplate tooltipBaseTemplate = templates.LastOrDefault((TooltipBaseTemplate t) => t != null);
			if (tooltipBaseTemplate != null)
			{
				SelectedItemInfoSectionVM.SetTemplate(tooltipBaseTemplate);
			}
		}
		else
		{
			UpdateCareerTooltip();
		}
	}

	private void UpdateCareerTooltip()
	{
		UpdateTabSectionTooltip(IsInLevelupProcess ? CareerProgressionTooltip : CareerTooltip);
		SelectedItemInfoSectionVM.SetTemplate((!IsInLevelupProcess) ? CareerProgressionTooltip : CareerProgressionDesc);
	}

	public void SwitchDescriptionShowed(bool? state = null)
	{
		if (!state.HasValue)
		{
			IsDescriptionShowed.Value = !IsDescriptionShowed.Value;
		}
		else
		{
			IsDescriptionShowed.Value = state.Value;
		}
	}

	public void SetFocusOn(BaseRankEntryFeatureVM featureVM)
	{
		if (UnitProgressionVM?.CurrentCareer.Value == this)
		{
			if (UnitProgressionVM.CurrentRankEntryItem != null && UnitProgressionVM != null && UnitProgressionVM.CurrentRankEntryItem != null && UnitProgressionVM.CurrentRankEntryItem.Value.GetFeatureGroup().Value == FeatureGroup.PetKeystone && featureVM != null)
			{
				List<TooltipBaseTemplate> templates = new List<TooltipBaseTemplate>
				{
					new TooltipTemplatePetKeystone(featureVM as RankEntrySelectionFeatureVM, Unit)
				};
				UpdateSelectedItemInfoSection(templates);
			}
			else
			{
				UpdateSelectedItemInfoSection((featureVM != null) ? new List<TooltipBaseTemplate> { featureVM.TooltipTemplate() } : UnitProgressionVM?.CurrentRankEntryItem.Value?.TooltipTemplates());
			}
		}
	}

	public void SetTooltip(TooltipBaseTemplate template)
	{
		UpdateSelectedItemInfoSection(new List<TooltipBaseTemplate> { template });
	}

	public bool HasPrerequisiteFeatures()
	{
		foreach (BlueprintFeature prerequisiteFeature in PrerequisiteFeatures)
		{
			if (Unit.Facts.Contains(prerequisiteFeature))
			{
				return true;
			}
		}
		return false;
	}

	public bool SelectionHasPrerequisiteFeatures()
	{
		if (LevelUpManager == null)
		{
			return false;
		}
		if (LevelUpManager.Selections.FirstOrDefault((SelectionState x) => x is SelectionStateFeature selectionStateFeature2 && selectionStateFeature2.Blueprint.Group == FeatureGroup.ChargenOccupation) is SelectionStateFeature { SelectionItem: not null } selectionStateFeature)
		{
			return PrerequisiteFeatures.Contains(selectionStateFeature.SelectionItem.Value.Feature);
		}
		return false;
	}
}
