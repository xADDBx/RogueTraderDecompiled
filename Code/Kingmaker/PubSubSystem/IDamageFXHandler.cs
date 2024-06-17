using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.PubSubSystem;

public interface IDamageFXHandler : ISubscriber
{
	void HandleDamageDealt(RuleDealDamage dealDamage);
}
