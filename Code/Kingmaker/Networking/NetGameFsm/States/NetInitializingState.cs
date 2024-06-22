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

public class NetInitializingState : StateLongAsync
{
	private static readonly TimeSpan DelayBetweenReconnect = TimeSpan.FromSeconds(1.0);

	private const int ReconnectAttempts = 3;

	private readonly INetGame m_NetGame;

	public NetInitializingState([NotNull] INetGame netGame)
	{
		m_NetGame = netGame;
	}

	protected override async Task DoAction(CancellationToken cancellationToken)
	{
		if (PhotonManager.Instance == null)
		{
			await Awaiters.UnityThread;
			PhotonManager.CreateInstance();
		}
		await PlatformServices.Platform.User.Initialize();
		cancellationToken.ThrowIfCancellationRequested();
		AuthenticationValues authenticationValues = await PlatformServices.Platform.AuthService.GetAuthData(cancellationToken, IAuthenticationService.DefaultGetAuthDataTimeout);
		cancellationToken.ThrowIfCancellationRequested();
		await PhotonManager.Instance.ConnectAsync(authenticationValues);
		m_NetGame.OnNetSystemInitialized();
	}

	protected override async void OnActionException(Exception exception)
	{
		base.OnActionException(exception);
		PFLog.Net.Exception(exception);
		await Awaiters.UnityThread;
		bool storeNotInitialized = false;
		if (!(exception is StoreNotInitializedException))
		{
			GetAuthDataException ex = exception as GetAuthDataException;
			if (ex == null)
			{
				if (!(exception is GetAuthDataTimeoutException))
				{
					if (!(exception is PhotonDisconnectedException))
					{
						if (exception is PhotonCustomAuthenticationFailedException)
						{
							EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
							{
								h.HandlePhotonCustomAuthenticationFailedError();
							});
						}
						else
						{
							EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
							{
								h.HandleUnknownException();
							});
						}
					}
				}
				else
				{
					EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
					{
						h.HandleGetAuthDataTimeout();
					});
				}
			}
			else
			{
				EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
				{
					h.HandleGetAuthDataError(ex.FormatErrorMessage());
				});
			}
		}
		else
		{
			EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
			{
				h.HandleStoreNotInitializedError();
			});
			storeNotInitialized = true;
		}
		m_NetGame.OnNetSystemInitializeFailed(storeNotInitialized);
	}
}
