using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickBackgroundFeature : ITooltipBrick
{
	private readonly BlueprintFeature m_Feature;

	private readonly FeatureTypes m_Type;

	private readonly MechanicEntity m_Caster;

	public TooltipBrickBackgroundFeature(BlueprintFeature feature, FeatureTypes type = FeatureTypes.Common, BaseUnitEntity caster = null)
	{
		m_Feature = feature;
		m_Type = type;
		m_Caster = caster;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickFeatureVM(m_Feature, isHeader: false, available: true, showIcon: true, new TooltipTemplateChargenBackground(m_Feature, isInfoWindow: false), m_Type, m_Caster);
	}
}
