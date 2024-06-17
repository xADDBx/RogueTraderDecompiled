using Kingmaker.UI.MVVM.View.Colonization.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Console;

public class ColonyEventsConsoleView : ColonyEventsBaseView
{
	[SerializeField]
	private ColonyEventConsoleView m_ColonyEventConsoleView;

	protected override void DrawEntitiesImpl()
	{
		m_WidgetListEvents.DrawEntries(base.ViewModel.EventsVMs, m_ColonyEventConsoleView);
	}
}
