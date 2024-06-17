using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickProfitFactor : ITooltipBrick
{
	private readonly ProfitFactorVM m_ProfitFactorVM;

	public TooltipBrickProfitFactor(ProfitFactorVM profitFactorVM)
	{
		m_ProfitFactorVM = profitFactorVM;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickProfitFactorVM(m_ProfitFactorVM);
	}
}
