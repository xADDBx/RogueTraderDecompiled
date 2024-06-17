using System;
using System.Text;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Mechanics.Facts.Interfaces;
using Kingmaker.Utility.CodeTimer;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings.GameLog;

[Serializable]
public class StatDamageLogMessage
{
	public Color32 Color;

	public LocalizedString MessageWithSource;

	public LocalizedString MessageWithUnknownSource;

	public LocalizedString TooltipCount;

	public LocalizedString TooltipSource;

	public CombatLogMessage GetData(RuleDealStatDamage rule, StatType statType, int count)
	{
		using (ProfileScope.New("Build Stat Damage Message"))
		{
			using (GameLogContext.Scope)
			{
				FillLogContext(rule, statType, count);
				StringBuilder stringBuilder = GameLogUtility.StringBuilder;
				AppendTooltipHeader(stringBuilder);
				AppendDamageDetails(stringBuilder, rule);
				return GetData(stringBuilder);
			}
		}
	}

	public CombatLogMessage GetData(RuleHealStatDamage evt, StatType statType, int count)
	{
		using (ProfileScope.New("Build Stat Heal Message"))
		{
			using (GameLogContext.Scope)
			{
				FillLogContext(evt, statType, count);
				StringBuilder stringBuilder = GameLogUtility.StringBuilder;
				AppendTooltipHeader(stringBuilder);
				return GetData(stringBuilder);
			}
		}
	}

	private void AppendTooltipHeader(StringBuilder tooltip)
	{
		tooltip.Append(TooltipCount);
		if (GameLogContext.HasSource)
		{
			tooltip.AppendLine();
			tooltip.Append(TooltipSource);
		}
	}

	private static void FillLogContext(RulebookTargetEvent evt, StatType statType, int count)
	{
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)(MechanicEntity)evt.Target;
		GameLogContext.Count = count;
		GameLogContext.Description = LocalizedTexts.Instance.Stats.GetText(statType);
		GameLogContext.SourceFact = (GameLogContext.Property<IMechanicEntityFact>)(IMechanicEntityFact)evt.Reason.Fact;
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)((evt.Initiator != evt.Target) ? ((MechanicEntity)evt.Initiator) : null);
	}

	private CombatLogMessage GetData(StringBuilder tooltip)
	{
		string text = (GameLogContext.HasSource ? MessageWithSource : MessageWithUnknownSource);
		PrefixIcon icon = GameLogContext.GetIcon();
		TooltipTemplateCombatLogMessage template = null;
		string text2 = tooltip.ToString();
		if (!string.IsNullOrEmpty(text2))
		{
			template = new TooltipTemplateCombatLogMessage(text, text2);
		}
		return new CombatLogMessage(text, Color, icon, template);
	}

	private void AppendDamageDetails(StringBuilder sb, RuleDealStatDamage rule)
	{
		if (rule.Result == 0)
		{
			return;
		}
		sb.AppendLine();
		sb.AppendLine();
		DiceFormula dices = rule.Dices;
		bool flag = dices.Dice != 0 && dices.Rolls > 0;
		if (flag)
		{
			sb.Append(dices.Rolls);
			if (dices.Dice > DiceType.One)
			{
				sb.Append('d');
				sb.Append((int)dices.Dice);
			}
		}
		int totalBonus = rule.TotalBonus;
		if (totalBonus != 0)
		{
			if (flag && totalBonus > 0)
			{
				sb.Append('+');
			}
			sb.Append(totalBonus);
		}
		if (!flag && totalBonus == 0)
		{
			sb.Append("0");
		}
		sb.Append(" = <b>");
		sb.Append(rule.Result);
		sb.Append("</b> ");
	}
}
