using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip;

public class TooltipBrickAnomalyInfo : ITooltipBrick
{
	private readonly BlueprintAnomaly m_BlueprintAnomaly;

	private readonly BlueprintStarSystemMap m_BlueprintStarSystemMap;

	public TooltipBrickAnomalyInfo(BlueprintAnomaly blueprintAnomaly, BlueprintStarSystemMap blueprintStarSystemMap)
	{
		m_BlueprintAnomaly = blueprintAnomaly;
		m_BlueprintStarSystemMap = blueprintStarSystemMap;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickAnomalyInfoVM(m_BlueprintAnomaly, m_BlueprintStarSystemMap);
	}
}
