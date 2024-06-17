using System;
using System.Collections.Generic;
using System.Globalization;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.Utility;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateAbsorption : TooltipBaseTemplate
{
	private readonly RuleCalculateStatsArmor m_StatsRule;

	private readonly BlueprintEncyclopediaGlossaryEntry m_AbsorptionGlossaryEntry;

	private readonly StatModifiersBreakdownData m_AbsoprtionValueModifiersData;

	public TooltipTemplateAbsorption(RuleCalculateStatsArmor statsRule)
	{
		m_StatsRule = statsRule;
		m_AbsorptionGlossaryEntry = UIUtility.GetGlossaryEntry("Absorption");
		StatModifiersBreakdown.AddCompositeModifiersManager(m_StatsRule.AbsorptionCompositeModifiers);
		m_AbsoprtionValueModifiersData = StatModifiersBreakdown.Build();
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_AbsorptionGlossaryEntry?.Title);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new TooltipBrickIconStatValue(UIStrings.Instance.CharacterSheet.Equipment, $"{m_StatsRule.ResultBaseAbsorption}%", null, null, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Bold));
		AddDodgeModifiers(list);
		list.Add(new TooltipBrickText(UIStrings.Instance.Inspect.UnconditionalModifiers, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		list.Add(new TooltipBrickSpace());
		list.Add(new TooltipBrickText(m_AbsorptionGlossaryEntry?.GetDescription()));
		return list;
	}

	private void AddDodgeModifiers(List<ITooltipBrick> bricks)
	{
		if (m_AbsoprtionValueModifiersData.HasBonuses)
		{
			bricks.Add(new TooltipBricksGroupStart());
			AddDodgeModifiers(bricks, m_AbsoprtionValueModifiersData, isValueModifiers: true);
			bricks.Add(new TooltipBricksGroupEnd());
		}
	}

	private void AddDodgeModifiers(List<ITooltipBrick> bricks, StatModifiersBreakdownData breakdownData, bool isValueModifiers)
	{
		foreach (StatBonusEntry sortedBonuse in breakdownData.SortedBonuses)
		{
			string text = string.Empty;
			if (sortedBonuse.Descriptor != 0)
			{
				text = Game.Instance.BlueprintRoot.LocalizedTexts.Descriptors.GetText(sortedBonuse.Descriptor);
			}
			else if (!string.IsNullOrWhiteSpace(sortedBonuse.Source))
			{
				text = sortedBonuse.Source;
			}
			if (sortedBonuse.Descriptor != 0 && !string.IsNullOrWhiteSpace(sortedBonuse.Source))
			{
				text = text + " [" + sortedBonuse.Source + "]";
			}
			TooltipBrickIconStatValueType type = ((sortedBonuse.Bonus >= 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
			string value = (isValueModifiers ? (UIConstsExtensions.GetValueWithSign(sortedBonuse.Bonus) + "%") : ("×" + ((float)Math.Abs(sortedBonuse.Bonus) / 100f).ToString(CultureInfo.InvariantCulture)));
			bricks.Add(new TooltipBrickIconStatValue(text, value, null, null, type));
		}
	}
}
