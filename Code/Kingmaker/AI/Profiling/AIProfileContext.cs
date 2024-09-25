using System;
using System.Collections.Generic;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.AI.Profiling;

public class AIProfileContext : IDisposable
{
	private static readonly List<AIProfileContext> Pool = new List<AIProfileContext>(8);

	private static int s_CurrentIndex;

	private static AIProfileBTreeNodeData Root;

	private static Action<AIProfileBTreeNodeData> s_OnBeforeFlushCallback;

	public AIProfileBTreeNodeData TreeNodeData;

	private static AIProfileBTreeNodeData CurrentTreeNodeData
	{
		get
		{
			if (s_CurrentIndex <= 0)
			{
				return null;
			}
			return Pool[s_CurrentIndex - 1].TreeNodeData;
		}
	}

	public BehaviourTreeNode Node => TreeNodeData.Node;

	public static AIProfileContext With(BehaviourTreeNode node)
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			return null;
		}
		AIProfileBTreeNodeData nodeData = GetNodeData(node);
		AIProfileContext aIProfileContext;
		if (s_CurrentIndex < Pool.Count)
		{
			aIProfileContext = Pool[s_CurrentIndex++];
		}
		else
		{
			Pool.Add(aIProfileContext = new AIProfileContext());
			s_CurrentIndex++;
		}
		aIProfileContext.Setup(nodeData);
		return aIProfileContext;
	}

	private static AIProfileBTreeNodeData GetNodeData(BehaviourTreeNode node)
	{
		if (CurrentTreeNodeData == null)
		{
			if (Root == null)
			{
				Root = new AIProfileBTreeNodeData(node);
			}
			return Root;
		}
		AIProfileBTreeNodeData aIProfileBTreeNodeData = CurrentTreeNodeData.Children.FindOrDefault((AIProfileBTreeNodeData c) => c.Node == node);
		if (aIProfileBTreeNodeData == null)
		{
			aIProfileBTreeNodeData = new AIProfileBTreeNodeData(node);
			CurrentTreeNodeData.Children.Add(aIProfileBTreeNodeData);
		}
		return aIProfileBTreeNodeData;
	}

	private void Setup(AIProfileBTreeNodeData nodeData)
	{
		TreeNodeData = nodeData;
		TreeNodeData.EnterContext();
	}

	public void Dispose()
	{
		TreeNodeData.ExitContext();
		s_CurrentIndex = Math.Max(s_CurrentIndex - 1, 0);
	}

	public static void Flush()
	{
		if (Root != null)
		{
			s_OnBeforeFlushCallback?.Invoke(Root);
		}
		Root = null;
	}

	public static void SetOnBeforeFlushCallback(Action<AIProfileBTreeNodeData> beforeFlushCallback)
	{
		s_OnBeforeFlushCallback = beforeFlushCallback;
	}
}
