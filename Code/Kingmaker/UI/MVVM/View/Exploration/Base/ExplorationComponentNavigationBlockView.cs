using System;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public abstract class ExplorationComponentNavigationBlockView : IDisposable
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public GridConsoleNavigationBehaviour NavigationBehaviour => m_NavigationBehaviour;

	public void Initialize()
	{
		BuildNavigation();
	}

	public void Dispose()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
	}

	private void BuildNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
	}
}
