using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IInitiativeChangeHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleInitiativeChanged();
}
