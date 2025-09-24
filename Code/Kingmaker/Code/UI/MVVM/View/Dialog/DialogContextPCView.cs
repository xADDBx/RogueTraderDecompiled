using Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent;
using Kingmaker.Code.UI.MVVM.View.Dialog.Epilog;
using Kingmaker.Code.UI.MVVM.View.Dialog.Interchapter;
using Kingmaker.Code.UI.MVVM.View.Dialog.SurfaceDialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Epilog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Interchapter;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog;

public class DialogContextPCView : ViewBase<DialogContextVM>
{
	[SerializeField]
	private UIDestroyViewLink<BookEventPCView, BookEventVM> m_BookEventPCView;

	[SerializeField]
	private UIDestroyViewLink<EpilogPCView, EpilogVM> m_EpilogPCView;

	[SerializeField]
	private UIDestroyViewLink<SurfaceDialogPCView, DialogVM> m_DialogPCView;

	[SerializeField]
	private UIDestroyViewLink<InterchapterBaseView, InterchapterVM> m_InterchapterView;

	protected override void BindViewImplementation()
	{
		if (m_BookEventPCView != null)
		{
			AddDisposable(base.ViewModel.BookEventVM.Subscribe(m_BookEventPCView.Bind));
		}
		if (m_EpilogPCView != null)
		{
			AddDisposable(base.ViewModel.EpilogVM.Subscribe(m_EpilogPCView.Bind));
		}
		if (m_DialogPCView != null)
		{
			AddDisposable(base.ViewModel.DialogVM.Subscribe(m_DialogPCView.Bind));
		}
		if (m_InterchapterView != null)
		{
			AddDisposable(base.ViewModel.InterchapterVM.Subscribe(m_InterchapterView.Bind));
		}
		base.ViewModel.ToggleDialogFadeCommand.Subscribe(ToggleFade);
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void ToggleFade(bool value)
	{
		m_DialogPCView.ViewInstance?.ToggleFade(value);
	}
}
