using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings.GameLog;

[Serializable]
public class HealLogMessage
{
	public Color32 Color;

	public LocalizedString HealMessage;

	public LocalizedString HealSelfMessage;

	[CanBeNull]
	public CombatLogMessage GetData(RuleCalculateHeal rule)
	{
		using (GameLogContext.Scope)
		{
			if (rule.Value <= 0)
			{
				return null;
			}
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)(MechanicEntity)rule.Initiator;
			GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteTarget;
			GameLogContext.Count = rule.Value;
			string text = ((rule.Target != rule.Initiator) ? ((string)HealMessage) : ((string)HealSelfMessage));
			TooltipTemplateCombatLogMessage template = new TooltipTemplateCombatLogMessage(text, null);
			PrefixIcon icon = GameLogContext.GetIcon();
			return new CombatLogMessage(text, GetColor(), icon, template, hasTooltip: true, 0, isSeparator: false, GameLogEventAddSeparator.States.Finish, isPerformAttackMessage: false, rule.ConcreteTarget);
		}
	}

	public void AppendHealDetails(StringBuilder sb, RuleCalculateHeal rule)
	{
		DiceFormula healFormula = rule.HealFormula;
		bool flag = healFormula.Dice != 0 && healFormula.Rolls > 0;
		if (flag)
		{
			AppendDiceFormula(sb, healFormula);
		}
		int bonus = rule.Bonus;
		if (bonus != 0)
		{
			if (flag && bonus > 0)
			{
				sb.Append('+');
			}
			sb.Append(bonus);
		}
		if (!flag && bonus == 0)
		{
			sb.Append("0");
		}
		sb.Append(" = <b>");
		sb.Append(rule.ValueWithoutReduction);
		sb.Append("</b> ");
		sb.AppendLine();
	}

	private static void AppendDiceFormula(StringBuilder sb, DiceFormula dice)
	{
		sb.Append(dice.Rolls);
		if (dice.Dice > DiceType.One)
		{
			sb.Append('d');
			sb.Append((int)dice.Dice);
		}
	}

	protected Color32 GetColor()
	{
		return Multiply((Color.r > 0 || Color.g > 0 || Color.b > 0 || Color.a > 0) ? Color : GameLogStrings.Instance.DefaultColor, GameLogStrings.Instance.ColorMultiplier);
	}

	public static Color32 Multiply(Color32 a, Color32 b)
	{
		a.r = (byte)(a.r * b.r >> 8);
		a.g = (byte)(a.g * b.g >> 8);
		a.b = (byte)(a.b * b.b >> 8);
		a.a = (byte)(a.a * b.a >> 8);
		return a;
	}
}
