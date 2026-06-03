namespace Kingmaker.AI.BehaviourTrees;

public static class BehaviourTreeBreakpointTypeExtensions
{
	public static bool HasFlagNonAlloc(this BehaviourTreeBreakpointType value, BehaviourTreeBreakpointType flag)
	{
		return (value & flag) != 0;
	}
}
