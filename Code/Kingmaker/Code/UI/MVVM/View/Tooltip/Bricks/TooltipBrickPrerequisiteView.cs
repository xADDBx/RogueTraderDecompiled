using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPrerequisiteView : TooltipBaseBrickView<TooltipBrickPrerequisiteVM>, IConsoleTooltipBrick
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private PrerequisiteEntryView m_PrerequisiteEntryView;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		DrawEntries();
		CreateNavigation();
	}

	private void DrawEntries()
	{
		m_WidgetList.DrawEntries(base.ViewModel.PrerequisiteEntries.ToArray(), m_PrerequisiteEntryView);
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		foreach (IWidgetView entry in m_WidgetList.Entries)
		{
			if (entry is PrerequisiteEntryView prerequisiteEntryView)
			{
				m_NavigationBehaviour.AddRow<FloatConsoleNavigationBehaviour>(prerequisiteEntryView.GetNavigation());
			}
		}
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return m_NavigationBehaviour;
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
