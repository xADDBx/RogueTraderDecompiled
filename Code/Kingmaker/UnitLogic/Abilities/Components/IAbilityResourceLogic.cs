using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.UnitLogic.Abilities.Components;

public interface IAbilityResourceLogic : IAbilityRestriction
{
	void Spend(AbilityData ability);

	int CalculateCost(AbilityData ability);

	bool IsSpendResource();

	int CalculateResourceAmount(AbilityData ability);
}
