using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.PubSubSystem;

public interface ITemporaryHitPoints : ISubscriber<IEntity>, ISubscriber
{
	void HandleOnAddTemporaryHitPoints(int amount, Buff buff);

	void HandleOnRemoveTemporaryHitPoints(int amount, Buff buff);
}
