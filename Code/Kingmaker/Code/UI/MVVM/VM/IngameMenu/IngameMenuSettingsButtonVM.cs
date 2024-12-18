using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.IngameMenu;

public class IngameMenuSettingsButtonVM : IngameMenuBaseVM, INetRoleSetHandler, ISubscriber, INetEvents, IPauseHandler, IAreaHandler, IPartyCombatHandler
{
	public readonly BoolReactiveProperty PlayerHaveRoles = new BoolReactiveProperty();

	public readonly BoolReactiveProperty NetFirstLoadState = new BoolReactiveProperty();

	public readonly BoolReactiveProperty ShowPauseButton = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsPause = new BoolReactiveProperty();

	private bool IsNotServiceWindow => Game.Instance.RootUiContext.CurrentServiceWindow == ServiceWindowsType.None;

	private bool IsExplorationWindow => Game.Instance.RootUiContext.IsExplorationWindow;

	public IngameMenuSettingsButtonVM()
	{
		ShowPauseButton.Value = !RootUIContext.Instance.IsSpace && !Game.Instance.Player.IsInCombat;
		IsPause.Value = Game.Instance.IsPaused && !Game.Instance.PauseController.IsPausedByPlayers;
		HandleRoleSet(string.Empty);
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive;
	}

	protected override void DisposeImplementation()
	{
	}

	public void OpenEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void Pause()
	{
		Game.Instance.PauseBind();
	}

	public void OpenNetRoles()
	{
		EventBus.RaiseEvent(delegate(INetRolesRequest h)
		{
			h.HandleNetRolesRequest();
		});
	}

	protected override void UpdateHandler()
	{
		if (Game.Instance.CurrentlyLoadedArea != null)
		{
			ShouldShow.Value = base.IsAppropriateGameMode && IsNotServiceWindow && !IsExplorationWindow && (ShouldShow.Value || Game.Instance.CurrentMode != GameModeType.SpaceCombat);
		}
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

	public void OnPauseToggled()
	{
		IsPause.Value = Game.Instance.IsPaused && !Game.Instance.PauseController.IsPausedByPlayers;
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		ShowPauseButton.Value = !RootUIContext.Instance.IsSpace && !Game.Instance.Player.IsInCombat;
		IsPause.Value = Game.Instance.IsPaused && !Game.Instance.PauseController.IsPausedByPlayers;
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		ShowPauseButton.Value = !RootUIContext.Instance.IsSpace && !inCombat;
	}
}
