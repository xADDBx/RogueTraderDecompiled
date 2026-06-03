using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IForceRebindOverdriveBlock : ISubscriber
{
	void HandleForceRebindOverdriveBlock();
}
