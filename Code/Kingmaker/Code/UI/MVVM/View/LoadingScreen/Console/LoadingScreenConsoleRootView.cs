using Kingmaker.Code.UI.MVVM.VM.LoadingScreen;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.LoadingScreen.Console;

public class LoadingScreenConsoleRootView : ViewBase<LoadingScreenRootVM>, IInitializable
{
	[SerializeField]
	private LoadingScreenConsoleView m_LoadingScreenConsoleView;

	public void Initialize()
	{
		m_LoadingScreenConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.LoadingScreenVM.Subscribe(m_LoadingScreenConsoleView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
