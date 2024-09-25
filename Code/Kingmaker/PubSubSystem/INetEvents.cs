using System;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetEvents : ISubscriber
{
	void HandleTransferProgressChanged(bool value);

	[Obsolete("Use HandleNetGameStateChanged instead")]
	void HandleNetStateChanged(LobbyNetManager.State state);

	void HandleNetGameStateChanged(NetGame.State state);

	void HandleNLoadingScreenClosed();
}
