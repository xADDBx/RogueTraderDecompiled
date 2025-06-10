using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPetSetupHandle : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandlePetSetup(BaseUnitEntity petEntity);
}
