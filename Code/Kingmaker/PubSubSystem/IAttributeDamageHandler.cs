using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAttributeDamageHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleAttributeDamage(StatType attribute, int oldDamage, int newDamage, bool drain);
}
public interface IAttributeDamageHandler<TTag> : IAttributeDamageHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IAttributeDamageHandler, TTag>
{
}
