using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IForceSelectHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleForceSelect(bool single, bool ask);
}
