using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Stats;

public class CharGenAttributesPhaseVM : CharGenBackgroundBasePhaseVM<CharGenAttributesItemVM>, ILevelUpManagerUIHandler, ISubscriber, ICharGenAttributesPhaseHandler
{
	public const int MaxRanksPerStat = 2;

	public CharInfoSkillsBlockVM CharInfoSkillsBlock;

	public readonly ReactiveProperty<int> AvailablePointsLeft = new ReactiveProperty<int>();

	public readonly ReactiveCommand OnUpdateState = new ReactiveCommand();

	private readonly List<SelectionStateFeature> m_SelectionStates = new List<SelectionStateFeature>();

	private readonly Dictionary<StatType, int> m_StatRanks = new Dictionary<StatType, int>();

	private int m_ValuePerRank;

	private readonly ReactiveProperty<BaseUnitEntity> m_PreviewUnit = new ReactiveProperty<BaseUnitEntity>();

	private IEnumerable<SelectionStateFeature> AvailableSelectionStates => m_SelectionStates.Where((SelectionStateFeature s) => !s.IsMade);

	public CharGenAttributesPhaseVM(CharGenContext charGenContext, ReactiveProperty<CharGenPhaseBaseVM> currentPhase)
		: base(charGenContext, FeatureGroup.ChargenAttribute, CharGenPhaseType.Attributes, currentPhase)
	{
		CanShowVisualSettings = false;
	}

	protected override void Clear()
	{
		base.Clear();
		m_SelectionStates.Clear();
		m_StatRanks.Clear();
	}

	protected override void OnBeginDetailedView()
	{
		if (!Subscribed)
		{
			UpdatePreviewUnit();
			CharInfoSkillsBlock = AddDisposableAndReturn(new CharInfoSkillsBlockVM(m_PreviewUnit, null));
			AddDisposable(CharInfoSkillsBlock.OnStatsUpdated.Subscribe(delegate
			{
				UpdateSkillsHighlight();
			}));
			AddDisposable(SelectedItem.Subscribe(delegate
			{
				UpdateSkillsHighlight();
			}));
			AddDisposable(EventBus.Subscribe(this));
		}
		if (!Subscribed)
		{
			AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
			Subscribed = true;
		}
		TrySelectItem();
		UpdateHint();
		UpdateRecommendedStats();
	}

	private void UpdateHint()
	{
		if (AvailablePointsLeft.Value == 0)
		{
			base.PhaseNextHint.Value = string.Empty;
		}
		else
		{
			base.PhaseNextHint.Value = UIStrings.Instance.CharGen.SpreadOutPointsHint.Text ?? "";
		}
	}

	private void UpdateRecommendedStats()
	{
		BaseUnitEntity unit = CharGenContext.LevelUpManager?.Value.PreviewUnit;
		List<StatType> selectedCareerRecommendedStats = CharGenUtility.GetSelectedCareerRecommendedStats<BlueprintAttributeAdvancement>(unit);
		List<StatType> selectedCareerRecommendedStats2 = CharGenUtility.GetSelectedCareerRecommendedStats<BlueprintSkillAdvancement>(unit);
		foreach (CharGenAttributesItemVM item in SelectionGroup.EntitiesCollection)
		{
			item.UpdateRecommendedMark(selectedCareerRecommendedStats);
		}
		CharInfoSkillsBlock.SetRecommendedMarks(selectedCareerRecommendedStats2);
	}

	protected override bool CheckIsCompleted()
	{
		if (m_SelectionStates.Any())
		{
			return m_SelectionStates.All((SelectionStateFeature s) => s.IsMade && s.IsValid);
		}
		return false;
	}

	protected override void HandleLevelUpManager(LevelUpManager manager)
	{
		Clear();
		if (manager == null)
		{
			return;
		}
		List<BlueprintSelectionFeature> list = CharGenUtility.GetFeatureSelectionsByGroup(manager.Path, FeatureGroup, manager.PreviewUnit).ToList();
		if (!list.Any())
		{
			return;
		}
		foreach (BlueprintSelectionFeature item2 in list)
		{
			if (manager.GetSelectionState(manager.Path, item2, 0) is SelectionStateFeature item)
			{
				m_SelectionStates.Add(item);
			}
		}
		IOrderedEnumerable<CharGenAttributesItemVM> orderedEnumerable = from i in list.First().GetSelectionItems(manager.PreviewUnit, manager.Path).Select(CreateItem)
			orderby CharInfoAbilityScoresBlockVM.AbilitiesOrdered.IndexOf(i.StatType)
			select i;
		foreach (CharGenAttributesItemVM item3 in orderedEnumerable)
		{
			Items.Add(item3);
			m_StatRanks[item3.StatType] = 0;
		}
		m_ValuePerRank = orderedEnumerable.First().ValuePerRank;
		SelectionGroup.TrySelectFirstValidEntity();
		UpdateState();
	}

	protected override CharGenAttributesItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return null;
	}

	private CharGenAttributesItemVM CreateItem(FeatureSelectionItem selectionItem)
	{
		return new CharGenAttributesItemVM(selectionItem, TryAdvanceStat, ShowTooltipForItem, PhaseType, CurrentPhase);
	}

	private void TryAdvanceStat(StatType statType, bool advance)
	{
		Game.Instance.GameCommandQueue.CharGenTryAdvanceStat(statType, advance);
	}

	void ICharGenAttributesPhaseHandler.HandleTryAdvanceStat(StatType statType, bool advance)
	{
		int num;
		if (advance)
		{
			SelectionStateFeature selectionStateFeature = AvailableSelectionStates.FirstOrDefault();
			if (selectionStateFeature == null)
			{
				return;
			}
			FeatureSelectionItem selectionItem = selectionStateFeature.Items.FirstOrDefault((FeatureSelectionItem i) => i.Feature is BlueprintStatAdvancement blueprintStatAdvancement2 && blueprintStatAdvancement2.Stat == statType);
			selectionStateFeature.Select(selectionItem);
			num = 1;
		}
		else
		{
			m_SelectionStates.FirstOrDefault(delegate(SelectionStateFeature state)
			{
				if (state.IsMade)
				{
					FeatureSelectionItem? selectionItem2 = state.SelectionItem;
					if (selectionItem2.HasValue && selectionItem2.GetValueOrDefault().Feature is BlueprintStatAdvancement blueprintStatAdvancement)
					{
						return blueprintStatAdvancement.Stat == statType;
					}
				}
				return false;
			})?.ClearSelection();
			num = -1;
		}
		m_StatRanks[statType] += num;
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
		UpdateState();
		UpdateHint();
	}

	private void UpdateState()
	{
		int num = AvailableSelectionStates.Count();
		AvailablePointsLeft.Value = num * m_ValuePerRank;
		LevelUpManager value = CharGenContext.LevelUpManager.Value;
		if (value == null)
		{
			return;
		}
		foreach (CharGenAttributesItemVM item in Items)
		{
			ModifiableValue stat = value.TargetUnit.Stats.GetStat(item.StatType);
			ModifiableValue stat2 = value.PreviewUnit.Stats.GetStat(item.StatType);
			item.StatValue.Value = stat2.ModifiedValue;
			item.DiffValue.Value = stat2.ModifiedValue - stat.ModifiedValue;
			int num2 = m_StatRanks[item.StatType];
			item.StatRanks.Value = num2;
			item.CanAdvance.Value = num > 0 && num2 < 2;
			item.CanRetreat.Value = m_SelectionStates.Any(delegate(SelectionStateFeature state)
			{
				if (state.IsMade)
				{
					FeatureSelectionItem? selectionItem = state.SelectionItem;
					if (selectionItem.HasValue && selectionItem.GetValueOrDefault().Feature is BlueprintStatAdvancement blueprintStatAdvancement)
					{
						return blueprintStatAdvancement.Stat == item.StatType;
					}
				}
				return false;
			});
			item.UpdateTooltip(stat2);
		}
		UpdatePreviewUnit();
		UpdateIsCompleted();
		OnUpdateState.Execute();
		SetupTooltipTemplate();
	}

	protected override TooltipBaseTemplate TooltipTemplate()
	{
		return SelectedItem.Value?.Tooltip.Value;
	}

	public void ShowTooltipForItem(CharGenAttributesItemVM itemVM)
	{
		ReactiveTooltipTemplate.Value = ((itemVM == null) ? TooltipTemplate() : itemVM.Tooltip.Value);
	}

	private void UpdateSkillsHighlight()
	{
		StatType? statType = SelectedItem.Value?.StatType;
		CharInfoSkillsBlock.Stats.ForEach(delegate(CharInfoStatVM i)
		{
			i.HighlightBySourceType(statType);
		});
	}

	private void UpdatePreviewUnit()
	{
		m_PreviewUnit.Value = CharGenContext.LevelUpManager.Value?.PreviewUnit;
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
		UpdatePreviewUnit();
	}

	public void HandleUICommitChanges()
	{
	}

	public void HandleUISelectionChanged()
	{
		UpdateState();
	}
}
