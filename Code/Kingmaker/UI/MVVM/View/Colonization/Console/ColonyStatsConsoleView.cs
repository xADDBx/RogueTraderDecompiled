using Kingmaker.UI.MVVM.View.Colonization.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Console;

public class ColonyStatsConsoleView : ColonyStatsBaseView
{
	[SerializeField]
	private ColonyStatConsoleView m_ColonyStatConsoleView;

	protected override void DrawEntitiesImpl()
	{
		m_WidgetListStats.DrawEntries(base.ViewModel.StatVMs, m_ColonyStatConsoleView);
	}
}
