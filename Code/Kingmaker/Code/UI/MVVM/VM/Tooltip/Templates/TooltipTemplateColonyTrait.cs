using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateColonyTrait : TooltipBaseTemplate
{
	[CanBeNull]
	public string TraitName { get; }

	[CanBeNull]
	public string TraitMechanicString { get; }

	[CanBeNull]
	public string TraitDescription { get; }

	public int EfficiencyModifier { get; }

	public int ContentmentModifier { get; }

	public int SecurityModifier { get; }

	public TooltipTemplateColonyTrait(string traitName, string traitMechanicString, string traitDescription, int efficiencyModifier, int contentmentModifier, int securityModifier)
	{
		TraitName = traitName;
		TraitMechanicString = traitMechanicString;
		TraitDescription = traitDescription;
		EfficiencyModifier = efficiencyModifier;
		ContentmentModifier = contentmentModifier;
		SecurityModifier = securityModifier;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(TraitName, TooltipTitleType.H2, TextAlignmentOptions.Left);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new TooltipBrickText(TraitDescription));
		list.Add(new TooltipBrickSpace());
		list.Add(new TooltipBrickText("<i>" + TraitMechanicString + "</i>"));
		list.Add(new TooltipBrickSpace());
		UIColonizationTexts.ColonyStatsStrings[] statStrings = UIStrings.Instance.ColonizationTexts.StatStrings;
		if (EfficiencyModifier != 0)
		{
			UIColonizationTexts.ColonyStatsStrings colonyStatsStrings = statStrings[0];
			list.Add(new TooltipBrickIconStatValue(colonyStatsStrings.Name, EfficiencyModifier.ToString("+#;-#"), null, null, (EfficiencyModifier > 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative));
		}
		if (ContentmentModifier != 0)
		{
			UIColonizationTexts.ColonyStatsStrings colonyStatsStrings2 = statStrings[2];
			list.Add(new TooltipBrickIconStatValue(colonyStatsStrings2.Name, ContentmentModifier.ToString("+#;-#"), null, null, (ContentmentModifier > 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative));
		}
		if (SecurityModifier != 0)
		{
			UIColonizationTexts.ColonyStatsStrings colonyStatsStrings3 = statStrings[1];
			list.Add(new TooltipBrickIconStatValue(colonyStatsStrings3.Name, SecurityModifier.ToString("+#;-#"), null, null, (SecurityModifier > 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative));
		}
		return list;
	}
}
