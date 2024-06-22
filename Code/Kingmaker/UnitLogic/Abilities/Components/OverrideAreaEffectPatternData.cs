using Kingmaker.UnitLogic.Abilities.Components.Patterns;

namespace Kingmaker.UnitLogic.Abilities.Components;

public struct OverrideAreaEffectPatternData
{
	public OrientedPatternData Pattern { get; }

	public bool OverridePatternWithAttackPattern { get; }

	public OverrideAreaEffectPatternData(OrientedPatternData pattern, bool overridePatternWithAttackPattern)
	{
		Pattern = pattern;
		OverridePatternWithAttackPattern = overridePatternWithAttackPattern;
	}
}
