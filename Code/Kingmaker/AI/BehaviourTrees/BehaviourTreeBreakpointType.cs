using System;

namespace Kingmaker.AI.BehaviourTrees;

[Flags]
public enum BehaviourTreeBreakpointType : byte
{
	None = 0,
	Before = 1,
	After = 2,
	BeforeAndAfter = 3
}
