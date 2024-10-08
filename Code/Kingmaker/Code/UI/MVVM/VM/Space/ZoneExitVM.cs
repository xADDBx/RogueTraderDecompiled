using System;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Space;

public class ZoneExitVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber, INetRoleSetHandler, IStarSystemShipMovementHandler, IFullScreenUIHandler, INetStopPlayingHandler
{
	public readonly ReactiveProperty<bool> IsWarpJumpAvailable = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsExitAvailable = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveProperty<bool> ShipIsMoving = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> HasAccessStarshipInventory = new ReactiveProperty<bool>();

	private IDisposable m_UpdateDispatcher;

	private readonly string I_PUSHED_WARP_JUMP_BUTTON_BEFORE = "I_PUSHED_WARP_JUMP_BUTTON_BEFORE";

	public bool PushedWarpJumpBefore => PlayerPrefs.GetInt(I_PUSHED_WARP_JUMP_BUTTON_BEFORE, 0) == 1;

	public ZoneExitVM()
	{
		IsExitAvailable.Value = UINetUtility.IsControlMainCharacter();
		HasAccessStarshipInventory.Value = Game.Instance.Player.CanAccessStarshipInventory;
		AddDisposable(EventBus.Subscribe(this));
		HandleGameModeChanged();
	}

	protected override void DisposeImplementation()
	{
		m_UpdateDispatcher?.Dispose();
	}

	private void OnUpdateHandler()
	{
		IsWarpJumpAvailable.Value = Game.Instance.SectorMapController.CanJumpToWarp && IsExitAvailable.Value;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		HandleGameModeChanged();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		HandleGameModeChanged();
	}

	public void ExitToShip()
	{
		Game.Instance.SectorMapController.JumpToShipArea();
	}

	public void ForceOpenShipCustomization()
	{
		EventBus.RaiseEvent(delegate(IShipCustomizationForceUIHandler h)
		{
			h.HandleForceOpenShipCustomization();
		});
	}

	public void OpenShipCustomization()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenShipCustomization();
		});
	}

	public void ExitToWarp()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.StarSystem))
		{
			Game.Instance.SectorMapController.JumpToSectorMap();
			if (!PushedWarpJumpBefore)
			{
				PlayerPrefs.SetInt(I_PUSHED_WARP_JUMP_BUTTON_BEFORE, 1);
				PlayerPrefs.Save();
			}
		}
	}

	public void StopShip()
	{
		Game.Instance.GameCommandQueue.StopStarSystemStarShip();
	}

	private void HandleGameModeChanged()
	{
		m_UpdateDispatcher?.Dispose();
		HasAccessStarshipInventory.Value = Game.Instance.Player.CanAccessStarshipInventory;
		if (!(Game.Instance.CurrentMode != GameModeType.StarSystem) || !(Game.Instance.CurrentMode != GameModeType.GlobalMap))
		{
			m_UpdateDispatcher = MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
			{
				OnUpdateHandler();
			});
		}
	}

	public void HandleRoleSet(string entityId)
	{
		IsExitAvailable.Value = UINetUtility.IsControlMainCharacter();
	}

	public void HandleStopPlaying()
	{
		IsExitAvailable.Value = UINetUtility.IsControlMainCharacter();
	}

	public void HandleStarSystemShipMovementStarted()
	{
		ShipIsMoving.Value = true;
	}

	public void HandleStarSystemShipMovementEnded()
	{
		ShipIsMoving.Value = false;
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (state)
		{
			StopShip();
		}
	}
}
