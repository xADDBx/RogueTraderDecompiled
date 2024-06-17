using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUISpaceCombatExperienceGainedPerAreaHandler : ISubscriber<IStarshipEntity>, ISubscriber
{
	void HandlerOnSpaceCombatExperienceGainedPerArea(int exp);
}
