using System;
using Photon.Realtime;

namespace Kingmaker.Networking;

public class PhotonDisconnectedException : Exception
{
	public readonly DisconnectCause Cause;

	public PhotonDisconnectedException(DisconnectCause cause)
	{
		Cause = cause;
	}
}
