using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using Kingmaker.Networking.Platforms;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Fsm;

namespace Kingmaker.Networking.NetGameFsm.States;

public class PlatformInitializingState : StateLongAsync
{
	private readonly INetGame m_NetGame;

	public PlatformInitializingState(INetGame netGame)
	{
		m_NetGame = netGame;
	}

	public override Task OnEnter()
	{
		CheatNetManager.Init();
		return base.OnEnter();
	}

	protected override async Task DoAction(CancellationToken cancellationToken)
	{
		PhotonManager.Invite.InitPlatform();
		if (await PlatformServices.Platform.WaitInitialization())
		{
			m_NetGame.OnPlatformInitialized();
			return;
		}
		await Awaiters.UnityThread;
		PFLog.Net.Error("Store not initialized");
		EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
		{
			h.HandleStoreNotInitializedError();
		});
		m_NetGame.OnPlatformInitializeFailed();
	}

	protected override async void OnActionException(Exception exception)
	{
		base.OnActionException(exception);
		PFLog.Net.Exception(exception);
		await Awaiters.UnityThread;
		EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
		{
			h.HandleUnknownException();
		});
		m_NetGame.OnPlatformInitializeFailed();
	}
}
