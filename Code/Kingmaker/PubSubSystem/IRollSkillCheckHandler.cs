using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.PubSubSystem;

public interface IRollSkillCheckHandler : ISubscriber
{
	void HandlePartySkillCheckRolled(RulePerformPartySkillCheck check);

	void HandleUnitSkillCheckRolled(RulePerformSkillCheck check);
}
