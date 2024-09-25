using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitActiveEquipmentSetHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitChangeActiveEquipmentSet();
}
public interface IUnitActiveEquipmentSetHandler<TTag> : IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitActiveEquipmentSetHandler, TTag>
{
}
