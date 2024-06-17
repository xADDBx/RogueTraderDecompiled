using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface ITrySelectNotControllableHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleSelectNotControllable(bool single, bool ask);
}
