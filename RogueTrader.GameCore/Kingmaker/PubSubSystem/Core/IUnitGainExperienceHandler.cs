using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IUnitGainExperienceHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitGainExperience(int gained, bool withSound = false);
}
