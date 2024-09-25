using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.Utility;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateDodge : TooltipBaseTemplate
{
	private readonly RuleCalculateDodgeChance m_DodgeRule;

	private readonly BlueprintEncyclopediaGlossaryEntry m_DodgeGlossaryEntry;

	private readonly StatModifiersBreakdownData m_DodgeValueModifiersData;

	private readonly StatModifiersBreakdownData m_DodgePercentModifiersData;

	public TooltipTemplateDodge(RuleCalculateDodgeChance dodgeRule)
	{
		m_DodgeRule = dodgeRule;
		m_DodgeGlossaryEntry = UIUtility.GetGlossaryEntry("Dodge");
		StatModifiersBreakdown.AddModifiersManager(m_DodgeRule.DodgeValueModifiers);
		m_DodgeValueModifiersData = StatModifiersBreakdown.Build();
		StatModifiersBreakdown.AddModifiersManager(m_DodgeRule.DodgePercentModifiers);
		m_DodgePercentModifiersData = StatModifiersBreakdown.Build();
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_DodgeGlossaryEntry?.Title);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new TooltipBrickSeparator());
		if (m_DodgeRule.IsAutoDodge)
		{
			AddAutoDodge(list);
		}
		else
		{
			list.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Tooltips.BaseChance, UIConfig.Instance.PercentHelper.AddPercentTo(m_DodgeRule.BaseValue), null, null, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Bold));
			AddDodgeModifiers(list);
			list.Add(new TooltipBrickText(UIStrings.Instance.Inspect.UnconditionalModifiers, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		}
		list.Add(new TooltipBrickText(m_DodgeGlossaryEntry?.GetDescription()));
		return list;
	}

	private void AddDodgeModifiers(List<ITooltipBrick> bricks)
	{
		if (m_DodgeValueModifiersData.HasBonuses || m_DodgePercentModifiersData.HasBonuses)
		{
			bricks.Add(new TooltipBricksGroupStart());
			AddDodgeModifiers(bricks, m_DodgeValueModifiersData, isValueModifiers: true);
			AddDodgeModifiers(bricks, m_DodgePercentModifiersData, isValueModifiers: false);
			bricks.Add(new TooltipBricksGroupEnd());
		}
	}

	private void AddAutoDodge(List<ITooltipBrick> bricks)
	{
		FeatureCountableFlag.BuffList associatedBuffs = m_DodgeRule.Defender.Features.AutoDodge.AssociatedBuffs;
		bricks.Add(new TooltipBrickTriggeredAuto(GameLogStrings.Instance.TooltipBrickStrings.AutoDodge.Text, associatedBuffs.Buffs.ToList(), isSuccess: true));
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
			if (sortedBonuse.Descriptor == ModifierDescriptor.Difficulty && SettingsHelper.CalculateCRModifier() < 1f)
			{
				text = text + " (" + UIStrings.Instance.Tooltips.DifficultyReduceDescription.Text + ")";
			}
			TooltipBrickIconStatValueType type = ((sortedBonuse.Bonus >= 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
			string value = (isValueModifiers ? UIConfig.Instance.PercentHelper.AddPercentTo(UIConstsExtensions.GetValueWithSign(sortedBonuse.Bonus)) : ("×" + ((float)Math.Abs(sortedBonuse.Bonus) / 100f).ToString(CultureInfo.InvariantCulture)));
			bricks.Add(new TooltipBrickIconStatValue(text, value, null, null, type));
		}
	}
}
