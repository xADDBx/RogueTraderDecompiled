using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateAnswerConditions : TooltipBaseTemplate
{
	private readonly BlueprintAnswer m_BlueprintAnswer;

	public TooltipTemplateAnswerConditions(BlueprintAnswer blueprintAnswer)
	{
		m_BlueprintAnswer = blueprintAnswer;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(UIStrings.Instance.Overtips.RequiredResourceCount, TooltipTitleType.H1, TextAlignmentOptions.Left);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (TryFillSelectConditions(out var result))
		{
			list.AddRange(result);
		}
		else
		{
			FillRequirements(out var result2);
			list.AddRange(result2);
		}
		return list;
	}

	private bool TryFillSelectConditions(out List<ITooltipBrick> result)
	{
		result = new List<ITooltipBrick>();
		if (m_BlueprintAnswer.SelectConditions.Conditions.Length > 1)
		{
			string text = m_BlueprintAnswer.SelectConditions.Operation switch
			{
				Operation.Or => UIStrings.Instance.Dialog.OperationOrConditionDesc, 
				Operation.And => UIStrings.Instance.Dialog.OperationAndConditionDesc, 
				_ => "", 
			};
			result.Add(new TooltipBrickText(text));
		}
		Condition[] conditions = m_BlueprintAnswer.SelectConditions.Conditions;
		foreach (Condition condition in conditions)
		{
			if (!(condition is ConditionHaveFullCargo condition2))
			{
				if (!(condition is ContextConditionHasItem condition3))
				{
					if (condition is ContextConditionHasPF condition4)
					{
						AddRequiredProfitFactor(result, condition4);
					}
				}
				else
				{
					AddRequiredItem(result, condition3);
				}
			}
			else
			{
				AddRequiredCargo(result, condition2);
			}
		}
		return result.Count > 0;
	}

	private void FillRequirements(out List<ITooltipBrick> result)
	{
		result = new List<ITooltipBrick>();
		IEnumerable<Requirement> requirements = m_BlueprintAnswer.GetRequirements();
		if (!requirements.Any())
		{
			return;
		}
		result.Add(new TooltipBrickText(UIStrings.Instance.Dialog.OperationAndConditionDesc));
		foreach (Requirement item in requirements)
		{
			RequirementUI requirement = RequirementUIFactory.GetRequirement(item);
			TooltipBrickIconStatValueType type = (item.Check() ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
			List<ITooltipBrick> obj = result;
			string name = requirement.Name;
			string countText = requirement.CountText;
			Sprite icon = requirement.Icon;
			Color? iconColor = requirement.IconColor;
			obj.Add(new TooltipBrickIconStatValue(name, countText, null, icon, type, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Normal, null, null, null, null, iconColor));
		}
	}

	private static void AddRequiredCargo(List<ITooltipBrick> result, ConditionHaveFullCargo condition)
	{
		string arg = ((condition.Cargo != null) ? condition.Cargo.Name : UIStrings.Instance.CargoTexts.GetLabelByOrigin(condition.Origin));
		string name = string.Format(UIStrings.Instance.Dialog.CargoRequiredText, arg);
		Sprite icon = ((condition.Cargo != null) ? condition.Cargo.Icon : BlueprintRoot.Instance.UIConfig.UIIcons.CargoTooltipIcons.GetIconByOrigin(condition.Origin));
		TooltipBrickIconStatValueType backgroundType = (condition.Check() ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
		Color? iconColor = Color.white;
		result.Add(new TooltipBrickIconStatValue(name, "100%", null, icon, TooltipBrickIconStatValueType.Normal, backgroundType, TooltipBrickIconStatValueStyle.Normal, null, null, null, null, iconColor));
	}

	private static void AddRequiredItem(List<ITooltipBrick> result, ContextConditionHasItem condition)
	{
		TooltipBrickIconStatValueType backgroundType = (condition.Check() ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
		string name = condition.ItemToCheck.Name;
		string value = condition.Quantity.ToString(">=0;<#");
		Sprite icon = condition.ItemToCheck.Icon;
		Color? iconColor = Color.white;
		result.Add(new TooltipBrickIconStatValue(name, value, null, icon, TooltipBrickIconStatValueType.Normal, backgroundType, TooltipBrickIconStatValueStyle.Normal, null, null, null, null, iconColor));
	}

	private static void AddRequiredProfitFactor(List<ITooltipBrick> result, ContextConditionHasPF condition)
	{
		TooltipBrickIconStatValueType backgroundType = (condition.Check() ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
		result.Add(new TooltipBrickIconStatValue(UIStrings.Instance.ProfitFactorTexts.Title, condition.Value.ToString(">=0;<#"), null, BlueprintRoot.Instance.UIConfig.UIIcons.ProfitFactor, TooltipBrickIconStatValueType.Normal, backgroundType));
	}
}
