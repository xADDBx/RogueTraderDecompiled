using System;
using System.Collections.Generic;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.Utility.BuildModeUtils;

namespace Kingmaker.AI.DebugUtilities;

public abstract class AIDebuggerContext<T> : IDisposable where T : AIDebuggerContext<T>, new()
{
	private static readonly List<T> _Pool = new List<T>();

	private static int _CurrentIndex;

	private IContextData _currentNodeContextData;

	protected static IContextData _CurrentTreeNodeContextData
	{
		get
		{
			if (_CurrentIndex <= 0)
			{
				return null;
			}
			return _Pool[_CurrentIndex - 1]._currentNodeContextData;
		}
	}

	protected static IDisposable InternalWith(BehaviourTreeNode node, Blackboard blackboard)
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			return null;
		}
		if (BehaviourTreeNodeBreakpoint.IsActiveDebugWillRemainsThisTick())
		{
			return null;
		}
		T val;
		if (_CurrentIndex < _Pool.Count)
		{
			val = _Pool[_CurrentIndex];
		}
		else
		{
			_Pool.Add(val = new T());
		}
		IContextData nodeData = val.GetNodeData(node, blackboard);
		if (nodeData != null)
		{
			val.Setup(nodeData);
		}
		_CurrentIndex++;
		return val;
	}

	public void Dispose()
	{
		_currentNodeContextData?.ExitContext();
		_CurrentIndex = Math.Max(_CurrentIndex - 1, 0);
	}

	protected abstract IContextData GetNodeData(BehaviourTreeNode node, Blackboard blackboard);

	private void Setup(IContextData nodeData)
	{
		_currentNodeContextData = nodeData;
		_currentNodeContextData.EnterContext();
	}
}
