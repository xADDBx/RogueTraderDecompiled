using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPlanetInfo : ITooltipBrick
{
	private readonly BlueprintPlanet m_BlueprintPlanet;

	private readonly BlueprintStarSystemMap m_BlueprintStarSystemMap;

	public TooltipBrickPlanetInfo(BlueprintPlanet blueprintPlanet, BlueprintStarSystemMap blueprintStarSystemMap)
	{
		m_BlueprintPlanet = blueprintPlanet;
		m_BlueprintStarSystemMap = blueprintStarSystemMap;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickPlanetInfoVM(m_BlueprintPlanet, m_BlueprintStarSystemMap);
	}
}
