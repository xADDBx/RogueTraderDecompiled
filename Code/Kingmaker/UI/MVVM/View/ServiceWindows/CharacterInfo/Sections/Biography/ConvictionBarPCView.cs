using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Biography;

public class ConvictionBarPCView : ConvictionBarBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_RightButtonRadical.SetTooltip(base.ViewModel.RadicalTooltip);
		m_LeftButtonPuritan.SetTooltip(base.ViewModel.PuritanTooltip);
		m_RightLabel.SetTooltip(base.ViewModel.PuritanTooltip);
		m_LeftLabel.SetTooltip(base.ViewModel.RadicalTooltip);
	}
}
