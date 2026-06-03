using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.TurnTimer;

public class TurnTimerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ITimerCounterUIHandler, ISubscriber, ITurnEndHandler, ISubscriber<IMechanicEntity>, IInterruptTurnEndHandler, IInterruptCurrentTurnHandler
{
	public readonly StringReactiveProperty Counter = new StringReactiveProperty();

	public readonly FloatReactiveProperty Progress = new FloatReactiveProperty(0f);

	public readonly BoolReactiveProperty IsShowing = new BoolReactiveProperty();

	public readonly ReactiveCommand CounterChanged = new ReactiveCommand();

	public readonly ReactiveCommand ProgressChanged = new ReactiveCommand();

	public readonly BoolReactiveProperty IsFiveSecsLeft = new BoolReactiveProperty();

	private readonly Dictionary<string, TimerShowCounterUIStruct> m_Timers = new Dictionary<string, TimerShowCounterUIStruct>();

	public TurnTimerVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(UpdateTimerValues));
	}

	protected override void DisposeImplementation()
	{
	}

	private void UpdateTimerValues()
	{
		if (m_Timers.Empty() || Game.Instance.CurrentMode == GameModeType.Cutscene || Game.Instance.TurnController.IsPreparationTurn)
		{
			IsFiveSecsLeft.Value = false;
			IsShowing.Value = false;
			return;
		}
		TimerShowCounterUIStruct timerShowCounterUIStruct = m_Timers.Values.Last();
		float timeLeft = timerShowCounterUIStruct.Timer.TimeLeft;
		double num = timerShowCounterUIStruct.Timer.Duration;
		int num2 = Mathf.FloorToInt(timeLeft);
		int num3 = Mathf.FloorToInt(timeLeft * 100f % 100f);
		IsFiveSecsLeft.Value = num2 <= 5;
		Counter.Value = $"{num2:00}:{num3:00}";
		CounterChanged.Execute();
		Progress.Value = (float)((double)timeLeft / num);
		ProgressChanged.Execute();
		IsShowing.Value = true;
	}

	public void ShowTimerCounter(TimerShowCounterUIStruct counterUIStruct)
	{
		m_Timers[counterUIStruct.Id] = counterUIStruct;
	}

	public void HideTimerCounter(string id)
	{
		m_Timers.Remove(id);
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		HideTimers();
	}

	public void HandleUnitEndInterruptTurn()
	{
		HideTimers();
	}

	public void HandleOnInterruptCurrentTurn()
	{
		HideTimers();
	}

	private void HideTimers()
	{
		m_Timers.Clear();
		IsShowing.Value = false;
		IsFiveSecsLeft.Value = false;
	}
}
