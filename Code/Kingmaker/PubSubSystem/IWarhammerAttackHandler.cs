using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.PubSubSystem;

public interface IWarhammerAttackHandler : ISubscriber
{
	void HandleAttack(RulePerformAttack withWeaponAttackHit);
}
