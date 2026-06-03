using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitOverdriveAugmentHandler : ISubscriber
{
	void HandleAugmentActivateOverdrive(BaseUnitEntity owner);

	void HandleAugmentDeactivateOverdrive(BaseUnitEntity owner);
}
