using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IUnitInvisibleHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitUpdateInvisible();

	void RemoveUnitInvisible();
}
