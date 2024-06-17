using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UnitLogic.Parts;

public interface IUnitInventoryChanged : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleInventoryChanged();
}
public interface IUnitInventoryChanged<TTag> : IUnitInventoryChanged, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitInventoryChanged, TTag>
{
}
