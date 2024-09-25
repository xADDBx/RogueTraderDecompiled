using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.PubSubSystem;

public interface IUnitCommandStartHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitCommandDidStart(AbstractUnitCommand command);
}
public interface IUnitCommandStartHandler<TTag> : IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitCommandStartHandler, TTag>
{
}
