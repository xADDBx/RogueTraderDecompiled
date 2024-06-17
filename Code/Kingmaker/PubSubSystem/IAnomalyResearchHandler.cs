using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.PubSubSystem;

public interface IAnomalyResearchHandler : ISubscriber<AnomalyEntityData>, ISubscriber
{
	void HandleAnomalyStartResearch();

	void HandleAnomalyResearched(BaseUnitEntity unit, RulePerformSkillCheck skillCheck);
}
