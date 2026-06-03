using System;
using System.Text;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.AI.Profiling;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.AI.BehaviourTrees;

public class BehaviourTree
{
	public Blackboard Blackboard { get; } = new Blackboard();


	public BehaviourTreeNode Root { get; }

	public bool IsFinishedTurn => Blackboard.IsFinishedTurn;

	public event Action Inited;

	public BehaviourTree(MechanicEntity entity, BehaviourTreeNode rootNode, DecisionContext context)
	{
		Root = rootNode;
		Root.DebugName = "Root";
		Blackboard.Entity = entity;
		Blackboard.DecisionContext = context;
	}

	public void Init()
	{
		Blackboard.Reset();
		Root.Init();
		this.Inited?.Invoke();
	}

	public Status Tick()
	{
		if (Blackboard.IsFinishedTurn)
		{
			return Status.Success;
		}
		Status result;
		try
		{
			if (Blackboard.Stack.Count == 0)
			{
				Root.Init();
				this.Inited?.Invoke();
				AILogger.Instance.Log(AILogNode.Start(Root));
				AIProfileContext.Flush();
			}
			result = Root.Tick(Blackboard);
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
		stringBuilder.Append($"Exception occured in node: {Blackboard.Stack.Peek()}");
		stringBuilder.Append("\nTrace:");
		BehaviourTreeNode[] array = Blackboard.Stack.ToArray();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			stringBuilder.Append($"\n\t{array[num]}");
		}
		return stringBuilder.ToString();
	}
}
