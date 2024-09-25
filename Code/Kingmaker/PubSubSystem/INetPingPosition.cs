using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface INetPingPosition : ISubscriber
{
	void HandlePingPosition(NetPlayer player, Vector3 position);

	void HandlePingPositionSound(GameObject gameObject);
}
