using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.MomentumAndVeil.PC;

public class VeilThicknessPCView : VeilThicknessView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_TooltipArea.SetTooltip(base.ViewModel.Tooltip));
	}
}
