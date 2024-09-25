using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickWidget : ITooltipBrick
{
	private readonly TooltipBrickWidgetVM m_WidgetVM;

	public TooltipBrickWidget(IReadOnlyReactiveCollection<ITooltipBrick> bricks, TooltipBrickText tooltipBrickText = null)
	{
		m_WidgetVM = new TooltipBrickWidgetVM(bricks, tooltipBrickText);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return m_WidgetVM;
	}
}
