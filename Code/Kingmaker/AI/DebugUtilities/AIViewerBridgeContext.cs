using System;
using Kingmaker.AI.BehaviourTrees;

namespace Kingmaker.AI.DebugUtilities;

public class AIViewerBridgeContext : AIDebuggerContext<AIViewerBridgeContext>
{
	public delegate IContextData ContextDataProvider(BehaviourTreeNode node, Blackboard blackboard);

	private static ContextDataProvider _ContextDataProvider;

	public static IDisposable With(BehaviourTreeNode node, Blackboard blackboard)
	{
		return AIDebuggerContext<AIViewerBridgeContext>.InternalWith(node, blackboard);
	}

	public static void SetContextDataProvider(ContextDataProvider callback)
	{
		if (_ContextDataProvider != null)
		{
			PFLog.AI.Warning("ContextDataProvider for AIViewerContextBridge has been already set!");
		}
		else
		{
			_ContextDataProvider = callback;
		}
	}

	public static void RemoveContextDataProvider(ContextDataProvider callback)
	{
		if (_ContextDataProvider != callback)
		{
			PFLog.AI.Warning("Another ContextDataProvider for AIViewerContextBridge has been set than the one you are trying to remove!");
		}
		else
		{
			_ContextDataProvider = null;
		}
	}

	protected override IContextData GetNodeData(BehaviourTreeNode node, Blackboard blackboard)
	{
		return _ContextDataProvider?.Invoke(node, blackboard);
	}
}
