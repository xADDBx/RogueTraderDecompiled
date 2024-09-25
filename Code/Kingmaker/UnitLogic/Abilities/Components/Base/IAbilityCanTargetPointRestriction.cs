namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityCanTargetPointRestriction
{
	bool IsAbilityCanTargetPointRestrictionPassed(AbilityData ability);

	string GetAbilityCanTargetPointRestrictionUIText();
}
