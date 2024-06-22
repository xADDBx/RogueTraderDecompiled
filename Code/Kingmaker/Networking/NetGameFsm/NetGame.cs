using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking.NetGameFsm.States;
using Kingmaker.Networking.Platforms.Session;
using Kingmaker.Networking.Tools;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Fsm;

namespace Kingmaker.Networking.NetGameFsm;

public class NetGame : INetGame
{
	public enum State
	{
		PlatformNotInitialized,
		PlatformInitializing,
		PlatformInitialized,
		NetInitializing,
		NetInitialized,
		ChangingRegion,
		CreatingLobby,
		JoiningLobby,
		InLobby,
		UploadSaveAndStartLoading,
		DownloadSaveAndLoading,
		Playing,
		StopPlaying
	}

	public enum Trigger
	{
		ToPlatformInitializing,
		ToPlatformInitialized,
		ToPlatformNotInitialized,
		ToNetInitialization,
		ToNetInitialized,
		ChangeRegion,
		ChangeRegionFailed,
		ChangeRegionSuccess,
		CreateLobby,
		JoinLobby,
		EnterLobbyFailed,
		EnterLobbySuccess,
		UploadSaveAndStart,
		ToStartPlaying,
		ToDownloadSaveAndLoading,
		ToStopPlaying,
		NoConnection,
		NoActiveLobby,
		BackToLobby
	}

	private readonly StateMachine<State, Trigger> m_StateMachine;

	private const string StartStateDrawInfo = "shape=diamond style=filled fillcolor=gray";

	private const string MajorStateDrawInfo = "shape=box style=filled fillcolor=yellow";

	private const string LobbyCreationStateDrawInfo = "style=filled fillcolor=peachpuff1";

	private const string StartGameStateDrawInfo = "style=filled fillcolor=skyblue1";

	private const string GreenEdgeDrawInfo = "color=\"green3\" style=\"bold\"";

	public NetGameEventsHandler EventsHandler { get; }

	public State CurrentState => m_StateMachine.CurrentState;

	public bool NetRolesShowed { get; set; } = true;


	public JoinableUserTypes CurrentJoinableUserType { get; set; }

	public InvitableUserTypes CurrentInvitableUserType { get; set; }

	public NetGame()
	{
		m_StateMachine = InitStateMachine(this);
		EventsHandler = new NetGameEventsHandler();
		m_StateMachine.SetEventsHandler(EventsHandler);
		m_StateMachine.Start(State.PlatformNotInitialized);
		new NetSaveUploadDownloadErrorHandler();
	}

	private static StateMachine<State, Trigger> InitStateMachine(INetGame netGame)
	{
		StateMachine<State, Trigger> stateMachine = new StateMachine<State, Trigger>();
		stateMachine.Configure(State.PlatformNotInitialized, "shape=diamond style=filled fillcolor=gray").Permit(Trigger.ToPlatformInitializing, State.PlatformInitializing, "color=\"green3\" style=\"bold\"");
		stateMachine.Configure(State.PlatformInitializing).Permit(Trigger.ToPlatformNotInitialized, State.PlatformNotInitialized).Permit(Trigger.ToPlatformInitialized, State.PlatformInitialized, "color=\"green3\" style=\"bold\"")
			.Ignore(Trigger.ToPlatformInitializing)
			.SetStateFactory((object _) => new PlatformInitializingState(netGame));
		stateMachine.Configure(State.PlatformInitialized).Permit(Trigger.ToNetInitialization, State.NetInitializing, "color=\"green3\" style=\"bold\"").Ignore(Trigger.ToPlatformInitializing);
		stateMachine.Configure(State.NetInitializing).Permit(Trigger.ToNetInitialized, State.NetInitialized, "color=\"green3\" style=\"bold\"").Permit(Trigger.ToPlatformInitialized, State.PlatformInitialized)
			.Permit(Trigger.ToPlatformNotInitialized, State.PlatformNotInitialized)
			.Ignore(Trigger.ChangeRegion)
			.SetStateFactory((object _) => new NetInitializingState(netGame));
		stateMachine.Configure(State.NetInitialized, "shape=box style=filled fillcolor=yellow").Permit(Trigger.CreateLobby, State.CreatingLobby, "color=\"green3\" style=\"bold\"").Permit(Trigger.JoinLobby, State.JoiningLobby, "color=\"green3\" style=\"bold\"")
			.Permit(Trigger.ToStopPlaying, State.StopPlaying)
			.Permit(Trigger.ChangeRegion, State.ChangingRegion);
		stateMachine.Configure(State.ChangingRegion).Permit(Trigger.ChangeRegionSuccess, State.NetInitialized, "color=\"green3\" style=\"bold\"").Permit(Trigger.ChangeRegionFailed, State.PlatformInitialized)
			.Ignore(Trigger.ToStopPlaying)
			.SetStateFactory((object newRegion) => new ChangingRegionState(newRegion as string, netGame, PhotonManager.Instance));
		stateMachine.Configure(State.CreatingLobby, "style=filled fillcolor=peachpuff1").Permit(Trigger.EnterLobbyFailed, State.StopPlaying).Permit(Trigger.ToStopPlaying, State.StopPlaying)
			.Permit(Trigger.EnterLobbySuccess, State.InLobby, "color=\"green3\" style=\"bold\"")
			.SetStateFactory((object _) => new CreatingLobbyState(netGame, PhotonManager.Instance));
		stateMachine.Configure(State.JoiningLobby, "style=filled fillcolor=peachpuff1").Permit(Trigger.EnterLobbyFailed, State.StopPlaying).Permit(Trigger.ToStopPlaying, State.StopPlaying)
			.Permit(Trigger.EnterLobbySuccess, State.InLobby, "color=\"green3\" style=\"bold\"")
			.SetStateFactory((object payload) => new JoiningLobbyState(payload as string, netGame));
		stateMachine.Configure(State.StopPlaying).Permit(Trigger.NoConnection, State.PlatformInitialized).Permit(Trigger.NoActiveLobby, State.NetInitialized)
			.Permit(Trigger.BackToLobby, State.InLobby)
			.Ignore(Trigger.ToStopPlaying)
			.SetStateFactory((object shouldLeaveLobby) => new StopPlayingState((bool)shouldLeaveLobby, netGame, PhotonManager.Instance));
		stateMachine.Configure(State.InLobby, "shape=box style=filled fillcolor=yellow").Permit(Trigger.UploadSaveAndStart, State.UploadSaveAndStartLoading, "color=\"green3\" style=\"bold\"").Permit(Trigger.ToDownloadSaveAndLoading, State.DownloadSaveAndLoading, "color=\"green3\" style=\"bold\"")
			.Permit(Trigger.ToStopPlaying, State.StopPlaying)
			.SetStateFactory((object _) => new InLobbyState(PhotonManager.Instance));
		stateMachine.Configure(State.UploadSaveAndStartLoading, "style=filled fillcolor=skyblue1").Permit(Trigger.ToStartPlaying, State.Playing, "color=\"green3\" style=\"bold\"").Permit(Trigger.ToStopPlaying, State.StopPlaying)
			.Permit(Trigger.ToDownloadSaveAndLoading, State.DownloadSaveAndLoading)
			.Ignore(Trigger.ToPlatformInitializing)
			.SetStateFactory((object payload) => new UploadAndStartLoadingState(payload as UploadAndStartLoadingState.Args, netGame));
		stateMachine.Configure(State.DownloadSaveAndLoading, "style=filled fillcolor=skyblue1").Permit(Trigger.ToStartPlaying, State.Playing, "color=\"green3\" style=\"bold\"").Permit(Trigger.ToStopPlaying, State.StopPlaying)
			.Permit(Trigger.ToDownloadSaveAndLoading, State.DownloadSaveAndLoading)
			.Ignore(Trigger.ToPlatformInitializing)
			.SetStateFactory((object args) => new DownloadSaveAndLoadingState((DownloadSaveAndLoadingState.Args)args, netGame));
		stateMachine.Configure(State.Playing, "shape=box style=filled fillcolor=yellow").Permit(Trigger.ToDownloadSaveAndLoading, State.DownloadSaveAndLoading).Permit(Trigger.UploadSaveAndStart, State.UploadSaveAndStartLoading)
			.Permit(Trigger.ToStopPlaying, State.StopPlaying)
			.SetStateFactory((object _) => new PlayingState(PhotonManager.Instance));
		return stateMachine;
	}

	public Task WaitState(State waitState, CancellationToken cancellationToken = default(CancellationToken))
	{
		return m_StateMachine.WaitState(waitState, cancellationToken);
	}

	public Task WaitNextState(State nextState, CancellationToken cancellationToken = default(CancellationToken))
	{
		return m_StateMachine.WaitNextState(nextState, cancellationToken);
	}

	public void InitPlatform()
	{
		m_StateMachine.Fire(Trigger.ToPlatformInitializing);
	}

	public void OnPlatformInitialized()
	{
		m_StateMachine.Fire(Trigger.ToPlatformInitialized);
	}

	public void OnPlatformInitializeFailed()
	{
		m_StateMachine.Fire(Trigger.ToPlatformNotInitialized);
	}

	public void InitNetSystem()
	{
		m_StateMachine.Fire(Trigger.ToNetInitialization);
	}

	public void OnNetSystemInitialized()
	{
		m_StateMachine.Fire(Trigger.ToNetInitialized);
	}

	public void OnNetSystemInitializeFailed(bool storeNotInitialized)
	{
		m_StateMachine.Fire((!storeNotInitialized) ? Trigger.ToPlatformInitialized : Trigger.ToPlatformNotInitialized);
	}

	public void CreateNewLobby()
	{
		m_StateMachine.Fire(Trigger.CreateLobby);
	}

	public void OnLobbyCreated()
	{
		m_StateMachine.Fire(Trigger.EnterLobbySuccess);
	}

	public void OnLobbyCreationFailed()
	{
		StopPlaying(shouldLeaveLobby: true, "OnLobbyCreationFailed");
	}

	public void Join(string roomName)
	{
		m_StateMachine.Fire(Trigger.JoinLobby, roomName);
	}

	public void OnLobbyJoined()
	{
		m_StateMachine.Fire(Trigger.EnterLobbySuccess);
	}

	public void OnLobbyJoinFailed()
	{
		StopPlaying(shouldLeaveLobby: true, "OnLobbyJoinFailed");
	}

	public void OnSaveReceived(PhotonActorNumber saveFromPlayer)
	{
		PFLog.Net.Log($"[NetGame.OnSaveReceived] from {saveFromPlayer}, state={CurrentState}");
		bool flag = true;
		bool flag2 = false;
		if (CurrentState == State.UploadSaveAndStartLoading)
		{
			flag = PhotonManager.Instance.LocalClientId > saveFromPlayer.ActorNumber;
			flag2 = true;
			PFLog.Net.Log($"[NetGame.OnSaveReceived] upload will be stopped, because get save from {saveFromPlayer}");
		}
		else if (CurrentState == State.DownloadSaveAndLoading)
		{
			flag = PhotonManager.Save.SaveFromPlayer.ActorNumber > saveFromPlayer.ActorNumber;
			flag2 = true;
			PFLog.Net.Log($"[NetGame.OnSaveReceived] download will be stopped, because get save from {saveFromPlayer}");
		}
		if (flag)
		{
			FakeLoadingProcessCoroutine transitionLoadingProcess = null;
			if (flag2)
			{
				LoadingProcess.Instance.StopAll();
				transitionLoadingProcess = new FakeLoadingProcessCoroutine();
			}
			m_StateMachine.Fire(Trigger.ToDownloadSaveAndLoading, new DownloadSaveAndLoadingState.Args(saveFromPlayer, transitionLoadingProcess));
		}
		else
		{
			PFLog.Net.Log($"[NetGame.OnSaveReceived] ignore save from {saveFromPlayer}");
		}
	}

	public bool StartGame(SaveInfoKey saveInfoKey, [CanBeNull] Action callback = null)
	{
		if (!PhotonManager.Instance.IsEnoughPlayersForGame)
		{
			EventBus.RaiseEvent(delegate(INetGameStartHandler h)
			{
				h.HandleStartGameFailed();
			});
			return false;
		}
		m_StateMachine.Fire(Trigger.UploadSaveAndStart, new UploadAndStartLoadingState.Args(saveInfoKey, callback));
		NetRolesShowed = !PhotonManager.Instance.IsRoomOwner;
		return true;
	}

	public bool StartGameWithoutSave()
	{
		return StartGame(default(SaveInfoKey));
	}

	public void StartPlaying()
	{
		m_StateMachine.Fire(Trigger.ToStartPlaying);
	}

	public void StopPlaying(bool shouldLeaveLobby, string reason)
	{
		PFLog.Net.Log($"[NetGame.StopPlaying] shouldLeave={shouldLeaveLobby}, reason={reason}");
		m_StateMachine.Fire(Trigger.ToStopPlaying, shouldLeaveLobby);
	}

	public void OnPlayingStopped()
	{
		if (PhotonManager.Instance.InRoom)
		{
			m_StateMachine.Fire(Trigger.BackToLobby);
		}
		else if (PhotonManager.Instance.IsConnected)
		{
			m_StateMachine.Fire(Trigger.NoActiveLobby);
		}
		else
		{
			m_StateMachine.Fire(Trigger.NoConnection);
		}
	}

	public void ChangeRegion([NotNull] string region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		if (!region.Equals(PhotonManager.Instance.Region, StringComparison.Ordinal))
		{
			m_StateMachine.Fire(Trigger.ChangeRegion, region);
		}
		else
		{
			PFLog.Net.Log("[NetGame.ChangeRegion] Already connected to region '" + region + "'");
		}
	}

	public void OnRegionChanged()
	{
		m_StateMachine.Fire(Trigger.ChangeRegionSuccess);
	}

	public void OnRegionChangeFailed()
	{
		m_StateMachine.Fire(Trigger.ChangeRegionFailed);
	}

	public async void StartNetGameIfNeeded()
	{
		try
		{
			await StartNetGameIfNeededAsync();
		}
		catch (Exception ex)
		{
			PFLog.Net.Exception(ex);
		}
	}

	public async Task StartNetGameIfNeededAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		await WaitStateInitIfNeeded(State.PlatformNotInitialized, State.PlatformInitializing, State.PlatformInitialized, Trigger.ToPlatformInitializing, cancellationToken);
		await WaitStateInitIfNeeded(State.PlatformInitialized, State.NetInitializing, State.NetInitialized, Trigger.ToNetInitialization, cancellationToken);
	}

	private async Task WaitStateInitIfNeeded(State startState, State initializingState, State endState, Trigger initializationTrigger, CancellationToken cancellationToken)
	{
		if (m_StateMachine.CurrentState == startState)
		{
			m_StateMachine.Fire(initializationTrigger);
		}
		if (m_StateMachine.CurrentState == startState)
		{
			await m_StateMachine.WaitNextState(initializingState, cancellationToken);
		}
		if (m_StateMachine.CurrentState == initializingState)
		{
			await m_StateMachine.WaitNextState(endState, cancellationToken);
		}
	}

	public void CheatFireTrigger(Trigger trigger)
	{
		m_StateMachine.Fire(trigger);
	}
}
