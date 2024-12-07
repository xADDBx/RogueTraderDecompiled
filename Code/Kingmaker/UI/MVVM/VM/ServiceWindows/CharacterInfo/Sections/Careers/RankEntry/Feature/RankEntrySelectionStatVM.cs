using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;

public class RankEntrySelectionStatVM : RankEntrySelectionFeatureVM
{
	public readonly string StatDisplayName;

	public readonly string ShortName;

	public readonly ReactiveProperty<string> StatIncreaseLabel = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> SummaryStatIncreaseLabel = new ReactiveProperty<string>();

	private readonly BlueprintStatAdvancement m_StatAdvancement;

	private readonly ModifiableValue m_UnitStat;

	public RankEntrySelectionStatVM(RankEntrySelectionVM owner, CareerPathVM careerPathVM, FeatureSelectionItem featureSelectionItem, IReadOnlyReactiveProperty<SelectionStateFeature> selectionState, Action<FeatureSelectionItem?> selectFeature)
		: base(owner, careerPathVM, featureSelectionItem, selectionState, selectFeature)
	{
		if (base.Feature is BlueprintStatAdvancement blueprintStatAdvancement)
		{
			StatDisplayName = LocalizedTexts.Instance.Stats.GetText(blueprintStatAdvancement.Stat);
			ShortName = UIUtilityTexts.GetStatShortName(blueprintStatAdvancement.Stat);
			m_StatAdvancement = blueprintStatAdvancement;
			m_UnitStat = owner.UnitProgressionVM.Unit.Value.Stats.GetStat(blueprintStatAdvancement.Stat);
			AddDisposable(OnUpdateState.Subscribe(delegate
			{
				UpdateIncreaseLabel();
			}));
			SetTooltip();
			UpdateIncreaseLabel();
		}
	}

	private void UpdateIncreaseLabel()
	{
		StatIncreaseLabel.Value = FormatStat(m_StatAdvancement);
		LevelUpManager levelUpManager = Owner.UnitProgressionVM.LevelUpManager;
		BaseUnitEntity obj = levelUpManager?.TargetUnit ?? Owner.UnitProgressionVM.Unit.Value;
		BaseUnitEntity baseUnitEntity = levelUpManager?.PreviewUnit;
		ModifiableValue stat = obj.Stats.GetStat(m_StatAdvancement.Stat);
		ModifiableValue obj2 = baseUnitEntity?.Stats?.GetStat(m_StatAdvancement.Stat);
		SummaryStatIncreaseLabel.Value = $"{stat.ModifiedValue} > {obj2?.ModifiedValue}";
	}

	private void SetTooltip()
	{
		ModifiableValue unitStat = m_UnitStat;
		StatTooltipData statTooltipData = default(StatTooltipData);
		if (!(unitStat is ModifiableValueAttributeStat attribute))
		{
			if (!(unitStat is ModifiableValueSkill skill))
			{
				if (!(unitStat is ModifiableValueSavingThrow savingThrow))
				{
					if (unitStat != null)
					{
						statTooltipData = new StatTooltipData(m_UnitStat);
					}
					else
					{
						global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(unitStat);
					}
				}
				else
				{
					statTooltipData = new StatTooltipData(savingThrow);
				}
			}
			else
			{
				statTooltipData = new StatTooltipData(skill);
			}
		}
		else
		{
			statTooltipData = new StatTooltipData(attribute);
		}
		StatTooltipData statData = statTooltipData;
		base.Tooltip.Value = new TooltipTemplateRankEntryStat(statData, SelectionItem, SelectionState, showCompanionStats: true);
	}

	private string FormatStat(BlueprintStatAdvancement statAdvancement)
	{
		return "+" + statAdvancement.ValuePerRank;
	}
}
