using System;
using Kingmaker.Assets.Controllers.GlobalMap;
using Kingmaker.Controllers;
using Kingmaker.Controllers.StarSystem;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SystemMap;

public class TimeRewindVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber
{
	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> TimeControlEnabled = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<float> TimeMultiplier = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<GameTimeState> TimeState = new ReactiveProperty<GameTimeState>();

	public readonly ReactiveProperty<float> CurrentSegment = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> CurrentVVYear = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> CurrentAMRCYear = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> CurrentMillenium = new ReactiveProperty<float>(0f);

	private IGameTimeController m_TimeController;

	public TimeRewindVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(MainThreadDispatcher.FrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
		OnGameModeStart(Game.Instance.CurrentMode);
	}

	protected override void DisposeImplementation()
	{
	}

	private void UpdateTimeController()
	{
		Game instance = Game.Instance;
		IGameTimeController timeController;
		if (!(instance?.CurrentMode == GameModeType.GlobalMap))
		{
			IGameTimeController gameTimeController = instance?.GetController<StarSystemTimeController>();
			timeController = gameTimeController;
		}
		else
		{
			IGameTimeController gameTimeController = instance?.GetController<SectorMapTimeController>();
			timeController = gameTimeController;
		}
		m_TimeController = timeController;
	}

	private void OnUpdateHandler()
	{
		Game instance = Game.Instance;
		if (m_TimeController == null || instance == null)
		{
			return;
		}
		IGameTimeController timeController = m_TimeController;
		if (timeController == null)
		{
			return;
		}
		_ = timeController.TimeState;
		if (0 == 0)
		{
			TimeMultiplier.Value = instance.TimeController?.GameTimeScale ?? 1f;
			TimeState.Value = m_TimeController?.TimeState ?? GameTimeState.Normal;
			ReactiveProperty<bool> timeControlEnabled = TimeControlEnabled;
			ReactiveProperty<GameTimeState> timeState = TimeState;
			timeControlEnabled.Value = (timeState != null && timeState.Value == GameTimeState.Paused) || instance.Player?.PlayerShip?.Commands?.CurrentMoveTo == null;
			if (instance.Player != null)
			{
				CurrentSegment.Value = instance.Player.GetSegments();
				CurrentVVYear.Value = instance.Player.GetVVYears();
				CurrentAMRCYear.Value = instance.Player.GetAMRCYears();
				CurrentMillenium.Value = instance.Player.GetMillennium();
			}
		}
	}

	public void PauseTime()
	{
		SetGameTimeState(GameTimeState.Paused);
	}

	public void ResumeTime()
	{
		SetGameTimeState(GameTimeState.Normal);
	}

	public void RewindTime()
	{
		SetGameTimeState(GameTimeState.Fast);
	}

	private void SetGameTimeState(GameTimeState state)
	{
		m_TimeController?.SetState(state);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		UpdateTimeController();
		ShouldShow.Value = Game.Instance.CurrentMode == GameModeType.StarSystem || Game.Instance.CurrentMode == GameModeType.GlobalMap;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		UpdateTimeController();
	}
}
