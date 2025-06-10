using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPetInitializedHandle : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandlePetInitialized(BlueprintPet pet);
}
