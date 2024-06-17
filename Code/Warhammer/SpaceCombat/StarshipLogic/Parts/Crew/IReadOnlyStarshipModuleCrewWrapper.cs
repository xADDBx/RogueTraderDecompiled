using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Parts.Crew;

public interface IReadOnlyStarshipModuleCrewWrapper
{
	ShipModuleType ShipModuleType { get; }

	ShipCrewModuleState GetState(bool includeInTransition);

	int GetAliveCount(bool includeInTransition);

	bool CanMoveFrom();

	bool CanMoveTo();

	int GetCountInTransitionToModule();

	int GetAvailableToMoveCount();
}
