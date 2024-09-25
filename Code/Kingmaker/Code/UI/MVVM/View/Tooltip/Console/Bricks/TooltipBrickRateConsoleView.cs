using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickRateConsoleView : TooltipBrickRateView, IConsoleTooltipBrick
{
	[SerializeField]
	private List<OwlcatMultiButton> m_Buttons;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.SetEntitiesHorizontal(m_Buttons.Select((OwlcatMultiButton b) => new SimpleConsoleNavigationEntity(b)).ToList());
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
