using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICombatTargetChangeHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleTargetChange([CanBeNull] MechanicEntity prevTarget);
}
