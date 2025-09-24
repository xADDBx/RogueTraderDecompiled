using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickIconPatternVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly IconPatternMode IconMode;

	public readonly string Acronym;

	public readonly TalentIconInfo TalentIconInfo;

	public readonly UIUtilityItem.UIPatternData PatternData;

	public readonly TooltipBrickIconPattern.TextFieldValues TitleValues;

	public readonly TooltipBrickIconPattern.TextFieldValues SecondaryValues;

	public readonly TooltipBrickIconPattern.TextFieldValues TertiaryValues;

	public readonly TooltipBaseTemplate Tooltip;

	public readonly string AbilityPropertyName;

	public readonly string AbilityPropertyValue;

	public readonly string AbilityPropertyDesc;

	public TooltipBrickIconPatternVM(Sprite icon, UIUtilityItem.UIPatternData patternData, TooltipBrickIconPattern.TextFieldValues titleValues, TooltipBrickIconPattern.TextFieldValues secondaryValues, TooltipBrickIconPattern.TextFieldValues tertiaryValues, TooltipBaseTemplate tooltip, IconPatternMode iconMode, string acronym, TalentIconInfo talentIconsInfo, string abilityPropertyName, string abilityPropertyValue, string abilityPropertyDesc)
	{
		Icon = icon;
		PatternData = patternData;
		TitleValues = titleValues;
		SecondaryValues = secondaryValues;
		TertiaryValues = tertiaryValues;
		Tooltip = tooltip;
		IconMode = iconMode;
		Acronym = acronym;
		TalentIconInfo = talentIconsInfo;
		AbilityPropertyName = abilityPropertyName;
		AbilityPropertyValue = abilityPropertyValue;
		AbilityPropertyDesc = abilityPropertyDesc;
	}
}
