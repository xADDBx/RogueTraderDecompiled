using System;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Transition;

public class TransitionLegendButtonVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public bool IsVisible;

	public string Name;

	public readonly ReactiveProperty<bool> Attention = new ReactiveProperty<bool>();

	public Action CloseAction;

	public Action ClickAction;

	public Action HoverAction;

	public Action UnHoverAction;

	public readonly TransitionEntryVM TransitionEntryVM;

	public readonly ReactiveProperty<bool> IsHover = new ReactiveProperty<bool>();

	public TransitionLegendButtonVM(TransitionEntryVM transitionEntryVM, Action hoverAction, Action unHoverAction)
	{
		Attention.Value = transitionEntryVM.Attention.Value;
		IsVisible = transitionEntryVM.IsVisible.Value && transitionEntryVM.IsInteractable.Value;
		Name = transitionEntryVM.Name.Value;
		TransitionEntryVM = transitionEntryVM;
		CloseAction = transitionEntryVM.CloseAction;
		ClickAction = transitionEntryVM.ClickAction;
		HoverAction = hoverAction;
		UnHoverAction = unHoverAction;
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnClick()
	{
		if (UINetUtility.IsControlMainCharacterWithWarning())
		{
			ClickAction?.Invoke();
		}
	}
}
