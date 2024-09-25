using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityAoEPatternProvider
{
	bool IsIgnoreLos { get; }

	bool UseMeleeLos { get; }

	bool IsIgnoreLevelDifference { get; }

	int PatternAngle { get; }

	bool CalculateAttackFromPatternCentre { get; }

	TargetType Targets { get; }

	[CanBeNull]
	AoEPattern Pattern { get; }

	void OverridePattern(AoEPattern pattern);

	OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false);
}
