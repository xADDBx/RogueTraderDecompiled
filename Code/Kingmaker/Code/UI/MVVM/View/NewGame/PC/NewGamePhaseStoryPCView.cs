using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.PC;

public class NewGamePhaseStoryPCView : NewGamePhaseStoryBaseView
{
	[SerializeField]
	private NewGamePhaseStoryScenarioSelectorPCView m_StorySelectorPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_StorySelectorPCView.Bind(base.ViewModel.SelectionGroup);
		AddDisposable(m_PurchaseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowInStore();
		}));
	}
}
