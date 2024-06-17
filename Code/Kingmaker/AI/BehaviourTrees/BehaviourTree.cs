using System;
using System.Text;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.AI.Profiling;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.AI.BehaviourTrees;

public class BehaviourTree
{
	private Blackboard blackboard = new Blackboard();

	private BehaviourTreeNode root;

	public bool IsFinishedTurn => blackboard.IsFinishedTurn;

	public BehaviourTree(MechanicEntity entity, BehaviourTreeNode rootNode, DecisionContext context)
	{
		root = rootNode;
		root.DebugName = "Root";
		blackboard.Entity = entity;
		blackboard.DecisionContext = context;
	}

	public void Init()
	{
		root.Init();
		blackboard.Reset();
	}

	public Status Tick()
	{
		if (blackboard.IsFinishedTurn)
		{
			return Status.Success;
		}
		Status result;
		try
		{
			if (blackboard.Stack.Count == 0)
			{
				root.Init();
				AILogger.Instance.Log(AILogNode.Start(root));
				AIProfileContext.Flush();
			}
			result = root.Tick(blackboard);
		}
		catch (Exception ex)
		{
			result = Status.Failure;
			string message = BuildExceptionInfoMessage();
			AILogger.Instance.Exception(ex, message);
		}
		return result;
	}

	private string BuildExceptionInfoMessage()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"Exception occured in node: {blackboard.Stack.Peek()}");
		stringBuilder.Append("\nTrace:");
		BehaviourTreeNode[] array = blackboard.Stack.ToArray();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			stringBuilder.Append($"\n\t{array[num]}");
		}
		return stringBuilder.ToString();
	}
}
