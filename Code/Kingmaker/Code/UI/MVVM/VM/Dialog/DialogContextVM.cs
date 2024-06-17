using System;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Epilog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Interchapter;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog;

public class DialogContextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IDialogInteractionHandler, ISubscriber, IInterchapterHandler, IAreaHandler
{
	public readonly ReactiveProperty<BookEventVM> BookEventVM = new ReactiveProperty<BookEventVM>();

	public readonly ReactiveProperty<EpilogVM> EpilogVM = new ReactiveProperty<EpilogVM>();

	public readonly ReactiveProperty<DialogVM> DialogVM = new ReactiveProperty<DialogVM>();

	public readonly ReactiveProperty<InterchapterVM> InterchapterVM = new ReactiveProperty<InterchapterVM>();

	private bool m_IsAnyColonyScreenOpened;

	private static DialogType State => Game.Instance.DialogController.Dialog.Type;

	public DialogContextVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		DisposeDialog();
		DisposeBookEvent();
		DisposeEpilog();
		DisposeInterchapter();
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
		switch (dialog.Type)
		{
		case DialogType.Common:
		{
			DialogVM disposable = (DialogVM.Value = new DialogVM());
			AddDisposable(disposable);
			break;
		}
		case DialogType.Book:
		{
			BookEventVM disposable3 = (BookEventVM.Value = new BookEventVM());
			AddDisposable(disposable3);
			break;
		}
		case DialogType.Epilog:
		{
			EpilogVM disposable2 = (EpilogVM.Value = new EpilogVM());
			AddDisposable(disposable2);
			break;
		}
		case DialogType.StarSystemEvent:
		{
			DialogVM disposable = (DialogVM.Value = new DialogVM());
			AddDisposable(disposable);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void StartInterchapter(InterchapterData data)
	{
		InterchapterVM.Value = new InterchapterVM(data);
	}

	public void StopInterchapter(InterchapterData data)
	{
		InterchapterVM.Value?.Dispose();
		InterchapterVM.Value = null;
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
		DisposeImplementation();
	}

	private void DisposeBookEvent()
	{
		BookEventVM.Value?.Dispose();
		BookEventVM.Value = null;
	}

	private void DisposeEpilog()
	{
		EpilogVM.Value?.Dispose();
		EpilogVM.Value = null;
	}

	private void DisposeInterchapter()
	{
		InterchapterVM.Value?.Dispose();
		InterchapterVM.Value = null;
	}

	private void DisposeDialog()
	{
		DialogVM.Value?.Dispose();
		DialogVM.Value = null;
	}

	public void OnAreaBeginUnloading()
	{
		DisposeImplementation();
	}

	public void OnAreaDidLoad()
	{
	}
}
