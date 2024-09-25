using System;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CircleArc;

public class BaseCircleArcsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber
{
	public readonly ReactiveProperty<bool> IsCorrectGameMode = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> ShouldMoveArcs = new ReactiveProperty<bool>();

	protected GameModeType PreviousMode;

	protected BaseCircleArcsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		OnGameModeStart(Game.Instance.CurrentMode);
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		OnGameModeStartImpl(gameMode);
	}

	protected virtual void OnGameModeStartImpl(GameModeType gameMode)
	{
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		PreviousMode = gameMode;
		OnGameModeStopImpl(gameMode);
	}

	protected virtual void OnGameModeStopImpl(GameModeType gameMode)
	{
	}
}
