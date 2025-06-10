using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IShieldBlockHandle : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleShieldBlock();
}
