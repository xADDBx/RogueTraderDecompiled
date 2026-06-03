using System;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.AI.Profiling;

public class AIProfileContext : AIDebuggerContext<AIProfileContext>
{
	private static Action<AIProfileBTreeNodeData> _OnBeforeFlushCallback;

	private static AIProfileBTreeNodeData _RootContextData;

	public static IDisposable With(BehaviourTreeNode node, Blackboard blackboard)
	{
		return AIDebuggerContext<AIProfileContext>.InternalWith(node, blackboard);
	}

	public static void SetOnBeforeFlushCallback(Action<AIProfileBTreeNodeData> beforeFlushCallback)
	{
		_OnBeforeFlushCallback = beforeFlushCallback;
	}

	public static void Flush()
	{
		if (_RootContextData != null)
		{
			_OnBeforeFlushCallback?.Invoke(_RootContextData);
		}
		_RootContextData = null;
	}

	protected override IContextData GetNodeData(BehaviourTreeNode node, Blackboard blackboard)
	{
		if (AIDebuggerContext<AIProfileContext>._CurrentTreeNodeContextData == null)
		{
			if (_RootContextData == null)
			{
				_RootContextData = new AIProfileBTreeNodeData(node);
			}
			return _RootContextData;
		}
		IContextData contextData = AIDebuggerContext<AIProfileContext>._CurrentTreeNodeContextData.Children.FindOrDefault((IContextData c) => c.Node == node);
		if (contextData == null)
		{
			contextData = new AIProfileBTreeNodeData(node);
			AIDebuggerContext<AIProfileContext>._CurrentTreeNodeContextData.Children.Add(contextData);
		}
		return contextData;
	}
}
