using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.UnitLogic.Progression.Prerequisites;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;

public class RankEntrySelectionVM : VirtualListElementVMBase, IRankEntrySelectItem, IHasTooltipTemplates, IRankEntryFocusHandler, ISubscriber
{
	public static string SelectableHighlighterKey = "SelectableHighlighterKey";

	public readonly int Rank;

	public readonly FeatureGroup FeatureGroup;

	public readonly ReactiveProperty<RankEntryState> EntryState = new ReactiveProperty<RankEntryState>(RankEntryState.NotSelectable);

	public readonly ReactiveProperty<RankEntrySelectionFeatureVM> SelectedFeature = new ReactiveProperty<RankEntrySelectionFeatureVM>();

	public readonly ReactiveProperty<bool> IsCurrentRankEntryItem = new ReactiveProperty<bool>();

	public FeaturesFilterVM FeaturesFilterVM;

	public List<VirtualListElementVMBase> FilteredGroupList = new List<VirtualListElementVMBase>();

	public readonly ReactiveCommand OnFilterChange = new ReactiveCommand();

	private TooltipBaseTemplate m_HintTooltip;

	private TooltipBaseTemplate m_Tooltip;

	public readonly string GlossaryEntryKey;

	private readonly CareerPathVM m_CareerPathVM;

	private readonly BlueprintSelectionFeature m_SelectionFeature;

	private readonly Action<IRankEntrySelectItem> m_SelectAction;

	private List<RankEntryFeatureGroupVM> m_ShowGroupList;

	private readonly ReactiveProperty<SelectionStateFeature> m_SelectionStateFeature = new ReactiveProperty<SelectionStateFeature>();

	private readonly List<FeatureGroup> m_AscensionGroups = new List<FeatureGroup>
	{
		FeatureGroup.FirstCareerAbility,
		FeatureGroup.FirstCareerTalent,
		FeatureGroup.SecondCareerAbility,
		FeatureGroup.SecondCareerTalent,
		FeatureGroup.FirstOrSecondCareerAbility,
		FeatureGroup.FirstOrSecondCareerTalent
	};

	public readonly ReactiveProperty<FeatureGroupingMode> GroupingMode = new ReactiveProperty<FeatureGroupingMode>(FeatureGroupingMode.BySource);

	private readonly Dictionary<FeaturesFilter.FeatureFilterType, bool> m_GroupExpansionState = new Dictionary<FeaturesFilter.FeatureFilterType, bool>();

	private readonly Dictionary<string, bool> m_SourceGroupExpansionState = new Dictionary<string, bool>();

	private static readonly FeaturesFilter.FeatureFilterType[] DropdownGroupOrder = new FeaturesFilter.FeatureFilterType[7]
	{
		FeaturesFilter.FeatureFilterType.ArchetypeFilter,
		FeaturesFilter.FeatureFilterType.OriginFilter,
		FeaturesFilter.FeatureFilterType.WarpFilter,
		FeaturesFilter.FeatureFilterType.OffenseFilter,
		FeaturesFilter.FeatureFilterType.DefenseFilter,
		FeaturesFilter.FeatureFilterType.SupportFilter,
		FeaturesFilter.FeatureFilterType.UniversalFilter
	};

	public CharInfoFeatureVM UltimateFeature { get; }

	public TooltipBaseTemplate HintTooltip => m_HintTooltip ?? (m_HintTooltip = new TooltipTemplateGlossary(GlossaryEntryKey));

	public TooltipBaseTemplate Tooltip
	{
		get
		{
			if (SelectedFeature.Value != null)
			{
				return SelectedFeature.Value?.TooltipTemplate();
			}
			return m_Tooltip;
		}
	}

	public bool IsValidSelection
	{
		get
		{
			if (SelectedFeature.Value != null)
			{
				if (SelectedFeature.Value != null)
				{
					return EntryState.Value != RankEntryState.NotValid;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsFirstSelectable => m_CareerPathVM.FirstSelectable == this;

	public bool NeedToSelect
	{
		get
		{
			if (SelectedFeature.Value == null)
			{
				RankEntryState value = EntryState.Value;
				return value == RankEntryState.Selectable || value == RankEntryState.FirstSelectable || value == RankEntryState.WaitPreviousToSelect || value == RankEntryState.Selected || value == RankEntryState.NotValid;
			}
			return false;
		}
	}

	public bool SelectionMade => SelectedFeature.Value != null;

	public bool SelectionMadeAndValid
	{
		get
		{
			if (IsValidSelection)
			{
				return !NeedToSelect;
			}
			return false;
		}
	}

	public bool HasFeatures => m_ShowGroupList?.SelectMany((RankEntryFeatureGroupVM g) => g.FeatureList).Any() ?? false;

	public bool CanChangeSelection
	{
		get
		{
			RankEntryState value = EntryState.Value;
			return value == RankEntryState.Selectable || value == RankEntryState.FirstSelectable || value == RankEntryState.NotValid || value == RankEntryState.WaitPreviousToSelect;
		}
	}

	public BaseUnitProgressionVM UnitProgressionVM => m_CareerPathVM.UnitProgressionVM;

	public CareerPathVM CareerPathVM => m_CareerPathVM;

	public bool IsShipContext
	{
		get
		{
			if (FeatureGroup != FeatureGroup.ShipUpgrade)
			{
				CareerPathVM careerPathVM = CareerPathVM;
				if (careerPathVM != null && careerPathVM.Unit != null)
				{
					return CareerPathVM.Unit.IsPlayerShip();
				}
				return false;
			}
			return true;
		}
	}

	public int EntryRank => Rank;

	public BoolReactiveProperty HasUnavailableFeatures { get; } = new BoolReactiveProperty();


	public RankEntrySelectionVM(int rank, CareerPathVM careerPathVM, BlueprintSelectionFeature selectionFeature, Action<IRankEntrySelectItem> selectAction)
	{
		Rank = rank;
		FeatureGroup = selectionFeature.Group;
		m_CareerPathVM = careerPathVM;
		m_SelectAction = selectAction;
		m_SelectionFeature = selectionFeature;
		if (FeatureGroup == FeatureGroup.UltimateUpgradeAbility && careerPathVM.CareerPathUIMetaData != null && careerPathVM.CareerPathUIMetaData.UltimateFeatures.NotNull().Any())
		{
			BlueprintFeature correctUltimateFeature = GetCorrectUltimateFeature(careerPathVM);
			AddDisposable(UltimateFeature = new CharInfoFeatureVM(new UIFeature(correctUltimateFeature ?? careerPathVM.CareerPathUIMetaData.UltimateFeatures.FirstOrDefault()), careerPathVM.Unit));
		}
		if (RankEntryUtils.HasFilter(FeatureGroup))
		{
			FeaturesFilterVM = new FeaturesFilterVM();
			FeaturesFilterVM.CurrentFilter.Subscribe(delegate
			{
				HandleFilterChange();
			});
			if (!IsShipContext)
			{
				GroupingMode.Subscribe(delegate
				{
					HandleFilterChange();
				});
			}
		}
		else
		{
			UpdateFeatures();
		}
		GlossaryEntryKey = $"{((FeatureGroup != FeatureGroup.PetUltimateAbility) ? FeatureGroup : FeatureGroup.UltimateAbility)}_CareerPath_Selection";
		OverrideTooltip();
		AddDisposable(UnitProgressionVM.CurrentRankEntryItem.Subscribe(delegate(IRankEntrySelectItem item)
		{
			IsCurrentRankEntryItem.Value = item == this;
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	private void OverrideTooltip()
	{
		if (FeatureGroup == FeatureGroup.PetKeystone)
		{
			m_Tooltip = new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.KeystoneFeaturesHeader.Text, UIStrings.Instance.CharacterSheet.KeystoneFeaturesChargenDescription.Text);
		}
		if (FeatureGroup == FeatureGroup.PetUltimateAbility)
		{
			m_Tooltip = new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.UltimateUpgradeAbilityFeatureGroupHint.Text, UIStrings.Instance.CharacterSheet.UltimateAbilitiesChargenDescription.Text);
		}
	}

	private List<RankEntryFeatureGroupVM> CreateGroups()
	{
		List<RankEntryFeatureGroupVM> list = new List<RankEntryFeatureGroupVM>();
		foreach (var item3 in CreateSelectionItems(m_CareerPathVM))
		{
			BlueprintScriptableObject item = item3.Item1;
			List<RankEntrySelectionFeatureVM> item2 = item3.Item2;
			BlueprintFeature owner = (BlueprintFeature)item;
			List<BaseRankEntryFeatureVM> list2 = SortSelectionFeatures(item2);
			list2.ForEach(delegate(BaseRankEntryFeatureVM e)
			{
				e.SetHasFavorites(RankEntryUtils.HasFilter(FeatureGroup));
			});
			list2.ForEach(delegate(BaseRankEntryFeatureVM e)
			{
				e.OnFavoritesStateChanged = delegate
				{
					HandleFavoriteStateChanged(e);
				};
			});
			list.Add(new RankEntryFeatureGroupVM(list2, owner));
		}
		TryAddEmptyAscensionGroup(list);
		return list;
	}

	private static List<BaseRankEntryFeatureVM> SortSelectionFeatures(List<RankEntrySelectionFeatureVM> features)
	{
		Dictionary<RankEntrySelectionFeatureVM, BlueprintFeature> entryToFirstPrerequisiteFact = new Dictionary<RankEntrySelectionFeatureVM, BlueprintFeature>();
		foreach (RankEntrySelectionFeatureVM feature in features)
		{
			Prerequisite prerequisite = feature.Feature.Prerequisites.List.FirstOrDefault((Prerequisite p) => p is PrerequisiteFact prerequisiteFact && prerequisiteFact.Fact is BlueprintFeature);
			if (prerequisite != null && !entryToFirstPrerequisiteFact.ContainsKey(feature))
			{
				entryToFirstPrerequisiteFact.Add(feature, (prerequisite as PrerequisiteFact)?.Fact as BlueprintFeature);
			}
		}
		Dictionary<RankEntrySelectionFeatureVM, string> overrideNames = entryToFirstPrerequisiteFact.Select(delegate(KeyValuePair<RankEntrySelectionFeatureVM, BlueprintFeature> f)
		{
			int num = 0;
			RankEntrySelectionFeatureVM featureToCheck = entryToFirstPrerequisiteFact.Keys.FirstOrDefault((RankEntrySelectionFeatureVM kvp) => kvp.Feature == f.Value);
			string arg = featureToCheck?.DisplayName ?? f.Key.DisplayName;
			while (featureToCheck != null)
			{
				num++;
				featureToCheck = entryToFirstPrerequisiteFact.Keys.FirstOrDefault((RankEntrySelectionFeatureVM kvp) => kvp.Feature == entryToFirstPrerequisiteFact[featureToCheck]);
				if (featureToCheck != null)
				{
					arg = featureToCheck.DisplayName;
				}
			}
			return (Key: f.Key, $"{arg}_{num}");
		}).ToDictionary(((RankEntrySelectionFeatureVM Key, string) t) => t.Key, ((RankEntrySelectionFeatureVM Key, string) t) => t.Item2);
		return features.OrderBy((RankEntrySelectionFeatureVM f) => (!f.IsFavorite) ? (f.IsRecommended ? 1 : 2) : 0).ThenBy(NameSort).Cast<BaseRankEntryFeatureVM>()
			.ToList();
		string NameSort(RankEntrySelectionFeatureVM rankEntry)
		{
			if (!overrideNames.TryGetValue(rankEntry, out var value))
			{
				return rankEntry.DisplayName;
			}
			return value;
		}
	}

	private List<(BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>)> CreateSelectionItems(CareerPathVM careerPathVM)
	{
		BaseUnitEntity baseUnitEntity = careerPathVM.Unit;
		if (careerPathVM.IsInLevelupProcess)
		{
			baseUnitEntity = UnitProgressionVM.LevelUpManager?.PreviewUnit ?? careerPathVM.Unit;
		}
		List<FeatureSelectionItem> list = m_SelectionFeature.GetSelectionItems(baseUnitEntity, careerPathVM.CareerPath).ToList();
		int entryId = m_CareerPathVM.RankEntriesScan.IndexOf(this);
		m_CareerPathVM.AddedOnLevelUpFeatures?.ExcludeUnavailableFeatures(FeatureGroup, entryId, list);
		List<BlueprintFact> source = (from f in baseUnitEntity.Facts.List
			where f.FirstSource?.Blueprint is BlueprintCareerPath blueprintCareerPath && blueprintCareerPath.Tier > careerPathVM.CareerPath.Tier
			select f.Blueprint).ToList();
		Dictionary<BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>> dictionary = new Dictionary<BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>>();
		List<BlueprintFeature> list2 = new List<BlueprintFeature>();
		RankEntrySelectionFeatureVM.UpdateUnitFacts(baseUnitEntity);
		List<RankEntrySelectionFeatureVM> value2;
		foreach (FeatureSelectionItem item4 in list)
		{
			if (!dictionary.TryGetValue(item4.SourceBlueprint, out var value))
			{
				value2 = (dictionary[item4.SourceBlueprint] = new List<RankEntrySelectionFeatureVM>());
				value = value2;
			}
			if (!list2.Contains(item4.Feature) && !source.Contains(item4.SourceBlueprint))
			{
				RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM = ((!(item4.Feature is BlueprintStatAdvancement)) ? new RankEntrySelectionFeatureVM(this, careerPathVM, item4, m_SelectionStateFeature, SelectFeature) : new RankEntrySelectionStatVM(this, careerPathVM, item4, m_SelectionStateFeature, SelectFeature));
				RankEntrySelectionFeatureVM item = rankEntrySelectionFeatureVM;
				list2.Add(item4.Feature);
				value.Add(item);
			}
		}
		List<(BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>)> list4 = new List<(BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>)>();
		foreach (KeyValuePair<BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>> item5 in dictionary)
		{
			item5.Deconstruct(out var key, out value2);
			BlueprintScriptableObject item2 = key;
			List<RankEntrySelectionFeatureVM> item3 = value2;
			list4.Add((item2, item3));
		}
		return list4.OrderBy(delegate((BlueprintScriptableObject, List<RankEntrySelectionFeatureVM>) i)
		{
			if (!(i.Item1 is BlueprintFeature blueprintFeature))
			{
				return -1;
			}
			return blueprintFeature.HideInUI ? 1 : (-1);
		}).ToList();
	}

	protected override void DisposeImplementation()
	{
		m_ShowGroupList?.ForEach(delegate(RankEntryFeatureGroupVM vm)
		{
			vm.Dispose();
		});
		m_ShowGroupList?.Clear();
	}

	private void TryAddEmptyAscensionGroup(List<RankEntryFeatureGroupVM> rankEntryFeatureGroupVms)
	{
		if (rankEntryFeatureGroupVms.Empty() && m_AscensionGroups.Contains(FeatureGroup))
		{
			string featureGroupDescription = UIStrings.Instance.CharacterSheet.GetFeatureGroupDescription(FeatureGroup);
			rankEntryFeatureGroupVms.Add(new RankEntryEmptyFeaturesGroupVM(featureGroupDescription));
		}
		else if (rankEntryFeatureGroupVms.Count == 1)
		{
			switch (FeatureGroup)
			{
			case FeatureGroup.FirstOrSecondCareerAbility:
			{
				string description2 = UIStrings.Instance.CharacterSheet.AscensionMissingOnlySecondCareerAbilityFeatureGroupDescription;
				rankEntryFeatureGroupVms.Add(new RankEntryEmptyFeaturesGroupVM(description2));
				break;
			}
			case FeatureGroup.FirstOrSecondCareerTalent:
			{
				string description = UIStrings.Instance.CharacterSheet.AscensionMissingOnlySecondCareerTalentFeatureGroupDescription;
				rankEntryFeatureGroupVms.Add(new RankEntryEmptyFeaturesGroupVM(description));
				break;
			}
			}
		}
	}

	public void UpdateState(LevelUpManager levelUpManager)
	{
		m_ShowGroupList?.ForEach(delegate(RankEntryFeatureGroupVM vm)
		{
			vm.UpdateState(levelUpManager);
		});
		m_SelectionStateFeature.Value = (SelectionStateFeature)(levelUpManager?.GetSelectionState(m_CareerPathVM.CareerPath, m_SelectionFeature, Rank));
		(BlueprintFeature, int)? selectedFeature = m_CareerPathVM.Unit.Progression.GetSelectedFeature(m_CareerPathVM.CareerPath, Rank, m_SelectionFeature);
		if (selectedFeature.HasValue)
		{
			EntryState.Value = RankEntryState.Committed;
			SetSelectedFeature(selectedFeature.Value.Item1);
		}
		else if (m_SelectionStateFeature.Value != null)
		{
			if (!m_SelectionStateFeature.Value.IsValid)
			{
				EntryState.Value = RankEntryState.NotValid;
			}
			else if (m_SelectionStateFeature.Value.SelectionItem.HasValue)
			{
				EntryState.Value = RankEntryState.Selected;
			}
			else if (!m_SelectionStateFeature.Value.CanSelectAny)
			{
				EntryState.Value = RankEntryState.NotSelectable;
			}
			else if (m_CareerPathVM.FirstSelectable == this)
			{
				EntryState.Value = RankEntryState.FirstSelectable;
			}
			else if (Rank == m_CareerPathVM.FirstSelectable?.Rank)
			{
				EntryState.Value = RankEntryState.Selectable;
			}
			else
			{
				EntryState.Value = RankEntryState.WaitPreviousToSelect;
			}
			SetSelectedFeature(m_SelectionStateFeature.Value.SelectionItem);
		}
		else
		{
			EntryState.Value = RankEntryState.NotSelectable;
			SetSelectedFeature((FeatureSelectionItem?)null);
		}
	}

	public void HandleClick()
	{
		m_ShowGroupList = CreateGroups();
		m_SelectAction?.Invoke(this);
		HandleFilterChange();
	}

	public void ClearSelectedFeature()
	{
		m_SelectionStateFeature.Value?.ClearSelection();
		SetSelectedFeature((FeatureSelectionItem?)null);
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
	}

	private void SelectFeature(FeatureSelectionItem? featureSelectionItem)
	{
		if (!featureSelectionItem.HasValue)
		{
			ClearSelectedFeature();
			return;
		}
		SelectionStateFeature value = m_SelectionStateFeature.Value;
		if (value != null && value.Select(featureSelectionItem.Value))
		{
			SetSelectedFeature(m_SelectionStateFeature.Value.SelectionItem);
			int entryId = m_CareerPathVM.RankEntriesScan.IndexOf(this);
			m_CareerPathVM.AddedOnLevelUpFeatures?.RefreshSelectedFeatureAtRank(SelectedFeature.Value, entryId);
			EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
			{
				h.HandleUISelectionChanged();
			});
		}
	}

	private void SetSelectedFeature(FeatureSelectionItem? selectionItem)
	{
		BlueprintFeature owner = (BlueprintFeature)(selectionItem?.SourceBlueprint);
		SetSelectedFeature(selectionItem?.Feature, owner);
	}

	private void SetSelectedFeature(BlueprintFeature feature, BlueprintFeature owner = null)
	{
		SelectedFeature.Value?.SetSelectedAndUpdate(isSelected: false);
		SelectedFeature.Value = GetFeatureForSelection(feature, owner);
		SelectedFeature.Value?.SetSelectedAndUpdate(isSelected: true);
	}

	private RankEntrySelectionFeatureVM GetFeatureForSelection(BlueprintFeature feature, BlueprintFeature owner = null)
	{
		RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM = null;
		if (feature == null)
		{
			return null;
		}
		if (m_ShowGroupList != null)
		{
			foreach (RankEntryFeatureGroupVM item in m_ShowGroupList.Where((RankEntryFeatureGroupVM i) => owner == null || i.Owner == owner))
			{
				rankEntrySelectionFeatureVM = Enumerable.FirstOrDefault(item.FeatureList, (BaseRankEntryFeatureVM vm) => vm.Feature == feature) as RankEntrySelectionFeatureVM;
				if (rankEntrySelectionFeatureVM != null)
				{
					break;
				}
			}
		}
		else
		{
			FeatureSelectionItem featureSelectionItem = m_SelectionFeature.GetSelectionItems(m_CareerPathVM.Unit, m_CareerPathVM.CareerPath).FirstOrDefault((FeatureSelectionItem i) => i.Feature == feature);
			if (featureSelectionItem.Feature == null)
			{
				return null;
			}
			RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM2 = ((!(featureSelectionItem.Feature is BlueprintStatAdvancement)) ? new RankEntrySelectionFeatureVM(this, m_CareerPathVM, featureSelectionItem, m_SelectionStateFeature, SelectFeature) : new RankEntrySelectionStatVM(this, m_CareerPathVM, featureSelectionItem, m_SelectionStateFeature, SelectFeature));
			rankEntrySelectionFeatureVM = rankEntrySelectionFeatureVM2;
			AddDisposable(rankEntrySelectionFeatureVM);
		}
		return rankEntrySelectionFeatureVM;
	}

	private void HandleFavoriteStateChanged(BaseRankEntryFeatureVM featureVM)
	{
		if (IsShipContext)
		{
			HandleFilterChange();
			return;
		}
		AvailableTalentsDropDownVM availableTalentsDropDownVM = FindDropdownHeaderForFeature(featureVM);
		if (availableTalentsDropDownVM == null || !availableTalentsDropDownVM.IsExpanded.Value)
		{
			HandleFilterChange();
		}
	}

	private AvailableTalentsDropDownVM FindDropdownHeaderForFeature(BaseRankEntryFeatureVM featureVM)
	{
		return FilteredGroupList.OfType<AvailableTalentsDropDownVM>().FirstOrDefault((AvailableTalentsDropDownVM h) => h.GroupedFeatures?.Contains(featureVM) ?? false);
	}

	private void HandleFilterChange()
	{
		if (m_ShowGroupList == null)
		{
			m_ShowGroupList = CreateGroups();
		}
		if (IsShipContext)
		{
			RunLegacyFilterPipeline(FeaturesFilterVM?.CurrentFilter.Value);
			return;
		}
		bool flag = false;
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		foreach (RankEntryFeatureGroupVM showGroup in m_ShowGroupList)
		{
			List<VirtualListElementVMBase> all = showGroup.GetAll();
			flag |= all.Any(delegate(VirtualListElementVMBase f)
			{
				BaseRankEntryFeatureVM obj2 = f as BaseRankEntryFeatureVM;
				return obj2 != null && obj2.FeatureState.Value == RankFeatureState.NotSelectable;
			});
			if (!Game.Instance.Player.UISettings.ShowUnavailableFeatures)
			{
				all.RemoveAll(delegate(VirtualListElementVMBase f)
				{
					BaseRankEntryFeatureVM obj = f as BaseRankEntryFeatureVM;
					return obj != null && obj.FeatureState.Value == RankFeatureState.NotSelectable;
				});
			}
			all.RemoveAll((VirtualListElementVMBase f) => f is RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM && rankEntrySelectionFeatureVM.FeatureState.Value == RankFeatureState.NotSelectable && rankEntrySelectionFeatureVM.Feature.HideNotAvailibleInUI && rankEntrySelectionFeatureVM.UnitCanTakeFeature);
			list.AddRange(all);
		}
		FilteredGroupList.Clear();
		if (RankEntryUtils.HasFilter(FeatureGroup))
		{
			if (GroupingMode.Value == FeatureGroupingMode.BySource)
			{
				GroupFeaturesBySource(list);
			}
			else
			{
				GroupFeaturesByFilterType(list);
			}
		}
		else
		{
			FilteredGroupList.AddRange(list);
		}
		OnFilterChange?.Execute();
		HasUnavailableFeatures.Value = flag;
	}

	private void RunLegacyFilterPipeline(FeaturesFilter.FeatureFilterType? filter)
	{
		if (filter == FeaturesFilter.FeatureFilterType.FavoritesFilter)
		{
			m_ShowGroupList = CreateGroups();
		}
		bool flag = false;
		bool flag2 = false;
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		List<VirtualListElementVMBase> list2 = new List<VirtualListElementVMBase>();
		FilteredGroupList.Clear();
		foreach (RankEntryFeatureGroupVM showGroup in m_ShowGroupList)
		{
			List<VirtualListElementVMBase> filtered = showGroup.GetFiltered(filter);
			flag2 |= filtered.Any(delegate(VirtualListElementVMBase f)
			{
				BaseRankEntryFeatureVM obj3 = f as BaseRankEntryFeatureVM;
				return obj3 != null && obj3.FeatureState.Value == RankFeatureState.NotSelectable;
			});
			filtered.RemoveAll((VirtualListElementVMBase f) => f is RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM3 && rankEntrySelectionFeatureVM3.FeatureState.Value == RankFeatureState.NotSelectable && rankEntrySelectionFeatureVM3.Feature.HideNotAvailibleInUI && rankEntrySelectionFeatureVM3.UnitCanTakeFeature);
			list.AddRange(filtered.Where((VirtualListElementVMBase f) => f is RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM2 && !rankEntrySelectionFeatureVM2.UnitCanTakeFeature));
			filtered.RemoveAll((VirtualListElementVMBase f) => f is RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM && !rankEntrySelectionFeatureVM.UnitCanTakeFeature);
			if (Game.Instance.Player.UISettings.ShowUnavailableFeatures)
			{
				list2.AddRange(filtered.Where(delegate(VirtualListElementVMBase f)
				{
					BaseRankEntryFeatureVM obj2 = f as BaseRankEntryFeatureVM;
					return obj2 != null && obj2.FeatureState.Value == RankFeatureState.NotSelectable;
				}));
			}
			filtered.RemoveAll(delegate(VirtualListElementVMBase f)
			{
				BaseRankEntryFeatureVM obj = f as BaseRankEntryFeatureVM;
				return obj != null && obj.FeatureState.Value == RankFeatureState.NotSelectable;
			});
			if (flag && filtered.Any())
			{
				FilteredGroupList.Add(AddDisposableAndReturn(new SeparatorElementVM()));
			}
			FilteredGroupList.AddRange(filtered);
			flag = filtered.Any();
		}
		if (list.Count > 0)
		{
			if (FilteredGroupList.Count > 0)
			{
				FilteredGroupList.Add(AddDisposableAndReturn(new SeparatorElementVM()));
			}
			FilteredGroupList.AddRange(list);
		}
		if (list2.Count > 0)
		{
			FilteredGroupList.AddRange(list2);
		}
		OnFilterChange?.Execute();
		HasUnavailableFeatures.Value = flag2;
	}

	private static int GetFeatureSortPriority(VirtualListElementVMBase item)
	{
		if (!(item is BaseRankEntryFeatureVM baseRankEntryFeatureVM))
		{
			return 0;
		}
		if (baseRankEntryFeatureVM.FeatureState.Value != RankFeatureState.NotSelectable)
		{
			return 0;
		}
		if (item is RankEntrySelectionFeatureVM { UnitCanTakeFeature: false })
		{
			return 1;
		}
		return 2;
	}

	private static int GetFavRecSortTier(VirtualListElementVMBase item)
	{
		if (!(item is BaseRankEntryFeatureVM baseRankEntryFeatureVM))
		{
			return 3;
		}
		if (baseRankEntryFeatureVM.IsFavorite)
		{
			return 0;
		}
		if (baseRankEntryFeatureVM.IsRecommended)
		{
			return 1;
		}
		return 2;
	}

	private static int CompareFeatureItems(VirtualListElementVMBase a, VirtualListElementVMBase b)
	{
		int num = GetFeatureSortPriority(a).CompareTo(GetFeatureSortPriority(b));
		if (num != 0)
		{
			return num;
		}
		int num2 = GetFavRecSortTier(a).CompareTo(GetFavRecSortTier(b));
		if (num2 != 0)
		{
			return num2;
		}
		string strA = (a as BaseRankEntryFeatureVM)?.DisplayName ?? string.Empty;
		string strB = (b as BaseRankEntryFeatureVM)?.DisplayName ?? string.Empty;
		return string.Compare(strA, strB, StringComparison.CurrentCulture);
	}

	private void GroupFeaturesByFilterType(List<VirtualListElementVMBase> flatFeatures)
	{
		Dictionary<FeaturesFilter.FeatureFilterType, List<VirtualListElementVMBase>> dictionary = new Dictionary<FeaturesFilter.FeatureFilterType, List<VirtualListElementVMBase>>();
		FeaturesFilter.FeatureFilterType[] dropdownGroupOrder = DropdownGroupOrder;
		foreach (FeaturesFilter.FeatureFilterType key in dropdownGroupOrder)
		{
			dictionary[key] = new List<VirtualListElementVMBase>();
		}
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		HashSet<VirtualListElementVMBase> hashSet = new HashSet<VirtualListElementVMBase>();
		dropdownGroupOrder = DropdownGroupOrder;
		foreach (FeaturesFilter.FeatureFilterType featureFilterType in dropdownGroupOrder)
		{
			foreach (VirtualListElementVMBase flatFeature in flatFeatures)
			{
				if (!hashSet.Contains(flatFeature) && flatFeature is BaseRankEntryFeatureVM baseRankEntryFeatureVM && baseRankEntryFeatureVM.Feature.MeetsFilter(featureFilterType))
				{
					dictionary[featureFilterType].Add(flatFeature);
					hashSet.Add(flatFeature);
				}
			}
		}
		foreach (VirtualListElementVMBase flatFeature2 in flatFeatures)
		{
			if (!hashSet.Contains(flatFeature2))
			{
				list.Add(flatFeature2);
			}
		}
		dropdownGroupOrder = DropdownGroupOrder;
		foreach (FeaturesFilter.FeatureFilterType featureFilterType2 in dropdownGroupOrder)
		{
			List<VirtualListElementVMBase> list2 = dictionary[featureFilterType2];
			if (list2.Count != 0)
			{
				list2.Sort(CompareFeatureItems);
				AddDropdownGroup(featureFilterType2, GetFilterTypeTitle(featureFilterType2), list2);
			}
		}
		if (list.Count > 0)
		{
			list.Sort((VirtualListElementVMBase a, VirtualListElementVMBase b) => GetFeatureSortPriority(a).CompareTo(GetFeatureSortPriority(b)));
			AddDropdownGroup(FeaturesFilter.FeatureFilterType.None, UIStrings.Instance.CharacterSheet.NoneHint, list);
		}
	}

	private void GroupFeaturesBySource(List<VirtualListElementVMBase> allFeatures)
	{
		HashSet<VirtualListElementVMBase> allFeaturesSet = new HashSet<VirtualListElementVMBase>(allFeatures);
		foreach (RankEntryFeatureGroupVM showGroup in m_ShowGroupList)
		{
			List<VirtualListElementVMBase> list = showGroup.FeatureList.Where((BaseRankEntryFeatureVM f) => allFeaturesSet.Contains(f)).Cast<VirtualListElementVMBase>().ToList();
			if (list.Count != 0)
			{
				list.Sort(CompareFeatureItems);
				string text = showGroup.Owner?.Name ?? ((string)UIStrings.Instance.CharacterSheet.NoneHint);
				string sourceKey = showGroup.Owner?.AssetGuid ?? "none";
				if (text == "")
				{
					text = UIStrings.Instance.CharGen.Other;
				}
				AddSourceDropdownGroup(sourceKey, text, list);
			}
		}
	}

	private void AddSourceDropdownGroup(string sourceKey, string title, List<VirtualListElementVMBase> features)
	{
		bool value2;
		bool flag = !m_SourceGroupExpansionState.TryGetValue(sourceKey, out value2) || value2;
		AvailableTalentsDropDownVM availableTalentsDropDownVM = AddDisposableAndReturn(new AvailableTalentsDropDownVM(FeaturesFilter.FeatureFilterType.None, title, flag));
		availableTalentsDropDownVM.FeatureCount = features.Count;
		availableTalentsDropDownVM.GroupedFeatures = features;
		availableTalentsDropDownVM.IsExpanded.Subscribe(delegate(bool value)
		{
			m_SourceGroupExpansionState[sourceKey] = value;
		});
		FilteredGroupList.Add(availableTalentsDropDownVM);
		foreach (VirtualListElementVMBase feature in features)
		{
			bool flag2 = feature is BaseRankEntryFeatureVM baseRankEntryFeatureVM && baseRankEntryFeatureVM.IsFavorite;
			feature.Active.Value = flag || flag2;
			FilteredGroupList.Add(feature);
		}
	}

	public void SetGroupingMode(FeatureGroupingMode mode)
	{
		GroupingMode.Value = mode;
	}

	private void AddDropdownGroup(FeaturesFilter.FeatureFilterType filterType, string title, List<VirtualListElementVMBase> features)
	{
		bool value2;
		bool flag = !m_GroupExpansionState.TryGetValue(filterType, out value2) || value2;
		AvailableTalentsDropDownVM availableTalentsDropDownVM = AddDisposableAndReturn(new AvailableTalentsDropDownVM(filterType, title, flag));
		availableTalentsDropDownVM.FeatureCount = features.Count;
		availableTalentsDropDownVM.GroupedFeatures = features;
		availableTalentsDropDownVM.IsExpanded.Subscribe(delegate(bool value)
		{
			m_GroupExpansionState[filterType] = value;
		});
		FilteredGroupList.Add(availableTalentsDropDownVM);
		foreach (VirtualListElementVMBase feature in features)
		{
			bool flag2 = feature is BaseRankEntryFeatureVM baseRankEntryFeatureVM && baseRankEntryFeatureVM.IsFavorite;
			feature.Active.Value = flag || flag2;
			FilteredGroupList.Add(feature);
		}
	}

	private static string GetFilterTypeTitle(FeaturesFilter.FeatureFilterType filterType)
	{
		UITextCharSheet characterSheet = UIStrings.Instance.CharacterSheet;
		return filterType switch
		{
			FeaturesFilter.FeatureFilterType.ArchetypeFilter => characterSheet.ArchetypeFilterHint, 
			FeaturesFilter.FeatureFilterType.OriginFilter => characterSheet.OriginFilterHint, 
			FeaturesFilter.FeatureFilterType.WarpFilter => characterSheet.WarpFilterHint, 
			FeaturesFilter.FeatureFilterType.OffenseFilter => characterSheet.OffenseFilterHint, 
			FeaturesFilter.FeatureFilterType.DefenseFilter => characterSheet.DefenseFilterHint, 
			FeaturesFilter.FeatureFilterType.SupportFilter => characterSheet.SupportFilterHint, 
			FeaturesFilter.FeatureFilterType.UniversalFilter => characterSheet.UniversalFilterHint, 
			_ => filterType.ToString(), 
		};
	}

	public string GetHintText()
	{
		return UIStrings.Instance.CharacterSheet.GetFeatureGroupHint(FeatureGroup, CanChangeSelection);
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return new List<TooltipBaseTemplate> { HintTooltip, Tooltip };
	}

	public FeatureGroup? GetFeatureGroup()
	{
		return FeatureGroup;
	}

	public bool CanSelect()
	{
		if (EntryState.Value != 0)
		{
			return EntryState.Value != RankEntryState.Committed;
		}
		return false;
	}

	public void UpdateFeatures()
	{
		m_ShowGroupList = CreateGroups();
		HandleFilterChange();
		SetSelectedFeature(SelectedFeature.Value?.Feature);
	}

	public void UpdateReadOnlyState()
	{
		m_ShowGroupList.ForEach(delegate(RankEntryFeatureGroupVM g)
		{
			g.UpdateReadOnlyState();
		});
	}

	public void SetFocusOn(BaseRankEntryFeatureVM featureVM)
	{
		m_ShowGroupList?.SelectMany((RankEntryFeatureGroupVM g) => g.FeatureList).ForEach(delegate(BaseRankEntryFeatureVM rankEntry)
		{
			rankEntry.SetFocusOn(featureVM);
		});
	}

	public void ToggleShowUnavailableFeatures()
	{
		PlayerUISettings uISettings = Game.Instance.Player.UISettings;
		uISettings.ShowUnavailableFeatures = !uISettings.ShowUnavailableFeatures;
		HandleFilterChange();
	}

	public bool ContainsFeature(string key)
	{
		List<RankEntryFeatureGroupVM> showGroupList = m_ShowGroupList;
		if (showGroupList == null)
		{
			return false;
		}
		return showGroupList.SelectMany((RankEntryFeatureGroupVM l) => l.FeatureList).FindIndex((BaseRankEntryFeatureVM f) => f.Feature.AssetGuid == key) >= 0;
	}

	private BlueprintFeature GetCorrectUltimateFeature(CareerPathVM careerPathVM)
	{
		UnitPartPetOwner optional = careerPathVM.Unit.GetOptional<UnitPartPetOwner>();
		if (optional == null)
		{
			return null;
		}
		return optional.PetType switch
		{
			PetType.Mastiff => careerPathVM.CareerPathUIMetaData.UltimateFeatures.FirstOrDefault((BlueprintFeature f) => f.NameForAcronym == "Master_Ultimate_Feature"), 
			PetType.Eagle => careerPathVM.CareerPathUIMetaData.UltimateFeatures.FirstOrDefault((BlueprintFeature f) => f.NameForAcronym == "Master_Ultimate_Eagle_Feature"), 
			PetType.Raven => careerPathVM.CareerPathUIMetaData.UltimateFeatures.FirstOrDefault((BlueprintFeature f) => f.NameForAcronym == "Master_Ultimate_Raven_Feature"), 
			PetType.ServoskullSwarm => careerPathVM.CareerPathUIMetaData.UltimateFeatures.FirstOrDefault((BlueprintFeature f) => f.NameForAcronym == "Master_Ultimate_Servoskull_Feature"), 
			_ => null, 
		};
	}
}
