using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.PubSubSystem;

public interface IHealWoundOrTrauma : ISubscriber<IEntity>, ISubscriber
{
	void HandleOnHealWoundOrTrauma(Buff buff);
}
