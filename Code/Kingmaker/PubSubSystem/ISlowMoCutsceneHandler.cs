using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISlowMoCutsceneHandler : ISubscriber
{
	void AddUnitToNormalTimeline(AbstractUnitEntity unit);

	void OffSlowMo();
}
