using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IRespecHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleRespecFinished();
}
