using Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent.Console;
using Kingmaker.Code.UI.MVVM.View.Dialog.Epilog;
using Kingmaker.Code.UI.MVVM.View.Dialog.Interchapter;
using Kingmaker.Code.UI.MVVM.VM.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Epilog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Interchapter;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog.Console;

public class DialogContextConsoleView : ViewBase<DialogContextVM>
{
	[SerializeField]
	private UIViewLink<BookEventConsoleView, BookEventVM> m_BookEventConsoleView;

	[SerializeField]
	private UIViewLink<EpilogBaseView, EpilogVM> m_EpilogConsoleView;

	[SerializeField]
	private UIViewLink<SurfaceDialogConsoleView, DialogVM> m_DialogConsoleView;

	[SerializeField]
	private UIViewLink<InterchapterBaseView, InterchapterVM> m_InterchapterView;

	protected override void BindViewImplementation()
	{
		if (m_BookEventConsoleView != null)
		{
			AddDisposable(base.ViewModel.BookEventVM.Subscribe(m_BookEventConsoleView.Bind));
		}
		if (m_DialogConsoleView != null)
		{
			AddDisposable(base.ViewModel.DialogVM.Subscribe(m_DialogConsoleView.Bind));
		}
		if (m_EpilogConsoleView != null)
		{
			AddDisposable(base.ViewModel.EpilogVM.Subscribe(m_EpilogConsoleView.Bind));
		}
		if (m_InterchapterView != null)
		{
			AddDisposable(base.ViewModel.InterchapterVM.Subscribe(m_InterchapterView.Bind));
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
