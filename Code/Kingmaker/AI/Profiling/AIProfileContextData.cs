using System.Diagnostics;
using Kingmaker.AI.BehaviourTrees;

namespace Kingmaker.AI.Profiling;

public class AIProfileContextData
{
	public BehaviourTreeNode Node;

	public int Frames;

	public float TotalTimeMsec;

	public Status Result;

	public Stopwatch Stopwatch;

	public float AvgTimePerFrame
	{
		get
		{
			if (Frames <= 0)
			{
				return 0f;
			}
			return TotalTimeMsec / (float)Frames;
		}
	}

	public AIProfileContextData(BehaviourTreeNode node)
	{
		Node = node;
		Frames = 0;
		TotalTimeMsec = 0f;
		Result = Status.Unknown;
		Stopwatch = new Stopwatch();
	}

	public void EnterContext()
	{
		Stopwatch.Start();
	}

	public void ExitContext()
	{
		Stopwatch.Stop();
		Frames++;
		TotalTimeMsec += Stopwatch.ElapsedMilliseconds;
		Stopwatch.Reset();
		Result = Node.Status;
	}

	public override string ToString()
	{
		return $"{Node.DebugName}: {Result} ({TotalTimeMsec} ms, {Frames} frames, {AvgTimePerFrame} ms/frame)";
	}
}
