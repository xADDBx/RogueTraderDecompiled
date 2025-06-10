using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;

namespace Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;

public class ScatterPattern : IAbilityAoEPatternProvider
{
	public bool IsIgnoreLos => false;

	public bool UseMeleeLos => false;

	public bool IsIgnoreLevelDifference => false;

	public int PatternAngle => 0;

	public bool CalculateAttackFromPatternCentre => false;

	public bool ExcludeUnwalkable => false;

	TargetType IAbilityAoEPatternProvider.Targets => TargetType.Any;

	public AoEPattern Pattern => null;

	public void OverridePattern(AoEPattern pattern)
	{
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		return AbilityProjectileAttackLineHelper.GetOrientedPattern(ability, casterNode, targetNode, coveredTargetsOnly);
	}
}
