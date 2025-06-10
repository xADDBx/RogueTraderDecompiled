using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IChangeUnitLevel : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleChangeUnitLevel();
}
