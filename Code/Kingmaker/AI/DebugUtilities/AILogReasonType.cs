namespace Kingmaker.AI.DebugUtilities;

public enum AILogReasonType
{
	MoveCommandNotSet,
	AcceptableNodesNotFound,
	BetterPositionNotFound,
	PositionForRetreatNotFound,
	AbilityToEscapeFromTreatNotFound,
	NoNeedToMove,
	FoundPathTooShort,
	NothingToDo,
	UnreachableNode,
	UnreachableNodeTrimPath,
	AlreadyOnBestPosition,
	StarshipIsOffCource,
	StarshipPlanWasBroken,
	CantUseExtraMeasures,
	PathIsEmpty,
	BrainIsNull,
	AITimeout
}
