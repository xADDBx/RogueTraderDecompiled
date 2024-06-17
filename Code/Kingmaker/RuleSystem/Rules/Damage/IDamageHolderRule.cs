using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.RuleSystem.Rules.Damage;

public interface IDamageHolderRule
{
	DamageData Damage { get; }
}
