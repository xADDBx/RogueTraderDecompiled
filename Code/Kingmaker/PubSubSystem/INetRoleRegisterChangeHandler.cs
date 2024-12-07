using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetRoleRegisterChangeHandler : ISubscriber
{
	void HandleRoleRegisterChange(string characterId, PhotonActorNumber newPlayerOwner);
}
