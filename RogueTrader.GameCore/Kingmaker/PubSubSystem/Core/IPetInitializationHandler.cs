using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IPetInitializationHandler : ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	void OnPetInitialized();
}
