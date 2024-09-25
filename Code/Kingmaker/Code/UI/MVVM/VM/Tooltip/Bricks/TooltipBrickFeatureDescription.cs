using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickFeatureDescription : ITooltipBrick
{
	private readonly TooltipBrickFeatureDescriptionVM m_FeatureVM;

	public TooltipBrickFeatureDescription(BlueprintFeatureBase feature, MechanicEntity caster = null)
	{
		m_FeatureVM = new TooltipBrickFeatureDescriptionVM(feature, caster);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return m_FeatureVM;
	}
}
