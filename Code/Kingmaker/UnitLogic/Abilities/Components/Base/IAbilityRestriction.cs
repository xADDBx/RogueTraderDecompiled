namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityRestriction
{
	bool IsAbilityRestrictionPassed(AbilityData ability);

	string GetAbilityRestrictionUIText();
}
