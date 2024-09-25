using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetPingEntity : ISubscriber
{
	void HandlePingEntity(NetPlayer player, Entity entity);
}
