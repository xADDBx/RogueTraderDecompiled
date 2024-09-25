using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickEncumbrance : ITooltipBrick
{
	private readonly EncumbranceHelper.CarryingCapacity m_Capacity;

	public TooltipBrickEncumbrance(EncumbranceHelper.CarryingCapacity capacity)
	{
		m_Capacity = capacity;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickEncumbranceVM(m_Capacity);
	}
}
