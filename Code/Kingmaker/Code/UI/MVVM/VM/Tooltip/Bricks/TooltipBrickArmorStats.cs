using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickArmorStats : ITooltipBrick
{
	private BaseUnitEntity m_Unit;

	public TooltipBrickArmorStats(BaseUnitEntity unit)
	{
		m_Unit = unit;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickArmorStatsVM(m_Unit);
	}
}
