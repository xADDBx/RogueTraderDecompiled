using System.Collections.Generic;
using Kingmaker.AI.BehaviourTrees;

namespace Kingmaker.AI.DebugUtilities;

public interface IContextData
{
	BehaviourTreeNode Node { get; }

	List<IContextData> Children { get; }

	void EnterContext();

	void ExitContext();
}
