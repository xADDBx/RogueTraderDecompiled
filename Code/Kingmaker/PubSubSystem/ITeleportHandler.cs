using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;

namespace Kingmaker.PubSubSystem;

public interface ITeleportHandler : ISubscriber
{
	void HandlePartyTeleport(AreaEnterPoint enterPoint);
}
