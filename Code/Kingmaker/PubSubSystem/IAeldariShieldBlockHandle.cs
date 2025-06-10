using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAeldariShieldBlockHandle : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleAeldariShieldBlock();
}
