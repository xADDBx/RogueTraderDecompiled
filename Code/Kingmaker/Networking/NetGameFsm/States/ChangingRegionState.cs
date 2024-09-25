using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using JetBrains.Annotations;
using Kingmaker.Networking.Platforms;
using Kingmaker.Networking.Platforms.Authentication;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Fsm;
using Photon.Realtime;

namespace Kingmaker.Networking.NetGameFsm.States;

public class ChangingRegionState : StateLongAsync
{
	private readonly string m_NewRegion;

	private readonly INetGame m_NetGame;

	private readonly PhotonManager m_PhotonManager;

	public ChangingRegionState([NotNull] string newRegion, [NotNull] INetGame netGame, [NotNull] PhotonManager photonManager)
	{
		m_NewRegion = newRegion;
		m_NetGame = netGame;
		m_PhotonManager = photonManager;
	}

	protected override async Task DoAction(CancellationToken cancellationToken)
	{
		AuthenticationValues authenticationValues = await PlatformServices.Platform.AuthService.GetAuthData(cancellationToken, IAuthenticationService.DefaultGetAuthDataTimeout);
		await m_PhotonManager.SetRegionAsync(m_NewRegion, authenticationValues);
		m_NetGame.OnRegionChanged();
	}

	protected override async void OnActionException(Exception exception)
	{
		base.OnActionException(exception);
		PFLog.Net.Exception(exception);
		await Awaiters.UnityThread;
		EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
		{
			h.HandleChangeRegionError();
		});
		m_NetGame.OnRegionChangeFailed();
	}
}
