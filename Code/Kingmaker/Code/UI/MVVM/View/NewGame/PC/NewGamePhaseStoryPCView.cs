using Kingmaker.Code.UI.MVVM.View.CustomUIVideoPlayer.PC;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.PC;

public class NewGamePhaseStoryPCView : NewGamePhaseStoryBaseView
{
	[SerializeField]
	private NewGamePhaseStoryScenarioSelectorPCView m_StorySelectorPCView;

	[SerializeField]
	private CustomUIVideoPlayerPCView m_CustomUIVideoPlayerPCView;

	public override void Initialize()
	{
		base.Initialize();
		if (!IsInit)
		{
			m_CustomUIVideoPlayerPCView.Initialize();
			IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		m_CustomUIVideoPlayerPCView.Bind(base.ViewModel.CustomUIVideoPlayerVM);
		base.BindViewImplementation();
		m_StorySelectorPCView.Bind(base.ViewModel.SelectionGroup);
		AddDisposable(m_PurchaseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ShowInStore();
		}));
		AddDisposable(m_InstallButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.InstallDlc();
		}));
	}

	protected override void ShowHideVideoImpl(bool state)
	{
		base.ShowHideVideoImpl(state);
		m_CustomUIVideoPlayerPCView.gameObject.SetActive(state);
	}
}
