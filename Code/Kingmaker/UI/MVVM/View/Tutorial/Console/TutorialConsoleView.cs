using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tutorial.Console;

public class TutorialConsoleView : ViewBase<TutorialVM>
{
	[SerializeField]
	private UIDestroyViewLink<TutorialBigWindowConsoleView, TutorialModalWindowVM> m_BigWindowView;

	[SerializeField]
	private UIDestroyViewLink<TutorialSmallWindowConsoleView, TutorialHintWindowVM> m_SmallWindowView;

	public void Initialize()
	{
		HideAll();
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		AddDisposable(base.ViewModel.BigWindowVM.Subscribe(m_BigWindowView.Bind));
		AddDisposable(base.ViewModel.SmallWindowVM.Subscribe(m_SmallWindowView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
		HideAll();
	}

	public void HideAll()
	{
		base.gameObject.SetActive(value: false);
	}
}
