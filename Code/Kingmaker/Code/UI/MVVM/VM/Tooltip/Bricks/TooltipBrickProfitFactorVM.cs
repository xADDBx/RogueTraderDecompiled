using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickProfitFactorVM : TooltipBaseBrickVM
{
	public readonly ProfitFactorVM ProfitFactorVM;

	public TooltipBrickProfitFactorVM(ProfitFactorVM profitFactorVM)
	{
		AddDisposable(ProfitFactorVM = profitFactorVM);
	}
}
