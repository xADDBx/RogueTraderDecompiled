using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetRolesRequest : ISubscriber
{
	void HandleNetRolesRequest();
}
