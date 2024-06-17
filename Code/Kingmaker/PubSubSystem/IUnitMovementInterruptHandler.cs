using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IUnitMovementInterruptHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleMovementInterruption(Vector3 destination);
}
