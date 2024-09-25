using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEntityJoinTBCombat : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleEntityJoinTBCombat();
}
public interface IEntityJoinTBCombat<TTag> : IEntityJoinTBCombat, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityJoinTBCombat, TTag>
{
}
