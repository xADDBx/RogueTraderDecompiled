using Kingmaker.Code.UI.MVVM.VM.LoadingScreen;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.LoadingScreen.PC;

public class LoadingScreenPCRootView : ViewBase<LoadingScreenRootVM>, IInitializable
{
	[SerializeField]
	private LoadingScreenPCView m_LoadingScreenPCView;

	public void Initialize()
	{
		m_LoadingScreenPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.LoadingScreenVM.Subscribe(m_LoadingScreenPCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
