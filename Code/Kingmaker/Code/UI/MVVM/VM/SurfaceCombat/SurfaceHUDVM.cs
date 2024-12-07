using System;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.IngameMenu;
using Kingmaker.Code.UI.MVVM.VM.Party;
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
using Kingmaker.UI.MVVM.VM.CombatLog;
using Kingmaker.UI.MVVM.VM.Inspect;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;

public class SurfaceHUDVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IAreaActivationHandler, ISubscriber, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, IGameModeHandler, INetRoleSetHandler, INetEvents, IPartyCombatHandler
{
	public readonly ReactiveProperty<bool> IsTurnBasedActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> ShowEndTurn = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> CanEndTurn = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> DeploymentPhase = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> CanDeploy = new ReactiveProperty<bool>();

	public readonly BoolReactiveProperty PlayerHaveRoles = new BoolReactiveProperty();

	public readonly BoolReactiveProperty NetFirstLoadState = new BoolReactiveProperty();

	public readonly ReactiveProperty<InitiativeTrackerVM> InitiativeTrackerVM = new ReactiveProperty<InitiativeTrackerVM>();

	public readonly ReactiveProperty<SurfaceCombatUnitVM> CurrentUnit = new ReactiveProperty<SurfaceCombatUnitVM>();

	public readonly ReactiveProperty<CombatStartWindowVM> CombatStartWindowVM = new ReactiveProperty<CombatStartWindowVM>();

	public readonly InspectVM InspectVM;

	public readonly CombatLogVM CombatLogVM;

	public readonly IngameMenuVM IngameMenuVM;

	public readonly IngameMenuSettingsButtonVM IngameMenuSettingsButtonVM;

	public readonly PartyVM PartyVM;

	public readonly SurfaceActionBarVM ActionBarVM;

	private IDisposable m_TrackerSubscription;

	private BaseUnitEntity SingleSelectedUnit => Game.Instance.SelectionCharacter.SingleSelectedUnit.Value;

	public void EndTurn()
	{
		Game.Instance.TurnController.TryEndPlayerTurnManually();
	}

	public SurfaceHUDVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		UpdateIsTurnBasedActive();
		TurnBasedModeChanged(IsTurnBasedActive.Value);
		if (Game.Instance.TurnController.IsPreparationTurn)
		{
			HandleBeginPreparationTurn(Game.Instance.TurnController.IsDeploymentAllowed);
		}
		AddDisposable(MainThreadDispatcher.FrequentUpdateAsObservable().Subscribe(delegate
		{
			UpdateIsTurnBasedActive();
		}));
		AddDisposable(IsTurnBasedActive.Subscribe(TurnBasedModeChanged));
		AddDisposable(IngameMenuVM = new IngameMenuVM());
		AddDisposable(IngameMenuSettingsButtonVM = new IngameMenuSettingsButtonVM());
		AddDisposable(InspectVM = new InGameInspectVM());
		AddDisposable(CombatLogVM = new CombatLogVM());
		AddDisposable(PartyVM = new PartyVM());
		AddDisposable(ActionBarVM = new SurfaceActionBarVM(CurrentUnit));
		AddDisposable(Game.Instance.SelectionCharacter.SingleSelectedUnit.Subscribe(delegate
		{
			OnUnitChanged();
		}));
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	private void OnUnitChanged()
	{
		MechanicEntity mechanicEntity = ((TurnController.IsInTurnBasedCombat() && InitiativeTrackerVM.Value != null && !Game.Instance.TurnController.IsPreparationTurn) ? InitiativeTrackerVM.Value.CurrentUnit.Value.Unit : SingleSelectedUnit);
		if (mechanicEntity != CurrentUnit.Value?.Unit)
		{
			CurrentUnit.Value?.Dispose();
			CurrentUnit.Value = new SurfaceCombatUnitVM(mechanicEntity, isCurrent: true);
		}
	}

	private void TurnBasedModeChanged(bool state)
	{
		InitiativeTrackerVM.Value?.Dispose();
		m_TrackerSubscription?.Dispose();
		if (state)
		{
			InitiativeTrackerVM disposable = (InitiativeTrackerVM.Value = new InitiativeTrackerVM());
			AddDisposable(disposable);
			m_TrackerSubscription = InitiativeTrackerVM.Value.CurrentUnit.Subscribe(delegate
			{
				OnUnitChanged();
			});
		}
		else
		{
			InitiativeTrackerVM.Value = null;
			m_TrackerSubscription = null;
		}
	}

	protected override void DisposeImplementation()
	{
		HandleEndPreparationTurn();
		CurrentUnit.Value?.Dispose();
		CurrentUnit.Value = null;
	}

	private void UpdateIsTurnBasedActive()
	{
		if (!LoadingProcess.Instance.IsLoadingInProcess)
		{
			TurnController turnController = Game.Instance.TurnController;
			bool turnBasedModeActive = turnController.TurnBasedModeActive;
			if (turnBasedModeActive && Game.Instance.IsPaused)
			{
				Game.Instance.IsPaused = false;
			}
			IsTurnBasedActive.Value = turnBasedModeActive;
			ShowEndTurn.Value = turnController.IsPlayerTurn && turnController.CurrentUnit.IsMyNetRole();
			CanEndTurn.Value = turnController.CurrentUnit is BaseUnitEntity baseUnitEntity && baseUnitEntity.CombatState.CanEndTurn();
		}
	}

	public void OnAreaActivated()
	{
		UpdateIsTurnBasedActive();
		TurnBasedModeChanged(IsTurnBasedActive.Value);
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		CombatStartWindowVM.Value?.Dispose();
		CombatStartWindowVM.Value = null;
		CombatStartWindowVM.Value = new CombatStartWindowVM(Game.Instance.TurnController.RequestEndPreparationTurn, canDeploy);
		OnUnitChanged();
		DeploymentPhase.Value = true;
		CanDeploy.Value = canDeploy;
	}

	public void HandleEndPreparationTurn()
	{
		CombatStartWindowVM.Value?.Dispose();
		CombatStartWindowVM.Value = null;
		OnUnitChanged();
		DeploymentPhase.Value = false;
		CanDeploy.Value = false;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			HandleEndPreparationTurn();
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		if (Game.Instance.TurnController.IsPreparationTurn)
		{
			HandleBeginPreparationTurn(Game.Instance.TurnController.IsDeploymentAllowed);
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			HandleEndPreparationTurn();
		}
		OnUnitChanged();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene && Game.Instance.TurnController.IsPreparationTurn)
		{
			HandleBeginPreparationTurn(Game.Instance.TurnController.IsDeploymentAllowed);
		}
		OnUnitChanged();
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
		PlayerHaveRoles.Value = Game.Instance.CoopData.PlayerRole.PlayerContainsAnyRole(NetworkingManager.LocalNetPlayer);
	}

	public void HandleTransferProgressChanged(bool value)
	{
	}

	public void HandleNetStateChanged(LobbyNetManager.State state)
	{
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	public void HandleNetGameStateChanged(NetGame.State state)
	{
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	public void HandleNLoadingScreenClosed()
	{
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!(!DeploymentPhase.Value || inCombat))
		{
			Game.Instance.TurnController.RequestEndPreparationTurn();
		}
	}
}
