using System;
using Kingmaker.AI.BehaviourTrees;

namespace Kingmaker.AI.DebugUtilities;

public class AILogNode : AILogObject
{
	private enum Type
	{
		Start,
		End,
		Status
	}

	private readonly Type type;

	private readonly BehaviourTreeNode btreeNode;

	public static AILogNode Start(BehaviourTreeNode node)
	{
		return new AILogNode(Type.Start, node);
	}

	public static AILogNode End(BehaviourTreeNode node)
	{
		return new AILogNode(Type.End, node);
	}

	public static AILogNode Status(BehaviourTreeNode node)
	{
		return new AILogNode(Type.Status, node);
	}

	private AILogNode(Type type, BehaviourTreeNode btreeNode)
	{
		this.type = type;
		this.btreeNode = btreeNode;
	}

	public override string GetLogString()
	{
		return type switch
		{
			Type.Start => "[" + btreeNode.DebugName + ": started]", 
			Type.End => "[" + btreeNode.DebugName + " ended]", 
			Type.Status => $"[{btreeNode.DebugName}: {btreeNode.Status}]", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
