using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.PubSubSystem;

public interface IHealingHandler : ISubscriber
{
	void HandleHealing(RuleHealDamage healDamage);
}
