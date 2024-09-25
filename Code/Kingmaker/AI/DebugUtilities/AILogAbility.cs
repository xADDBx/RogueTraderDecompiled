using System;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.AI.DebugUtilities;

public class AILogAbility : AILogObject
{
	private enum Type
	{
		SelectAbility,
		SelectAbilityToEscapeThreat,
		ConsiderAbility,
		AbilitySelected,
		ReferenceAbilitySelected,
		AbilityNotSelected,
		AbilityTargetFound,
		AbilityTargetNotFound,
		CantTargetWithAbility,
		Cast,
		CastFailed
	}

	private readonly Type type;

	private readonly CastTimepointType castTimepoint;

	private readonly AbilityData ability;

	private readonly TargetWrapper target;

	private readonly AbilityData.UnavailabilityReasonType? reason;

	public static AILogAbility SelectAbility(CastTimepointType castTimepoint)
	{
		return new AILogAbility(Type.SelectAbility, castTimepoint);
	}

	public static AILogAbility SelectAbilityToEscapeThreat()
	{
		return new AILogAbility(Type.SelectAbilityToEscapeThreat);
	}

	public static AILogAbility ConsiderAbility(AbilityData ability)
	{
		return new AILogAbility(Type.ConsiderAbility, CastTimepointType.None, ability);
	}

	public static AILogAbility AbilitySelected(AbilityData ability)
	{
		return new AILogAbility(Type.SelectAbilityToEscapeThreat, CastTimepointType.None, ability);
	}

	public static AILogAbility ReferenceAbilitySelected(AbilityData ability)
	{
		return new AILogAbility(Type.ReferenceAbilitySelected, CastTimepointType.None, ability);
	}

	public static AILogAbility TargetFound(CastTimepointType castTimepoint, AbilityData ability, TargetWrapper target)
	{
		return new AILogAbility(Type.AbilityTargetFound, castTimepoint, ability, target);
	}

	public static AILogAbility TargetNotFound(CastTimepointType castTimepoint, AbilityData ability)
	{
		return new AILogAbility(Type.AbilityTargetNotFound, castTimepoint, ability);
	}

	public static AILogAbility CantTargetWithAbility(AbilityData ability, TargetWrapper target, AbilityData.UnavailabilityReasonType? reason)
	{
		return new AILogAbility(Type.CantTargetWithAbility, CastTimepointType.None, ability, target, reason);
	}

	public static AILogAbility AbilityNotSelected(CastTimepointType castTimepoint)
	{
		return new AILogAbility(Type.AbilityNotSelected, castTimepoint);
	}

	public static AILogAbility Cast(AbilityData ability, TargetWrapper target)
	{
		return new AILogAbility(Type.Cast, CastTimepointType.None, ability, target);
	}

	public static AILogAbility CastFailed(AbilityData ability, TargetWrapper target)
	{
		return new AILogAbility(Type.CastFailed, CastTimepointType.None, ability, target);
	}

	private AILogAbility(Type type, CastTimepointType castTimepoint = CastTimepointType.None, AbilityData ability = null, TargetWrapper target = null, AbilityData.UnavailabilityReasonType? reason = null)
	{
		this.type = type;
		this.castTimepoint = castTimepoint;
		this.ability = ability;
		this.target = target;
		this.reason = reason;
	}

	public override string GetLogString()
	{
		return type switch
		{
			Type.SelectAbility => $"Select ability for CastTimepoint: {castTimepoint}", 
			Type.SelectAbilityToEscapeThreat => "Try select ability to escape from threat", 
			Type.ConsiderAbility => $"Consider ability: {ability}", 
			Type.AbilitySelected => $"Selected ability: {ability}", 
			Type.ReferenceAbilitySelected => "Select reference ability: " + (ability?.ToString() ?? "<null>"), 
			Type.AbilityNotSelected => $"No appropriate ability for CastTimepoint: {castTimepoint}", 
			Type.AbilityTargetFound => $"Select ability: {ability} (CastTimepoint: {castTimepoint}), target: {target}", 
			Type.AbilityTargetNotFound => $"Didn't find any target for {ability}", 
			Type.CantTargetWithAbility => $"Can't target {target} with {ability}: {reason}", 
			Type.Cast => $"Cast {ability} on {target} ({target.NearestNode})", 
			Type.CastFailed => string.Format("Failed to cast ability: {0} on target: {1} ({2})", ability?.ToString() ?? "<null>", target?.ToString() ?? "<null>", target?.NearestNode), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
