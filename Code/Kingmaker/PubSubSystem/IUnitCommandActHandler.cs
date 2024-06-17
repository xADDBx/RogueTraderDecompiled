using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.PubSubSystem;

public interface IUnitCommandActHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitCommandDidAct(AbstractUnitCommand command);
}
public interface IUnitCommandActHandler<TTag> : IUnitCommandActHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitCommandActHandler, TTag>
{
}
