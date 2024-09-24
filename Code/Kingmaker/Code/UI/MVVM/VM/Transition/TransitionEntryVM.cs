using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Transition;

public class TransitionEntryVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> Attention = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsInteractable = new ReactiveProperty<bool>();

	public readonly BlueprintMultiEntranceEntry Entry;

	public readonly Action CloseAction;

	public readonly Action ClickAction;

	public readonly bool IsCurrentlyLocation;

	public TransitionEntryVM(BlueprintMultiEntranceEntry entry, Action closeAction)
	{
		CloseAction = closeAction;
		ClickAction = Enter;
		Entry = entry;
		Name.Value = GetPointName();
		Attention.Value = entry.GetLinkedObjectives().Count > 0;
		IsVisible.Value = entry.IsVisible;
		IsInteractable.Value = entry.IsInteractable;
		IsCurrentlyLocation = entry.CheckCurrentlyEntryLocation();
	}

	protected override void DisposeImplementation()
	{
	}

	private string GetPointName()
	{
		ChangeTransitionPointName component = Entry.GetComponent<ChangeTransitionPointName>();
		if (component == null || !component.Conditions.HasConditions)
		{
			return Entry.Name;
		}
		if (component.Conditions.Check() && !string.IsNullOrWhiteSpace(component.AnotherName))
		{
			return component.AnotherName;
		}
		return Entry.Name;
	}

	public void Enter()
	{
		if (!UINetUtility.IsControlMainCharacterWithWarning())
		{
			return;
		}
		if (!IsInteractable.Value)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.Transition.TransitionIsUnavailable);
			});
			return;
		}
		CloseAction?.Invoke();
		if (Entry != null)
		{
			Game.Instance.GameCommandQueue.AreaTransition(Entry);
		}
	}

	public TooltipBaseTemplate GetTooltipTemplate()
	{
		return new TooltipTemplateTransitionEntry(Entry);
	}
}
