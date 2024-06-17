using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickTitleConsoleView : TooltipBrickTitleView, IConsoleTooltipBrick
{
	[SerializeField]
	private List<OwlcatMultiButton> m_Buttons;

	private readonly List<SimpleConsoleNavigationEntity> m_ConsoleNavigationEntities = new List<SimpleConsoleNavigationEntity>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		CreateNavigation();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_ConsoleNavigationEntities.Clear();
		for (int i = 0; i < m_TitleObjects.Count; i++)
		{
			if (m_TitleObjects[i].activeSelf && (bool)m_Buttons[i])
			{
				m_ConsoleNavigationEntities.Add(new SimpleConsoleNavigationEntity(m_Buttons[i]));
				m_Buttons[i].SetInteractable(state: false);
			}
		}
		m_NavigationBehaviour.SetEntitiesVertical(m_ConsoleNavigationEntities);
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
