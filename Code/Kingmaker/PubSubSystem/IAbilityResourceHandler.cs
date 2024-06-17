using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;

namespace Kingmaker.PubSubSystem;

public interface IAbilityResourceHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleAbilityResourceChange(AbilityResource resource);
}
