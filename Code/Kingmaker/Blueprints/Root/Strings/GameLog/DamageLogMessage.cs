using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Mechanics.Facts.Interfaces;
using Kingmaker.Utility.CodeTimer;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings.GameLog;

[Serializable]
public class DamageLogMessage
{
	public Color32 Color;

	public LocalizedString Message;

	public LocalizedString MessageFailedCheck;

	public LocalizedString MessageUnknownSource;

	public LocalizedString MessageCollision;

	public LocalizedString Tooltip;

	public LocalizedString TooltipSource;

	public LocalizedString TooltipSneak;

	public LocalizedString TooltipDifficulty;

	[Tooltip("Damage details in tooltip")]
	public LocalizedString DamageImmune;

	[CanBeNull]
	public CombatLogMessage GetData(RuleDealDamage rule)
	{
		using (ProfileScope.New("Build Damage Log Message"))
		{
			using (GameLogContext.Scope)
			{
				bool flag = rule.Result != rule.ResultBeforeDifficulty;
				int num = 0;
				GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)((rule.Target != rule.Initiator) ? ((MechanicEntity)rule.Initiator) : null);
				GameLogContext.SourceFact = (GameLogContext.Property<IMechanicEntityFact>)(IMechanicEntityFact)rule.Reason.Fact;
				GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)(MechanicEntity)rule.Target;
				GameLogContext.Count = rule.Result;
				if (rule.Damage != null)
				{
					GameLogContext.DamageType = UIUtilityTexts.GetTextByKey(rule.Damage.Type);
				}
				string text = (GameLogContext.HasSource ? ((string)Message) : (rule.Damage.CausedByCheckFail ? ((string)MessageFailedCheck) : ((!rule.IsCollisionDamage) ? ((string)MessageUnknownSource) : ((string)MessageCollision))));
				if (string.IsNullOrEmpty(text))
				{
					return null;
				}
				GameLogContext.Count = rule.ResultWithoutReduction;
				StringBuilder stringBuilder = GameLogUtility.StringBuilder;
				stringBuilder.Append(Tooltip);
				ItemEntityWeapon itemEntityWeapon = rule.Reason.Ability?.Weapon;
				string text2 = ((itemEntityWeapon != null) ? itemEntityWeapon.Name : rule.Reason.Name);
				if (!string.IsNullOrWhiteSpace(text2))
				{
					GameLogContext.Text = text2;
					stringBuilder.AppendLine();
					stringBuilder.Append(TooltipSource);
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				AppendDamageDetails(stringBuilder, rule);
				if (num > 0)
				{
					GameLogContext.Count = num;
					stringBuilder.AppendLine();
					stringBuilder.Append(TooltipSneak);
				}
				if (flag)
				{
					GameLogContext.Count = rule.Result;
					stringBuilder.AppendLine();
					stringBuilder.Append(TooltipDifficulty);
				}
				PrefixIcon icon = GameLogContext.GetIcon();
				TooltipTemplateCombatLogMessage template = null;
				string text3 = stringBuilder.ToString();
				if (!string.IsNullOrEmpty(text3))
				{
					template = new TooltipTemplateCombatLogMessage(text, text3);
				}
				return new CombatLogMessage(text, GetColor(), icon, template);
			}
		}
	}

	public void AppendDamageDetails(StringBuilder sb, RuleDealDamage rule)
	{
		DamageValue resultValue = rule.ResultValue;
		if (resultValue.Source.MinValue == resultValue.Source.MaxValue)
		{
			sb.Append(resultValue.Source.MinValue);
		}
		else
		{
			sb.Append(resultValue.Source.MinValue);
			sb.Append('-');
			sb.Append(resultValue.Source.MaxValue);
		}
		sb.Append(" = <b>");
		sb.Append(resultValue.ValueWithoutReduction);
		sb.Append("</b> ");
		sb.AppendLine();
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
