using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetRoleSetHandler : ISubscriber
{
	void HandleRoleSet(string entityId);
}
