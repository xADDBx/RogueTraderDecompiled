using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickCargoCapacity : ITooltipBrick
{
	private readonly CargoSlotVM m_CargoSlot;

	public TooltipBrickCargoCapacity(CargoSlotVM cargoSlotVM)
	{
		m_CargoSlot = cargoSlotVM;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickCargoCapacityVM(m_CargoSlot);
	}
}
