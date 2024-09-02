using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.Utility;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateStat : TooltipBaseTemplate
{
	protected readonly StatTooltipData StatData;

	protected readonly string Name;

	protected readonly string Desc;

	private readonly bool m_ShowCompanionStats;

	public TooltipTemplateStat(StatTooltipData statData, bool showCompanionStats = false)
	{
		try
		{
			StatData = statData;
			BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtility.GetGlossaryEntry(statData?.KeyWord);
			Name = glossaryEntry?.Title;
			Desc = glossaryEntry?.GetDescription();
			m_ShowCompanionStats = showCompanionStats;
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {statData?.Type}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(Name, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new TooltipBrickSeparator());
		AddBonusValue(list);
		AddTotalValue(list);
		AddStatBonusesGroup(list);
		AddCompanionsStats(list);
		list.Add(new TooltipBrickText(Desc));
		return list;
	}

	private void AddBonusValue(List<ITooltipBrick> result)
	{
		if (StatData.BonusValue.HasValue)
		{
			result.Add(new TooltipBrickIconValueStat(UIStrings.Instance.Tooltips.BonusValue, UIConstsExtensions.GetValueWithSign(StatData.BonusValue.Value)));
			result.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
		}
	}

	private void AddTotalValue(List<ITooltipBrick> result)
	{
		result.Add(new TooltipBrickIconValueStat(StatData.TotalValueLabel, StatData.ModifiedValue.ToString()));
	}

	private void AddStatBonusesGroup(List<ITooltipBrick> bricks)
	{
		bool hasBonuses = StatData.Breakdown.HasBonuses;
		bool flag = false;
		List<StatBonusEntry> sortedBonuses = StatData.Breakdown.SortedBonuses;
		if (StatData.Group != StatGroup.Skill && StatData.BaseValue != 0)
		{
			int num = StatData.BaseValue;
			if (hasBonuses)
			{
				int num2 = 0;
				foreach (StatBonusEntry item in sortedBonuses)
				{
					if (item.Descriptor == ModifierDescriptor.Difficulty)
					{
						num2 = item.Bonus;
						flag = sortedBonuses.Count == 1;
						break;
					}
				}
				num += num2;
			}
			bricks.Add(new TooltipBrickTextValue(UIStrings.Instance.Tooltips.BaseValue, num.ToString(), 1));
		}
		if (!hasBonuses || flag)
		{
			return;
		}
		foreach (StatBonusEntry item2 in sortedBonuses)
		{
			if (item2.Descriptor != ModifierDescriptor.Difficulty)
			{
				string valueWithSign = UIConstsExtensions.GetValueWithSign(item2.Bonus);
				string empty = string.Empty;
				string text = string.Empty;
				string text2 = string.Empty;
				if (item2.Descriptor != 0)
				{
					text = Game.Instance.BlueprintRoot.LocalizedTexts.Descriptors.GetText(item2.Descriptor);
				}
				if (!string.IsNullOrWhiteSpace(item2.Source))
				{
					text2 = item2.Source;
				}
				empty = ((!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2)) ? (text + " [" + text2 + "]") : (string.IsNullOrWhiteSpace(text) ? text2 : text));
				bricks.Add(new TooltipBrickTextValue(empty, valueWithSign, 1));
			}
		}
	}

	private void AddCompanionsStats(List<ITooltipBrick> bricks)
	{
		if (StatData == null || !m_ShowCompanionStats)
		{
			return;
		}
		BaseUnitEntity currentUnit = Game.Instance.SelectionCharacter.SelectedUnitInUI.Value;
		IEnumerable<BaseUnitEntity> enumerable = from ch in UIUtility.GetGroup((Game.Instance.LoadedAreaState?.Settings?.CapitalPartyMode).GetValueOrDefault())
			where ch != currentUnit
			select ch;
		if (!enumerable.Any())
		{
			return;
		}
		bricks.Add(new TooltipBricksGroupStart());
		foreach (BaseUnitEntity item in enumerable)
		{
			AddCharacterStatValue(bricks, item);
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private void AddCharacterStatValue(List<ITooltipBrick> bricks, BaseUnitEntity character)
	{
		StatTooltipData statData = StatData;
		if (statData != null && statData.Type.HasValue)
		{
			string value = character.Stats.GetStat(StatData.Type.Value).ModifiedValue.ToString();
			bricks.Add(new TooltipBrickIconStatValue(character.CharacterName, value, null, null, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Bold));
		}
	}
}
