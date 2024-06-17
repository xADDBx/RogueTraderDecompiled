using System;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.LoadingScreen;

public class LoadingScreenRootVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ILoadingScreen
{
	public readonly ReactiveProperty<LoadingScreenVM> LoadingScreenVM = new ReactiveProperty<LoadingScreenVM>();

	private BlueprintArea m_Area;

	protected override void DisposeImplementation()
	{
		DisposeLoadingScreen();
	}

	public void ShowLoadingScreen()
	{
		LoadingScreenVM disposable = (LoadingScreenVM.Value = new LoadingScreenVM(m_Area));
		AddDisposable(disposable);
	}

	public void HideLoadingScreen()
	{
		DisposeLoadingScreen();
	}

	public LoadingScreenState GetLoadingScreenState()
	{
		return LoadingScreenVM.Value?.State ?? LoadingScreenState.Hidden;
	}

	private void DisposeLoadingScreen()
	{
		DisposeAndRemove(LoadingScreenVM);
		m_Area = null;
	}

	public void SetLoadingArea(BlueprintArea area)
	{
		m_Area = area;
		LoadingScreenVM.Value?.SetLoadingArea(area);
	}
}
