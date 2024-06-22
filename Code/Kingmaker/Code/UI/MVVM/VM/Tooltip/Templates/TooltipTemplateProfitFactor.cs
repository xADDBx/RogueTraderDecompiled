using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateProfitFactor : TooltipBaseTemplate
{
	private readonly ProfitFactorVM m_ProfitFactorVM;

	public TooltipTemplateProfitFactor(ProfitFactorVM profitFactorVM)
	{
		m_ProfitFactorVM = profitFactorVM;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(UIStrings.Instance.ProfitFactorTexts.Title, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string profitFactorFormatted = UIUtility.GetProfitFactorFormatted(m_ProfitFactorVM.TotalValue.Value);
		list.Add(new TooltipBrickIconStatValue(UIStrings.Instance.ProfitFactorTexts.TotalValue, profitFactorFormatted, null, null, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Bold));
		IEnumerable<ProfitFactorModifierVM> enumerable = m_ProfitFactorVM.Modifiers.Where((ProfitFactorModifierVM mod) => mod.IsNegative);
		IEnumerable<ProfitFactorModifierVM> enumerable2 = m_ProfitFactorVM.Modifiers.Except(enumerable);
		if (enumerable2.Any() || enumerable.Any())
		{
			list.Add(new TooltipBrickSpace());
		}
		AddModifiers(list, UIStrings.Instance.ProfitFactorTexts.Income, enumerable2, isPositive: true);
		AddModifiers(list, UIStrings.Instance.ProfitFactorTexts.Loss, enumerable, isPositive: false);
		list.Add(new TooltipBrickText(UIStrings.Instance.ProfitFactorTexts.Description));
		return list;
	}

	private void AddModifiers(List<ITooltipBrick> bricks, string title, IEnumerable<ProfitFactorModifierVM> mods, bool isPositive)
	{
		if (!mods.Any())
		{
			return;
		}
		bricks.Add(new TooltipBricksGroupStart());
		bricks.Add(new TooltipBrickTitle(title, TooltipTitleType.H4));
		foreach (ProfitFactorModifierVM mod in mods)
		{
			AddModifier(bricks, mod, isPositive);
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private void AddModifier(List<ITooltipBrick> bricks, ProfitFactorModifierVM mod, bool isPositive)
	{
		bricks.Add(GetModBrick(mod, isPositive));
	}

	public static TooltipBrickIconStatValue GetModBrick(ProfitFactorModifierVM mod, bool isPositive)
	{
		string value = mod.ModifierValue.Value.ToString("+0.#;-0.#");
		TooltipBrickIconStatValueType type = (isPositive ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
		return new TooltipBrickIconStatValue(GetModifierName(mod), value, null, null, type);
	}

	private static string GetModifierName(ProfitFactorModifierVM mod)
	{
		switch (mod.Type)
		{
		case ProfitFactorModifierType.Project:
		{
			BlueprintColonyProject blueprintColonyProject = mod.Modifier.Modifier as BlueprintColonyProject;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintColonyProject?.Name);
		}
		case ProfitFactorModifierType.Event:
		{
			BlueprintColonyEvent blueprintColonyEvent = mod.Modifier.Modifier as BlueprintColonyEvent;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintColonyEvent?.Name);
		}
		case ProfitFactorModifierType.Order:
		{
			BlueprintQuestContract blueprintQuestContract = mod.Modifier.Modifier as BlueprintQuestContract;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintQuestContract?.Name);
		}
		case ProfitFactorModifierType.Chronicles:
		{
			BlueprintColonyChronicle blueprintColonyChronicle = mod.Modifier.Modifier as BlueprintColonyChronicle;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintColonyChronicle?.Name);
		}
		case ProfitFactorModifierType.ResourceShortage:
		{
			BlueprintResource blueprintResource = mod.Modifier.Modifier as BlueprintResource;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintResource?.Name);
		}
		case ProfitFactorModifierType.ColonyFoundation:
		{
			BlueprintColony blueprintColony = mod.Modifier.Modifier as BlueprintColony;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintColony?.Name);
		}
		case ProfitFactorModifierType.Answer:
			return (mod.Modifier.Modifier as BlueprintAnswer)?.Description;
		case ProfitFactorModifierType.Cue:
			return (mod.Modifier.Modifier as BlueprintCue)?.Description;
		case ProfitFactorModifierType.Other:
		case ProfitFactorModifierType.Companion:
		case ProfitFactorModifierType.Respec:
			return UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
