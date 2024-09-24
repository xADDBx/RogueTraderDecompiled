using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Galaxy.Api;
using Photon.Realtime;
using Plugins.GOG;

namespace Kingmaker.Networking.Platforms.Authentication;

internal class GogAuthenticationService : IAuthenticationService
{
	public class EncryptedAppTicketListener : IEncryptedAppTicketListener
	{
		private readonly TaskCompletionSource<Memory<byte>> m_Tcs;

		private EncryptedAppTicketListener(TaskCompletionSource<Memory<byte>> tcs)
		{
			m_Tcs = tcs;
		}

		public override void OnEncryptedAppTicketRetrieveSuccess()
		{
			byte[] array = new byte[1024];
			uint currentEncryptedAppTicketSize = 0u;
			GalaxyInstance.User().GetEncryptedAppTicket(array, 1024u, ref currentEncryptedAppTicketSize);
			m_Tcs.TrySetResult(new Memory<byte>(array, 0, (int)currentEncryptedAppTicketSize));
		}

		public override void OnEncryptedAppTicketRetrieveFailure(FailureReason failureReason)
		{
			base.OnEncryptedAppTicketRetrieveFailure(failureReason);
			m_Tcs.TrySetException(new GetAuthDataException((int)failureReason, failureReason.ToString()));
		}

		public static async Task<Memory<byte>> RequestTicket(byte[] additionalData, int additionalDataSize, CancellationToken cancellationToken)
		{
			TaskCompletionSource<Memory<byte>> tcs = new TaskCompletionSource<Memory<byte>>();
			using (cancellationToken.Register(delegate
			{
				tcs.TrySetCanceled();
			}))
			{
				EncryptedAppTicketListener listener = new EncryptedAppTicketListener(tcs);
				GalaxyInstance.User().RequestEncryptedAppTicket(additionalData, (uint)additionalDataSize, listener);
				return await tcs.Task;
			}
		}
	}

	public Task<AuthenticationValues> GetAuthData(CancellationToken cancellationToken)
	{
		if (!GogGalaxyManager.IsInitializedAndLoggedOn())
		{
			throw new StoreNotInitializedException();
		}
		return GetAuthDataImpl(cancellationToken);
	}

	private static async Task<AuthenticationValues> GetAuthDataImpl(CancellationToken cancellationToken)
	{
		PFLog.Net.Log("[GOG] Begin getting auth data");
		Memory<byte> memory = await EncryptedAppTicketListener.RequestTicket(Array.Empty<byte>(), 0, cancellationToken);
		string @string = Encoding.UTF8.GetString(memory.Span);
		PFLog.Net.Log("[GOG] Finish getting auth data: " + @string);
		AuthenticationValues authenticationValues = new AuthenticationValues();
		authenticationValues.AuthType = CustomAuthenticationType.Custom;
		authenticationValues.UserId = GogGalaxyManager.Instance.UserId.ToString();
		authenticationValues.AddAuthParameter("encryptedAppTicket", @string);
		return authenticationValues;
	}

	public void OnConnected()
	{
		PFLog.Net.Log("[GOG] On connected");
	}
}
