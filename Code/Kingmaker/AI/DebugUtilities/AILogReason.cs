using System;
using Pathfinding;

namespace Kingmaker.AI.DebugUtilities;

public class AILogReason : AILogObject
{
	private readonly AILogReasonType reason;

	private readonly GraphNode nodeParam;

	public AILogReason(AILogReasonType reason, GraphNode nodeParam = null)
	{
		this.reason = reason;
		this.nodeParam = nodeParam;
	}

	public override string GetLogString()
	{
		return reason switch
		{
			AILogReasonType.MoveCommandNotSet => "Move command was not set up -> already moved", 
			AILogReasonType.AcceptableNodesNotFound => "No acceptable nodes were found", 
			AILogReasonType.BetterPositionNotFound => "Better position not found", 
			AILogReasonType.PositionForRetreatNotFound => "Position for retreat not found", 
			AILogReasonType.AbilityToEscapeFromTreatNotFound => "No appropriate ability to escape from threat", 
			AILogReasonType.NoNeedToMove => "No need to move", 
			AILogReasonType.FoundPathTooShort => "Path must contain 2 or more nodes", 
			AILogReasonType.NothingToDo => "Nothing to do, finish turn", 
			AILogReasonType.UnreachableNode => $"GetAcceptableNodes returned unreachable node: {nodeParam}", 
			AILogReasonType.UnreachableNodeTrimPath => $"{nodeParam} is unreachable, trim path", 
			AILogReasonType.AlreadyOnBestPosition => $"Already on best position: {nodeParam}", 
			AILogReasonType.StarshipIsOffCource => "Something wrong happened, the starship is off course", 
			AILogReasonType.StarshipPlanWasBroken => "Last action broke starship acting plan, try plan all over again", 
			AILogReasonType.CantUseExtraMeasures => "Can't use extra measures", 
			AILogReasonType.PathIsEmpty => "Path is empty", 
			AILogReasonType.BrainIsNull => "Brain is null, end turn", 
			AILogReasonType.AITimeout => "AI-agent acted for too long! Interrupt all commands and end turn", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
