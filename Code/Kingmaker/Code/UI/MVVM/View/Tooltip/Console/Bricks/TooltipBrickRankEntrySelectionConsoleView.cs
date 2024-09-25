using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickRankEntrySelectionConsoleView : TooltipBrickRankEntrySelectionView, IConsoleTooltipBrick
{
	private SimpleConsoleNavigationEntity m_MainButtonEntity;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		RankEntrySelectionFeatureVM value = base.ViewModel.RankEntrySelectionVM.SelectedFeature.Value;
		TooltipBaseTemplate tooltip = ((value != null) ? value.Tooltip.Value : base.ViewModel.RankEntrySelectionVM.Tooltip);
		m_MainButtonEntity = new SimpleConsoleNavigationEntity(m_MainButton, tooltip);
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return m_MainButtonEntity;
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
