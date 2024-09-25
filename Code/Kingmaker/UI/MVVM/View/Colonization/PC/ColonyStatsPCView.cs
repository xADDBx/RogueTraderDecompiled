using Kingmaker.UI.MVVM.View.Colonization.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyStatsPCView : ColonyStatsBaseView
{
	[SerializeField]
	private ColonyStatPCView m_ColonyStatPCView;

	protected override void DrawEntitiesImpl()
	{
		m_WidgetListStats.DrawEntries(base.ViewModel.StatVMs, m_ColonyStatPCView);
	}
}
