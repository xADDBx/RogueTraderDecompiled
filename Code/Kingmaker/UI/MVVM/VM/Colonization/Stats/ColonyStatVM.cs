using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Colonization.Stats;

public class ColonyStatVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<ColonyStatType> StatType = new ReactiveProperty<ColonyStatType>();

	public readonly ReactiveProperty<string> StatName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<int> StatValue = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<bool> IsNegativelyModified = new ReactiveProperty<bool>(initialValue: false);

	public ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public ColonyStatVM(Colony colony, ColonyStatType colonyStatType)
	{
		StatType.Value = colonyStatType;
		UIColonizationTexts.ColonyStatsStrings statStrings = UIStrings.Instance.ColonizationTexts.GetStatStrings((int)colonyStatType);
		StatName.Value = statStrings.Name;
		Tooltip.Value = new TooltipTemplateColonyStats(colony, statStrings.Name, statStrings.Description, StatValue.Value, colonyStatType);
	}

	protected override void DisposeImplementation()
	{
	}

	public void UpdateStat(Colony colony)
	{
		ColonyStat colonyStat = StatType.Value switch
		{
			ColonyStatType.Efficiency => colony.Efficiency, 
			ColonyStatType.Contentment => colony.Contentment, 
			ColonyStatType.Security => colony.Security, 
			_ => null, 
		};
		if (colonyStat == null)
		{
			PFLog.UI.Error("ColonyStatVM.UpdateStat - can't find stat");
			return;
		}
		StatValue.Value = colonyStat.Value;
		IsNegativelyModified.Value = CheckStatModifiedByEvent(colonyStat);
	}

	private bool CheckStatModifiedByEvent(ColonyStat stat)
	{
		foreach (ColonyStatModifier modifier in stat.Modifiers)
		{
			ColonyStatModifierType modifierType = modifier.ModifierType;
			bool num = modifierType == ColonyStatModifierType.Event || modifierType == ColonyStatModifierType.Trait;
			bool flag = modifier.Value < 0f;
			if (num && flag)
			{
				return true;
			}
		}
		return false;
	}
}
