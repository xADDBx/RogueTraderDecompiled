using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.PC;

public class NewGamePhaseStoryScenarioSelectorPCView : NewGamePhaseStoryScenarioSelectorBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private NewGamePhaseStoryScenarioEntityPCView m_ItemPrefab;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_ItemPrefab));
	}
}
