using System;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Epilog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Interchapter;
using Kingmaker.Code.UI.MVVM.VM.Dialog.RewardWindows;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog;

public class DialogContextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IDialogInteractionHandler, ISubscriber, IInterchapterHandler, IAreaHandler, ISoulMarkShiftRewardHandler
{
	public readonly ReactiveProperty<BookEventVM> BookEventVM = new ReactiveProperty<BookEventVM>();

	public readonly ReactiveProperty<EpilogVM> EpilogVM = new ReactiveProperty<EpilogVM>();

	public readonly ReactiveProperty<DialogVM> DialogVM = new ReactiveProperty<DialogVM>();

	public readonly ReactiveProperty<InterchapterVM> InterchapterVM = new ReactiveProperty<InterchapterVM>();

	public readonly ReactiveProperty<SoulMarkRewardVM> SoulMarkRewardVM = new ReactiveProperty<SoulMarkRewardVM>();

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
		DisposeSoulMarkReward();
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

	private void DisposeSoulMarkReward()
	{
		SoulMarkRewardVM.Value?.Dispose();
		SoulMarkRewardVM.Value = null;
	}

	public void OnAreaBeginUnloading()
	{
		DisposeImplementation();
	}

	public void OnAreaDidLoad()
	{
	}

	public void HandleSoulMarkShift(SoulMarkShift shift)
	{
		SoulMarkDirection direction = shift.Direction;
		int rank = SoulMarkShiftExtension.GetSoulMark(direction).Rank;
		int fillValue = rank - shift.Value;
		int soulMarkRankIndex = SoulMarkShiftExtension.GetSoulMarkRankIndex(direction, rank);
		int soulMarkRankIndex2 = SoulMarkShiftExtension.GetSoulMarkRankIndex(direction, fillValue);
		if (soulMarkRankIndex > soulMarkRankIndex2)
		{
			SoulMarkRewardVM.Value = new SoulMarkRewardVM(direction, soulMarkRankIndex, DisposeSoulMarkReward);
		}
	}
}
