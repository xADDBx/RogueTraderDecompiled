namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityAllowTargetingType
{
	public enum TargetTypeEnum
	{
		CanTargetPoint,
		CanTargetEnemies,
		CanTargetFriends,
		CanTargetSelf
	}

	TargetTypeEnum TargetType { get; }

	bool IsRestrictionPassed(AbilityData abilityData);
}
