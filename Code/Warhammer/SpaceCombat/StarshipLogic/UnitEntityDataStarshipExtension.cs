using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic;

public static class UnitEntityDataStarshipExtension
{
	public static bool IsStarship(this AbstractUnitEntity unit)
	{
		return unit.Blueprint is BlueprintStarship;
	}

	public static bool IsStarshipAndIsNotInCombat(this AbstractUnitEntity unit)
	{
		if (unit.Blueprint is BlueprintStarship)
		{
			return !unit.IsInCombat;
		}
		return false;
	}

	public static PartStarshipHull GetHull(this AbstractUnitEntity unit)
	{
		return unit.Parts.GetOptional<PartStarshipHull>();
	}

	public static int GetMaxInspiration(this StarshipEntity unit)
	{
		return unit.Morale.MoraleLeft / 10;
	}

	public static int GetDirection(this StarshipEntity unit)
	{
		return CustomGraphHelper.GuessDirection(unit.Forward);
	}
}
