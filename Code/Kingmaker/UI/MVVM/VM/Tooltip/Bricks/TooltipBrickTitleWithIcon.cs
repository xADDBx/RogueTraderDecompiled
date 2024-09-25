using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickTitleWithIcon : ITooltipBrick
{
	private readonly string m_Name;

	private readonly Sprite m_Icon;

	private readonly TooltipBaseTemplate m_Tooltip;

	public TooltipBrickTitleWithIcon(BlueprintFeatureBase feature, MechanicEntity caster = null)
	{
		m_Name = feature.Name;
		m_Icon = feature.Icon;
		m_Tooltip = new TooltipTemplateFeature(feature, withVariants: false, caster);
	}

	public TooltipBrickTitleWithIcon(CareerPathVM career)
	{
		m_Name = career.Name;
		m_Icon = career.Icon.Value;
		m_Tooltip = new TooltipTemplateCareer(career);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTitleWithIconVM(m_Name, m_Icon, m_Tooltip);
	}
}
