using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickOtherObjectsInfo : ITooltipBrick
{
	private readonly BlueprintArtificialObject m_BlueprintOtherObject;

	private readonly BlueprintStarSystemMap m_BlueprintStarSystemMap;

	public TooltipBrickOtherObjectsInfo(BlueprintArtificialObject blueprintOtherObject, BlueprintStarSystemMap blueprintStarSystemMap)
	{
		m_BlueprintOtherObject = blueprintOtherObject;
		m_BlueprintStarSystemMap = blueprintStarSystemMap;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickOtherObjectsInfoVM(m_BlueprintOtherObject, m_BlueprintStarSystemMap);
	}
}
