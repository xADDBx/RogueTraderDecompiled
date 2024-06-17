using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitDeathHandler : ISubscriber
{
	void HandleUnitDeath(AbstractUnitEntity unitEntity);
}
