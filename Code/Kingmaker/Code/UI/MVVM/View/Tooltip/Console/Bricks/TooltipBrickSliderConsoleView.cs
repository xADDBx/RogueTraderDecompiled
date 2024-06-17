using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickSliderConsoleView : TooltipBrickSliderView, IConsoleTooltipBrick
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		foreach (BrickSliderValueView sliderValueView in m_SliderValueViews)
		{
			if (sliderValueView is BrickSliderValueConsoleView brickSliderValueConsoleView)
			{
				m_NavigationBehaviour.AddEntityVertical(brickSliderValueConsoleView.GetConsoleEntity());
			}
		}
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
