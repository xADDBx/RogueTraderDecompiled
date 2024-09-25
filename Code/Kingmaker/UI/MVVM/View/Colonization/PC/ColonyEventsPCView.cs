using Kingmaker.UI.MVVM.View.Colonization.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyEventsPCView : ColonyEventsBaseView
{
	[SerializeField]
	private ColonyEventPCView m_ColonyEventPCView;

	protected override void DrawEntitiesImpl()
	{
		m_WidgetListEvents.DrawEntries(base.ViewModel.EventsVMs, m_ColonyEventPCView);
	}
}
