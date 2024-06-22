using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateUIFeature : TooltipBaseTemplate
{
	public readonly UIFeature UIFeature;

	public TooltipTemplateUIFeature(UIFeature uiFeature)
	{
		UIFeature = uiFeature;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string text = ((UIFeature.Icon != null) ? "" : UIUtility.GetAbilityAcronym(UIFeature.Name));
		Sprite icon = ((UIFeature.Icon != null) ? UIFeature.Icon : UIUtility.GetIconByText(UIFeature.NameForAcronym));
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = UIFeature.Name,
			TextParams = new TextFieldParams
			{
				FontStyles = FontStyles.Bold
			}
		};
		TooltipTemplateFeature tooltip = new TooltipTemplateFeature(UIFeature.Feature);
		string acronym = text;
		TalentIconInfo talentIconsInfo = UIFeature.TalentIconsInfo;
		yield return new TooltipBrickIconPattern(icon, null, titleValues, null, null, tooltip, IconPatternMode.SkillMode, acronym, talentIconsInfo);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDescription(list);
		AddSource(list);
		AddSelected(list);
		return list;
	}

	protected virtual void AddDescription(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickText(UIUtilityTexts.UpdateDescriptionWithUIProperties(UIFeature.Description, null)));
	}

	private void AddSource(List<ITooltipBrick> bricks)
	{
		if (UIFeature?.Source != null)
		{
			bricks.Add(new TooltipBrickSeparator());
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2));
			bricks.Add(new TooltipBrickFeature(UIFeature?.Source));
		}
	}

	private void AddSelected(List<ITooltipBrick> bricks)
	{
	}
}
