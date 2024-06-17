using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.PubSubSystem;

public interface IUnitCommandEndHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitCommandDidEnd(AbstractUnitCommand command);
}
public interface IUnitCommandEndHandler<TTag> : IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitCommandEndHandler, TTag>
{
}
