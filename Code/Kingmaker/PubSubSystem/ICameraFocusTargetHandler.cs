using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICameraFocusTargetHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleCameraRetain(MechanicEntity cameraFrom);
}
