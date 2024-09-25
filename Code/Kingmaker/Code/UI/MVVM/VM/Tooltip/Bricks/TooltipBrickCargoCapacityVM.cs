using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickCargoCapacityVM : TooltipBaseBrickVM
{
	public readonly IReadOnlyReactiveProperty<int> TotalFillValue;

	public readonly IReadOnlyReactiveProperty<int> UnusableFillValue;

	public TooltipBrickCargoCapacityVM(CargoSlotVM cargoSlotVM)
	{
		TotalFillValue = cargoSlotVM.TotalFillValue;
		UnusableFillValue = cargoSlotVM.UnusableFillValue;
	}
}
