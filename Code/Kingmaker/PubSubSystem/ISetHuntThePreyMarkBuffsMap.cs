using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISetHuntThePreyMarkBuffsMap : ISubscriber
{
	void HandleBuffsAdded(MechanicEntity entity, BuffVM vm);

	void HandleBuffsRemoved(MechanicEntity entity);
}
