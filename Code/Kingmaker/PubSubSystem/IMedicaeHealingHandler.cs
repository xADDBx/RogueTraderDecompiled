using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.PubSubSystem;

public interface IMedicaeHealingHandler : ISubscriber
{
	void HandleMedicaeHealing(RulePerformMedicaeHeal healDamage);
}
