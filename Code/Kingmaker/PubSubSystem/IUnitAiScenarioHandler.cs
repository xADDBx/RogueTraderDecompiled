using Kingmaker.AI.Scenarios;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitAiScenarioHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleScenarioActivated(AiScenario scenario);

	void HandleScenarioDeactivated(AiScenario scenario);
}
