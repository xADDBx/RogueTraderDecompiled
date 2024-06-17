using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.TextTools;

internal static class LogHelper
{
	internal static string GetEntityName([CanBeNull] IMechanicEntity entity)
	{
		if (entity == null)
		{
			return "<no entity>";
		}
		return GetEntityName((MechanicEntity)entity);
	}

	internal static string GetEntityName([CanBeNull] MechanicEntity entity)
	{
		if (entity == null)
		{
			return "<no entity>";
		}
		string text = (entity.IsDisposed ? entity.Blueprint.Name : (entity.GetDescriptionOptional()?.Name ?? entity.Blueprint.Name));
		if (entity is LightweightUnitEntity lightweightUnitEntity)
		{
			text = lightweightUnitEntity.Name;
		}
		if (text.IsNullOrEmpty() && entity is SimpleCaster simpleCaster)
		{
			text = simpleCaster.NameInLog;
		}
		if (text.IsNullOrEmpty())
		{
			return "<no name>";
		}
		if (!(entity is AbstractUnitEntity abstractUnitEntity))
		{
			return "<b>" + text + "</b>";
		}
		string text2 = ColorUtility.ToHtmlStringRGB(abstractUnitEntity.Blueprint.Color * UIConfig.Instance.DialogColors.NameColorMultiplyer);
		return "<b><color=#" + text2 + ">" + text + "</color></b>";
	}

	internal static string GetRollDescription(IRuleRollDice ruleRollDice)
	{
		if (ruleRollDice == null)
		{
			PFLog.Default.Error("Missing game log context property!");
			return string.Empty;
		}
		if (ruleRollDice.RollHistory == null || ruleRollDice.RollHistory.Count == 1)
		{
			if (!ruleRollDice.ReplacedOne)
			{
				return ruleRollDice.Result.ToString();
			}
			return "<s>1</s> 20";
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		bool flag2 = false;
		string value = "";
		string value2 = "";
		foreach (int item in ruleRollDice.RollHistory)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			flag = false;
			if (item == ruleRollDice.Result && !flag2)
			{
				value = "<b><u>";
				value2 = "</u></b>";
				flag2 = true;
			}
			if (ruleRollDice.ReplaceOneWithMax && item == 1)
			{
				stringBuilder.Append(value).Append("<s>1</s> ").Append("20")
					.Append(value2);
			}
			else
			{
				stringBuilder.Append(value).Append(item).Append(value2);
			}
		}
		if (ruleRollDice.Rerolls != null)
		{
			string text = "";
			foreach (RerollData reroll in ruleRollDice.Rerolls)
			{
				if (((MechanicEntityFact)reroll.Source).Blueprint is BlueprintUnitFact blueprintUnitFact)
				{
					text += blueprintUnitFact.Name;
				}
			}
			if (text != "")
			{
				stringBuilder.Append(" [").Append(text).Append("]");
			}
		}
		return stringBuilder.ToString();
	}
}
