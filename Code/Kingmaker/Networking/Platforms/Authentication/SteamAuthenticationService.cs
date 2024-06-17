using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Photon.Realtime;
using Steamworks;

namespace Kingmaker.Networking.Platforms.Authentication;

internal class SteamAuthenticationService : IAuthenticationService
{
	private HAuthTicket m_AuthTicket;

	private Task<AuthenticationValues> m_AuthenticationValuesTask = Task.FromResult<AuthenticationValues>(null);

	public Task<AuthenticationValues> GetAuthData(CancellationToken cancellationToken)
	{
		if (!SteamManager.Initialized)
		{
			throw new StoreNotInitializedException();
		}
		if (!m_AuthenticationValuesTask.IsCompleted)
		{
			throw new AuthenticationAlreadyInProgressException();
		}
		return m_AuthenticationValuesTask = GetAuthDataImpl(cancellationToken);
	}

	private async Task<AuthenticationValues> GetAuthDataImpl(CancellationToken cancellationToken)
	{
		TaskCompletionSource<bool> callbackTcs = new TaskCompletionSource<bool>();
		using (cancellationToken.Register(delegate
		{
			callbackTcs.SetCanceled();
		}))
		{
			using (RegisterCallback(callbackTcs))
			{
				PFLog.Net.Log("[SteamAuthenticationService.GetAuthDataImpl] callback registered");
				byte[] ticketByteArray = new byte[1024];
				PFLog.Net.Log("[SteamAuthenticationService.GetAuthDataImpl] request session ticket");
				m_AuthTicket = SteamUser.GetAuthSessionTicket(ticketByteArray, ticketByteArray.Length, out var ticketSize);
				if (m_AuthTicket == HAuthTicket.Invalid)
				{
					throw new GetAuthDataException(-1, "GetAuthSessionTicket returns invalid ticket");
				}
				await callbackTcs.Task;
				return CreateAuthenticationValues(ticketByteArray, (int)ticketSize);
			}
		}
	}

	public void OnConnected()
	{
		SteamUser.CancelAuthTicket(m_AuthTicket);
		m_AuthTicket = HAuthTicket.Invalid;
	}

	private static Callback<GetAuthSessionTicketResponse_t> RegisterCallback(TaskCompletionSource<bool> tcs)
	{
		return Callback<GetAuthSessionTicketResponse_t>.Create(delegate(GetAuthSessionTicketResponse_t response)
		{
			PFLog.Net.Log($"[SteamAuthenticationService.RegisterCallback] Result={response.m_eResult}");
			if (response.m_eResult == EResult.k_EResultOK)
			{
				tcs.SetResult(result: true);
			}
			else
			{
				tcs.SetException(new GetAuthDataException((int)response.m_eResult, response.m_eResult.ToString()));
			}
		});
	}

	private static AuthenticationValues CreateAuthenticationValues(byte[] ticketByteArray, int ticketSize)
	{
		AuthenticationValues authenticationValues = new AuthenticationValues();
		authenticationValues.AuthType = CustomAuthenticationType.Steam;
		authenticationValues.UserId = SteamUser.GetSteamID().ToString();
		authenticationValues.AddAuthParameter("ticket", SteamTicketDataToString(ticketByteArray, ticketSize));
		return authenticationValues;
	}

	private static string SteamTicketDataToString(byte[] ticketByteArray, int ticketSize)
	{
		StringBuilder stringBuilder = new StringBuilder(ticketSize * 2);
		for (int i = 0; i < ticketSize; i++)
		{
			stringBuilder.AppendFormat("{0:x2}", ticketByteArray[i]);
		}
		return stringBuilder.ToString();
	}
}
