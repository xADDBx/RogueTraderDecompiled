using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Transition;

public class TransitionLegendButtonVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly bool IsVisible;

	public readonly bool IsInteractable;

	public readonly string Name;

	public readonly ReactiveProperty<bool> Attention = new ReactiveProperty<bool>();

	private readonly Action m_ClickAction;

	public readonly Action HoverAction;

	public readonly Action UnHoverAction;

	public readonly TransitionEntryVM TransitionEntryVM;

	public readonly ReactiveProperty<bool> IsHover = new ReactiveProperty<bool>();

	public TransitionLegendButtonVM(TransitionEntryVM transitionEntryVM, Action hoverAction, Action unHoverAction)
	{
		Attention.Value = transitionEntryVM.Attention.Value;
		IsVisible = transitionEntryVM.IsVisible.Value && transitionEntryVM.IsInteractable.Value;
		IsInteractable = transitionEntryVM.IsInteractable.Value;
		Name = transitionEntryVM.Name.Value;
		TransitionEntryVM = transitionEntryVM;
		m_ClickAction = transitionEntryVM.ClickAction;
		HoverAction = hoverAction;
		UnHoverAction = unHoverAction;
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnClick()
	{
		if (!UINetUtility.IsControlMainCharacterWithWarning())
		{
			return;
		}
		if (!IsInteractable)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.Transition.TransitionIsUnavailable);
			});
		}
		else
		{
			m_ClickAction?.Invoke();
		}
	}
}
