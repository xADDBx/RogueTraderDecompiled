using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickFeatureDescriptionVM : TooltipBaseBrickVM
{
	public string Name;

	public string Description;

	public Sprite Icon;

	public Sprite SpecialFrame;

	public Color32 IconColor;

	public string Acronym;

	public TooltipBaseTemplate Tooltip;

	public TooltipBrickFeatureDescriptionVM(BlueprintFeatureBase feature, MechanicEntity caster = null)
	{
		Name = feature.Name;
		Description = feature.Description;
		if (feature.Icon != null)
		{
			Icon = feature.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtility.GetIconByText(feature.Name);
			IconColor = UIUtility.GetColorByText(feature.Name);
			Acronym = UIUtility.GetAbilityAcronym(feature);
		}
		Tooltip = new TooltipTemplateFeature(feature, withVariants: false, caster);
	}
}
