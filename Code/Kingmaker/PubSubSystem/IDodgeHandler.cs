using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.PubSubSystem;

public interface IDodgeHandler : ISubscriber
{
	void HandleJumpAsideDodge(RulePerformDodge dodgeRule);

	void HandleSimpleDodge(RulePerformDodge dodgeRule);
}
