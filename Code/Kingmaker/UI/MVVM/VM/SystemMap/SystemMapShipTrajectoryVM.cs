using System;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.SystemMap;

public class SystemMapShipTrajectoryVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber, IStarSystemShipMovementHandler, INetPingPosition
{
	public readonly ReactiveProperty<bool> IsSystemMap = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> ShipIsMoving = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<Vector3> ShowPingPosition = new ReactiveProperty<Vector3>();

	public SystemMapShipTrajectoryVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		OnGameModeStart(Game.Instance.CurrentMode);
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsSystemMap.Value = Game.Instance.CurrentMode == GameModeType.StarSystem;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		IsSystemMap.Value = Game.Instance.CurrentMode == GameModeType.StarSystem;
	}

	public void HandleStarSystemShipMovementStarted()
	{
		ShipIsMoving.Value = true;
	}

	public void HandleStarSystemShipMovementEnded()
	{
		ShipIsMoving.Value = false;
	}

	public void HandlePingPosition(NetPlayer player, Vector3 position)
	{
		ShowPingPosition.Value = position;
	}
}
