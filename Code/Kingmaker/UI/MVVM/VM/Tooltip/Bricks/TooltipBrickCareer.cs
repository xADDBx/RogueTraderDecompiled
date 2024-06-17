using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickCareer : ITooltipBrick
{
	private readonly CareerPathVM m_CareerPathVM;

	public TooltipBrickCareer(CareerPathVM careerPath)
	{
		m_CareerPathVM = careerPath;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickFeatureVM(m_CareerPathVM.CareerPath, isHeader: false, m_CareerPathVM.CareerTooltip);
	}
}
