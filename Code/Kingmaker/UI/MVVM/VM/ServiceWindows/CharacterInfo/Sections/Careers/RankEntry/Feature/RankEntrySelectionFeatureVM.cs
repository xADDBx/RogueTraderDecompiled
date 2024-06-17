using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;

public class RankEntrySelectionFeatureVM : BaseRankEntryFeatureVM, IVirtualListElementIdentifier
{
	protected readonly IReadOnlyReactiveProperty<SelectionStateFeature> SelectionState;

	protected readonly FeatureSelectionItem SelectionItem;

	private readonly Action<FeatureSelectionItem?> m_SelectFeatureSelectionAction;

	protected readonly RankEntrySelectionVM Owner;

	private bool m_IsSelected;

	private bool m_UnitHasFeature;

	private static List<BlueprintFact> s_UnitFacts = new List<BlueprintFact>();

	public int VirtualListTypeId
	{
		get
		{
			if (Owner.FeatureGroup != FeatureGroup.UltimateAbility)
			{
				return 0;
			}
			return 1;
		}
	}

	public bool IsCommonFeature => RankEntryUtils.IsCommonSelectionItem(SelectionItem);

	public bool CanSelectItem => (SelectionState?.Value?.CanSelect(SelectionItem)).GetValueOrDefault();

	public bool UnitHasFeature => m_UnitHasFeature;

	public RankEntrySelectionFeatureVM(RankEntrySelectionVM owner, CareerPathVM careerPathVM, FeatureSelectionItem featureSelectionItem, IReadOnlyReactiveProperty<SelectionStateFeature> selectionState, Action<FeatureSelectionItem?> selectFeature)
		: base(careerPathVM, new UIFeature(featureSelectionItem.Feature))
	{
		Owner = owner;
		SelectionItem = featureSelectionItem;
		SelectionState = selectionState;
		m_SelectFeatureSelectionAction = selectFeature;
		OverrideTooltip();
		AddDisposable(SelectionState.Subscribe(delegate
		{
			UpdateFeatureState();
		}));
		AddDisposable(owner.EntryState.Subscribe(delegate
		{
			UpdateFeatureState();
		}));
	}

	public override void Select()
	{
		if (!m_IsSelected)
		{
			m_SelectFeatureSelectionAction?.Invoke(SelectionItem);
		}
		UpdateUnitFacts(base.UnitProgressionVM?.LevelUpManager?.PreviewUnit ?? base.UnitProgressionVM?.Unit.Value);
	}

	public override bool CanSelect()
	{
		if (SelectionState.Value != null)
		{
			return SelectionState.Value.CanSelect(SelectionItem);
		}
		return false;
	}

	public void SetSelectedAndUpdate(bool isSelected)
	{
		m_IsSelected = isSelected;
		UpdateFeatureState();
	}

	protected override void UpdateFeatureState()
	{
		UpdateUnitHasFeature();
		if (m_IsSelected)
		{
			if (Owner.EntryState.Value == RankEntryState.NotValid)
			{
				FeatureState.Value = RankFeatureState.NotValid;
				return;
			}
			(int, int) currentLevelupRange = CareerPathVM.GetCurrentLevelupRange();
			if (Owner.Rank >= currentLevelupRange.Item1 && Owner.Rank <= currentLevelupRange.Item2)
			{
				FeatureState.Value = RankFeatureState.Selected;
			}
			else
			{
				FeatureState.Value = RankFeatureState.Committed;
			}
		}
		else if ((!Owner.IsFirstSelectable && Owner.SelectedFeature.Value == null) || Owner.CareerPathVM.ReadOnly.Value)
		{
			FeatureState.Value = StateForReadOnly();
		}
		else if (SelectionState.Value != null)
		{
			bool flag = SelectionState.Value.CanSelect(SelectionItem);
			FeatureState.Value = ((!flag) ? RankFeatureState.NotSelectable : RankFeatureState.Selectable);
		}
	}

	public virtual RankFeatureState StateForReadOnly()
	{
		if (!m_UnitHasFeature)
		{
			return RankFeatureState.NotActive;
		}
		return RankFeatureState.NotSelectable;
	}

	private void UpdateUnitHasFeature()
	{
		m_UnitHasFeature = s_UnitFacts.Contains(base.Feature);
	}

	public static void UpdateUnitFacts(BaseUnitEntity unit)
	{
		if (unit != null)
		{
			s_UnitFacts = unit.Facts.List.Select((EntityFact f) => f.Blueprint).ToList();
		}
	}

	private void OverrideTooltip()
	{
		BlueprintAbility abilityFromFeature = RankEntryUtils.GetAbilityFromFeature(UIFeature.Feature);
		base.Tooltip.Value = ((abilityFromFeature != null) ? ((TooltipBaseTemplate)new TooltipTemplateRankEntryAbility(abilityFromFeature, SelectionItem, SelectionState, CareerPathVM.Unit)) : ((TooltipBaseTemplate)new TooltipTemplateRankEntryFeature(UIFeature, SelectionItem, SelectionState, CareerPathVM.Unit)));
	}
}
