using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.PC;

public class NewGamePhaseStoryScenarioEntityPCView : NewGamePhaseStoryScenarioEntityBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private NewGamePhaseStoryScenarioEntityIntegralDlcPCView m_ItemPrefab;

	protected override void DrawEntitiesImpl()
	{
		base.DrawEntitiesImpl();
		m_WidgetList.DrawEntries(base.ViewModel.IntegralDlcVms, m_ItemPrefab);
	}
}
