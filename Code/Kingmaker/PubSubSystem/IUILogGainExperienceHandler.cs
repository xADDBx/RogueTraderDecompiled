using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUILogGainExperienceHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitGainExperience(int gained);
}
