using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;

namespace Kingmaker.PubSubSystem;

public interface IStarshipAttackHandler : ISubscriber
{
	void HandleAttack(RuleStarshipPerformAttack starshipAttack);
}
