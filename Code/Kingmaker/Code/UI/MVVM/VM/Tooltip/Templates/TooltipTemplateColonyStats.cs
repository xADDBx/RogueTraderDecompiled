using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UI.MVVM.VM.Colonization.Stats;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateColonyStats : TooltipBaseTemplate
{
	private ColonyStatType m_ColonyStatType;

	private Colony m_Colony;

	[CanBeNull]
	public string StatName { get; }

	[CanBeNull]
	public string StatDescription { get; }

	[CanBeNull]
	public int TotalCount { get; }

	public TooltipTemplateColonyStats(Colony colony, string statName, string statDescription, int totalCount, ColonyStatType colonyStatType)
	{
		m_Colony = colony;
		StatName = statName;
		StatDescription = statDescription;
		TotalCount = totalCount;
		m_ColonyStatType = colonyStatType;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(StatName, TooltipTitleType.H2, TextAlignmentOptions.Left);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_ColonyStatType == ColonyStatType.Efficiency)
		{
			List<ColonyStatModifier> list2 = m_Colony?.Efficiency?.Modifiers;
			if (list2 != null && list2.Count != 0)
			{
				list.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.BonusesSum));
				list.Add(new TooltipBrickSpace());
				foreach (ColonyStatModifier item in list2)
				{
					string value = item.Value.ToString("+#;-#");
					list.Add(new TooltipBrickIconStatValue(GetOriginName(item), value, null, null, (item.Value > 0f) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative));
				}
			}
		}
		else if (m_ColonyStatType == ColonyStatType.Contentment)
		{
			List<ColonyStatModifier> list3 = m_Colony?.Contentment?.Modifiers;
			if (list3 != null && list3.Count != 0)
			{
				list.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.BonusesSum));
				list.Add(new TooltipBrickSpace());
				foreach (ColonyStatModifier item2 in list3)
				{
					string value2 = item2.Value.ToString("+#;-#");
					list.Add(new TooltipBrickIconStatValue(GetOriginName(item2), value2, null, null, (item2.Value > 0f) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative));
				}
			}
		}
		else if (m_ColonyStatType == ColonyStatType.Security)
		{
			List<ColonyStatModifier> list4 = m_Colony?.Security?.Modifiers;
			if (list4 != null && list4.Count != 0)
			{
				list.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.BonusesSum));
				list.Add(new TooltipBrickSpace());
				foreach (ColonyStatModifier item3 in list4)
				{
					string value3 = item3.Value.ToString("+#;-#");
					list.Add(new TooltipBrickIconStatValue(GetOriginName(item3), value3, null, null, (item3.Value > 0f) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative));
				}
			}
		}
		list.Add(new TooltipBrickSpace());
		list.Add(new TooltipBrickText(StatDescription));
		return list;
	}

	private string GetOriginName(ColonyStatModifier mod)
	{
		if (mod.Modifier is IUIDataProvider iUIDataProvider)
		{
			return iUIDataProvider.Name;
		}
		if (mod.ModifierType == ColonyStatModifierType.Other)
		{
			return UIStrings.Instance.ColonizationTexts.ColonyStatModifierOriginOther;
		}
		return $"{mod.ModifierType}";
	}
}
