using System;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using UniRx;
using UnityEngine.Events;

namespace Owlcat.Runtime.UI.Controls.Other;

public static class OwlcatConsoleButtonExtensions
{
	public static IObservable<Unit> OnConfirmClickAsObservable(this OwlcatButton button)
	{
		ClickEvent confirmClickEvent = button.ConfirmClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)confirmClickEvent.AddListener, (Action<UnityAction>)confirmClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnLongConfirmClickAsObservable(this OwlcatButton button)
	{
		ClickEvent longConfirmClickEvent = button.LongConfirmClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)longConfirmClickEvent.AddListener, (Action<UnityAction>)longConfirmClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnFunc01ClickAsObservable(this OwlcatButton button)
	{
		ClickEvent func01ClickEvent = button.Func01ClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)func01ClickEvent.AddListener, (Action<UnityAction>)func01ClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnLongFunc01ClickAsObservable(this OwlcatButton button)
	{
		ClickEvent longFunc01ClickEvent = button.LongFunc01ClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)longFunc01ClickEvent.AddListener, (Action<UnityAction>)longFunc01ClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnFunc02ClickAsObservable(this OwlcatButton button)
	{
		ClickEvent func02ClickEvent = button.Func02ClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)func02ClickEvent.AddListener, (Action<UnityAction>)func02ClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnLongFunc02ClickAsObservable(this OwlcatButton button)
	{
		ClickEvent longFunc02ClickEvent = button.LongFunc02ClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)longFunc02ClickEvent.AddListener, (Action<UnityAction>)longFunc02ClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnConfirmClickAsObservable(this OwlcatMultiButton button)
	{
		ClickEvent confirmClickEvent = button.ConfirmClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)confirmClickEvent.AddListener, (Action<UnityAction>)confirmClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnLongConfirmClickAsObservable(this OwlcatMultiButton button)
	{
		ClickEvent longConfirmClickEvent = button.LongConfirmClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)longConfirmClickEvent.AddListener, (Action<UnityAction>)longConfirmClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnFunc01ClickAsObservable(this OwlcatMultiButton button)
	{
		ClickEvent func01ClickEvent = button.Func01ClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)func01ClickEvent.AddListener, (Action<UnityAction>)func01ClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnLongFunc01ClickAsObservable(this OwlcatMultiButton button)
	{
		ClickEvent longFunc01ClickEvent = button.LongFunc01ClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)longFunc01ClickEvent.AddListener, (Action<UnityAction>)longFunc01ClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnFunc02ClickAsObservable(this OwlcatMultiButton button)
	{
		ClickEvent func02ClickEvent = button.Func02ClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)func02ClickEvent.AddListener, (Action<UnityAction>)func02ClickEvent.RemoveListener);
	}

	public static IObservable<Unit> OnLongFunc02ClickAsObservable(this OwlcatMultiButton button)
	{
		ClickEvent longFunc02ClickEvent = button.LongFunc02ClickEvent;
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)longFunc02ClickEvent.AddListener, (Action<UnityAction>)longFunc02ClickEvent.RemoveListener);
	}

	public static IObservable<bool> OnFocusAsObservable(this OwlcatSelectable selectable)
	{
		if (selectable == null)
		{
			return Observable.Empty<bool>();
		}
		return selectable.OnFocus.AsObservable();
	}
}
