using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Platforms;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Networking;

public class InviteNetManager : IDisposable
{
	public IPlatformInvite m_PlatformInvite;

	private CancellationTokenSource m_OperationCts;

	private Task m_OperationTask = Task.CompletedTask;

	public void InitPlatform()
	{
		if (m_PlatformInvite == null)
		{
			m_PlatformInvite = PlatformInviteFactory.Create();
			if (FirstLaunchSettingsVM.HasShown)
			{
				CheckAvailableInvite();
			}
		}
	}

	public void CheckAvailableInvite()
	{
		if (m_PlatformInvite.TryGetInviteRoom(out var roomServer, out var roomName))
		{
			EventBus.RaiseEvent(delegate(INetLobbyRequest h)
			{
				h.HandleNetLobbyRequest(isMainMenu: true);
			});
			AcceptInvite(roomServer, roomName);
		}
	}

	public void ShowInviteWindow()
	{
		m_PlatformInvite.ShowInviteWindow();
	}

	public void StartAnnounceGame()
	{
		m_PlatformInvite.StartAnnounceGame();
		if (PlatformServices.Platform.HasSecondaryPlatform && PlatformServices.Platform.SecondaryPlatform.IsInitialized())
		{
			PlatformServices.Platform.SecondaryPlatform.Invite.StartAnnounceGame();
		}
	}

	public void StopAnnounceGame()
	{
		m_PlatformInvite.StopAnnounceGame();
		if (PlatformServices.Platform.HasSecondaryPlatform && PlatformServices.Platform.SecondaryPlatform.IsInitialized())
		{
			PlatformServices.Platform.SecondaryPlatform.Invite.StopAnnounceGame();
		}
	}

	public void AcceptInvite(string roomServer, string roomName)
	{
		RunOperation("AcceptInvite", (CancellationToken token) => AcceptInvite(roomServer, roomName, token));
	}

	public void CreateNewLobby()
	{
		RunOperation("CreateNewLobby", (CancellationToken token) => CreateNewLobby(token));
	}

	private async Task AcceptInvite(string roomServer, string roomName, CancellationToken token)
	{
		PFLog.Net.Log("[Invite] Accepting invite started... s='" + roomServer + "' r='" + roomName + "'");
		if (await ShowAlertIfNeeded())
		{
			await PrepareFsmToLobby(token);
			await ChangeRegionIfNeeded(roomServer, token);
			PFLog.Net.Log("[Invite] Joining room " + roomName);
			PhotonManager.NetGame.Join(roomName);
		}
	}

	public async Task CreateNewLobby(CancellationToken token)
	{
		await PrepareFsmToLobby(token);
		PhotonManager.NetGame.CreateNewLobby();
		await PhotonManager.NetGame.WaitNextState(NetGame.State.InLobby, token);
	}

	private static async Task PrepareFsmToLobby(CancellationToken token)
	{
		NetGame game = PhotonManager.NetGame;
		PFLog.Net.Log($"[Invite] Waiting for stable room state, current={game.CurrentState}");
		await (await Task.WhenAny(game.WaitState(NetGame.State.PlatformNotInitialized, token), game.WaitState(NetGame.State.PlatformInitialized, token), game.WaitState(NetGame.State.NetInitialized, token), game.WaitState(NetGame.State.InLobby, token), game.WaitState(NetGame.State.Playing, token)));
		PFLog.Net.Log(string.Format("[Invite] Waiting for {0} state, current={1}", "NetInitialized", game.CurrentState));
		switch (game.CurrentState)
		{
		case NetGame.State.PlatformNotInitialized:
		case NetGame.State.PlatformInitialized:
			await PhotonManager.NetGame.StartNetGameIfNeededAsync(token);
			break;
		case NetGame.State.NetInitializing:
			await game.WaitNextState(NetGame.State.NetInitialized, token);
			break;
		case NetGame.State.InLobby:
		case NetGame.State.Playing:
			if (game.CurrentState == NetGame.State.InLobby || game.CurrentState == NetGame.State.Playing)
			{
				game.StopPlaying(shouldLeaveLobby: true, "InviteNetManager");
			}
			if (game.CurrentState == NetGame.State.InLobby || game.CurrentState == NetGame.State.Playing)
			{
				await game.WaitNextState(NetGame.State.StopPlaying, token);
			}
			if (game.CurrentState == NetGame.State.StopPlaying)
			{
				await game.WaitNextState(NetGame.State.NetInitialized, token);
			}
			break;
		default:
			throw new Exception($"Invalid state after finding stable state {game.CurrentState}");
		case NetGame.State.NetInitialized:
			break;
		}
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyRequest();
		});
		if (game.CurrentState != NetGame.State.NetInitialized)
		{
			throw new Exception($"Invalid state {game.CurrentState}");
		}
	}

	private static async Task<bool> ShowAlertIfNeeded()
	{
		if (PhotonManager.Initialized && PhotonManager.Instance.InRoom)
		{
			Task<bool> task;
			Action<bool> callback = Utils.CallbackToTask(out task);
			EventBus.RaiseEvent(delegate(INetInviteHandler h)
			{
				h.HandleInvite(callback);
			});
			if (!(await task))
			{
				PFLog.Net.Log("[Invite] Invite declined.");
				EventBus.RaiseEvent(delegate(INetInviteHandler h)
				{
					h.HandleInviteAccepted(accepted: false);
				});
				return false;
			}
			PFLog.Net.Log("[Invite] Invite accepted.");
			EventBus.RaiseEvent(delegate(INetInviteHandler h)
			{
				h.HandleInviteAccepted(accepted: true);
			});
		}
		return true;
	}

	private static async Task ChangeRegionIfNeeded([NotNull] string roomServer, CancellationToken token)
	{
		NetGame game = PhotonManager.NetGame;
		if (PhotonManager.Instance.Region == roomServer)
		{
			return;
		}
		PFLog.Net.Log("[Invite] Changing region from " + PhotonManager.Instance.Region + " to " + roomServer + ".");
		game.ChangeRegion(roomServer);
		if (!(PhotonManager.Instance.Region == roomServer) || game.CurrentState != NetGame.State.NetInitialized)
		{
			if (game.CurrentState == NetGame.State.NetInitialized)
			{
				await game.WaitNextState(NetGame.State.ChangingRegion, token);
			}
			if (game.CurrentState == NetGame.State.ChangingRegion)
			{
				await game.WaitNextState(NetGame.State.NetInitialized, token);
			}
		}
	}

	private async void RunOperation(string operationName, Func<CancellationToken, Task> run)
	{
		if (!m_OperationTask.IsCompleted)
		{
			PFLog.Net.Error($"[InviteNetManager.RunOperation] #{m_OperationTask.Id} Another operation already in progress, {operationName}");
			return;
		}
		try
		{
			m_OperationCts = new CancellationTokenSource();
			m_OperationTask = run(m_OperationCts.Token);
			PFLog.Net.Log($"[InviteNetManager.RunOperation] #{m_OperationTask.Id} Run {operationName}");
			await m_OperationTask;
		}
		catch (OperationCanceledException)
		{
			PFLog.Net.Log("[InviteNetManager.RunOperation] " + operationName + " cancelled");
		}
		catch (Exception ex2)
		{
			PFLog.Net.Exception(ex2);
		}
		finally
		{
			m_OperationCts.Dispose();
			m_OperationCts = null;
		}
	}

	public void Dispose()
	{
		m_PlatformInvite.Dispose();
	}
}
