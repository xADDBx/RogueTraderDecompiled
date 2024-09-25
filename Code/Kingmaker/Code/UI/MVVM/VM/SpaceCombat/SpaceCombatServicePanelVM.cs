using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.TimeSurvival;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat;

public class SpaceCombatServicePanelVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetRoleSetHandler, ISubscriber, INetEvents
{
	public readonly ReactiveProperty<float> ShipHealthRatio = new ReactiveProperty<float>();

	public readonly ReactiveProperty<string> ShipHealthText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> RoundsLeft = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> IsTurnBasedActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsPlayerTurn = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsTorpedoesTurn = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsInTimeSurvival = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> CanEndTurn = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsAvailable = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<int> CombatActionsCount = new ReactiveProperty<int>();

	public readonly ReactiveProperty<InitiativeTrackerVM> InitiativeTrackerVM = new ReactiveProperty<InitiativeTrackerVM>();

	public readonly ReactiveProperty<SurfaceCombatUnitVM> SelectedUnitVM = new ReactiveProperty<SurfaceCombatUnitVM>();

	public readonly BoolReactiveProperty PlayerHaveRoles = new BoolReactiveProperty();

	public readonly BoolReactiveProperty NetFirstLoadState = new BoolReactiveProperty();

	private readonly List<ISpaceCombatActionsHolder> m_ActionsHolders;

	private static bool CanEndTurnAndNoActing
	{
		get
		{
			if (Game.Instance.TurnController.CurrentUnit is BaseUnitEntity { IsDirectlyControllable: not false } baseUnitEntity)
			{
				return baseUnitEntity.Commands.Empty;
			}
			return false;
		}
	}

	public void EndTurn()
	{
		Game.Instance.TurnController.TryEndPlayerTurnManually();
	}

	public SpaceCombatServicePanelVM(IEnumerable<ISpaceCombatActionsHolder> actionsHolders)
	{
		IsInTimeSurvival.Value = false;
		m_ActionsHolders = new List<ISpaceCombatActionsHolder>(actionsHolders);
		IsAvailable.Value = UINetUtility.IsControlMainCharacter();
		AddDisposable(IsTurnBasedActive.Subscribe(delegate(bool s)
		{
			TurnBasedModeChanged(s);
		}));
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			UpdateHandler();
		}));
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive && Game.Instance.CurrentMode == GameModeType.SpaceCombat;
	}

	private void TurnBasedModeChanged(bool state)
	{
		InitiativeTrackerVM.Value?.Dispose();
		if (state)
		{
			InitiativeTrackerVM disposable = (InitiativeTrackerVM.Value = new InitiativeTrackerVM());
			AddDisposable(disposable);
		}
		else
		{
			InitiativeTrackerVM.Value = null;
		}
	}

	protected override void DisposeImplementation()
	{
	}

	private void UpdateHandler()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		ShipHealthRatio.Value = (float)playerShip.Health.HitPointsLeft / (float)playerShip.Health.MaxHitPoints;
		ShipHealthText.Value = $"<color=#73BE53>{playerShip.Health.HitPointsLeft}</color>/{playerShip.Health.MaxHitPoints}";
		TimeSurvival component = Game.Instance.CurrentlyLoadedArea.GetComponent<TimeSurvival>();
		IsInTimeSurvival.Value = component != null && !component.UnlimitedTime;
		if (IsInTimeSurvival.Value)
		{
			RoundsLeft.Value = component?.RoundsLeft.ToString();
		}
		TurnController turnController = Game.Instance.TurnController;
		IsTurnBasedActive.Value = turnController.TurnBasedModeActive;
		IsPlayerTurn.Value = turnController.IsPlayerTurn && turnController.CurrentUnit == Game.Instance.Player.PlayerShip;
		IsTorpedoesTurn.Value = turnController.IsPlayerTurn && turnController.CurrentUnit != Game.Instance.Player.PlayerShip;
		CanEndTurn.Value = UIUtility.GetCurrentSelectedUnit()?.CombatState.CanEndTurn() ?? false;
		UpdateIsTurnBasedActive();
		if (CanEndTurnAndNoActing)
		{
			CombatActionsCount.Value = m_ActionsHolders.Count((ISpaceCombatActionsHolder holder) => holder.HasActions());
		}
	}

	public void HighlightOn()
	{
		m_ActionsHolders.Where((ISpaceCombatActionsHolder holder) => holder.HasActions()).ForEach(delegate(ISpaceCombatActionsHolder holder)
		{
			holder.HighlightOn();
		});
	}

	public void HighlightOff()
	{
		m_ActionsHolders.ForEach(delegate(ISpaceCombatActionsHolder holder)
		{
			holder.HighlightOff();
		});
	}

	private void UpdateIsTurnBasedActive()
	{
		if (!LoadingProcess.Instance.IsLoadingInProcess)
		{
			IsTurnBasedActive.Value = Game.Instance.TurnController.TurnBasedModeActive;
		}
	}

	public void OpenNetRoles()
	{
		EventBus.RaiseEvent(delegate(INetRolesRequest h)
		{
			h.HandleNetRolesRequest();
		});
	}

	public void HandleRoleSet(string entityId)
	{
		IsAvailable.Value = UINetUtility.IsControlMainCharacter();
		PlayerHaveRoles.Value = Game.Instance.CoopData.PlayerRole.PlayerContainsAnyRole(NetworkingManager.LocalNetPlayer);
	}

	public void HandleTransferProgressChanged(bool value)
	{
	}

	public void HandleNetStateChanged(LobbyNetManager.State state)
	{
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive && Game.Instance.CurrentMode == GameModeType.SpaceCombat;
	}

	public void HandleNetGameStateChanged(NetGame.State state)
	{
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive && Game.Instance.CurrentMode == GameModeType.SpaceCombat;
	}

	public void HandleNLoadingScreenClosed()
	{
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive && Game.Instance.CurrentMode == GameModeType.SpaceCombat;
	}
}
