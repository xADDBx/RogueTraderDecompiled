using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitVisualChangeHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitChangeEquipmentColor(int rampIndex, bool secondary);
}
