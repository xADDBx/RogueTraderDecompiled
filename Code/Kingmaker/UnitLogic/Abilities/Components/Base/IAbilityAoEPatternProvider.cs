using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Groups;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityAoEPatternProvider
{
	bool IsIgnoreLos { get; }

	bool UseMeleeLos { get; }

	bool IsIgnoreLevelDifference { get; }

	int PatternAngle { get; }

	bool CalculateAttackFromPatternCentre { get; }

	bool ExcludeUnwalkable { get; }

	TargetType Targets { get; }

	[CanBeNull]
	AoEPattern Pattern { get; }

	AoEPattern OriginalPattern => Pattern;

	void OverridePattern(AoEPattern pattern);

	OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false);

	bool CanTargetDueToType(MechanicEntity caster, MechanicEntity target)
	{
		if (Targets == TargetType.Any)
		{
			return true;
		}
		PartCombatGroup combatGroupOptional = target.GetCombatGroupOptional();
		TargetType targets = Targets;
		if (targets != 0)
		{
			if (targets == TargetType.Ally && (combatGroupOptional == null || !combatGroupOptional.IsAlly(caster)))
			{
				goto IL_003c;
			}
		}
		else if (combatGroupOptional != null && !combatGroupOptional.IsEnemy(caster))
		{
			goto IL_003c;
		}
		return true;
		IL_003c:
		return false;
	}
}
