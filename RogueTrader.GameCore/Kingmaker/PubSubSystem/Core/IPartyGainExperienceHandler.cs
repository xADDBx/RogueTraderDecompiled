using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IPartyGainExperienceHandler : ISubscriber
{
	void HandlePartyGainExperience(int gained, bool isExperienceForDeath);
}
