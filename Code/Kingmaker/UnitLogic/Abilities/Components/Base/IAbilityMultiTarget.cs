namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityMultiTarget
{
	int TargetAbilityCount { get; }

	bool TryGetNextTargetAbility(AbilityData rootAbility, int targetIndex, out AbilityData ability);
}
