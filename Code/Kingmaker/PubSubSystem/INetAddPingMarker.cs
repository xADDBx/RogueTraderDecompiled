using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface INetAddPingMarker : ISubscriber
{
	void HandleAddPingEntityMarker(Entity entity);

	void HandleRemovePingEntityMarker(Entity entity);

	void HandleAddPingPositionMarker(GameObject gameObject);

	void HandleRemovePingPositionMarker(GameObject gameObject);
}
