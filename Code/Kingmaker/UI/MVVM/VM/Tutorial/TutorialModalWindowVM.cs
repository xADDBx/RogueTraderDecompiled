using System;
using Kingmaker.Tutorial;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Tutorial;

public class TutorialModalWindowVM : TutorialWindowVM
{
	public IntReactiveProperty CurrentPageIndex { get; } = new IntReactiveProperty();


	public ReactiveProperty<TutorialData.Page> CurrentPage { get; } = new ReactiveProperty<TutorialData.Page>();


	public int PageCount => base.Pages?.Count ?? 0;

	public bool MultiplePages => PageCount > 1;

	public TutorialModalWindowVM(TutorialData data, Action callbackHide)
		: base(data, callbackHide)
	{
		AddDisposable(CurrentPageIndex.Subscribe(delegate(int i)
		{
			CurrentPage.Value = base.Pages?[i];
		}));
	}

	protected override void DisposeImplementation()
	{
	}
}
