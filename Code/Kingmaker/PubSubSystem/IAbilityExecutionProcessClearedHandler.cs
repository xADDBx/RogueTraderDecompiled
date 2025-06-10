using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IAbilityExecutionProcessClearedHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleExecutionProcessCleared(AbilityExecutionContext caster);
}
