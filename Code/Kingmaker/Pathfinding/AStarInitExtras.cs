using System;
using System.Reflection;

namespace Kingmaker.Pathfinding;

public static class AStarInitExtras
{
	private static readonly Action<AstarPath> InitializePathProcessorDlg;

	private static readonly Action<AstarPath> InitializeProfilerDlg;

	private static readonly Action<AstarPath> InitializeAstarDataDlg;

	static AStarInitExtras()
	{
		InitializePathProcessorDlg = (Action<AstarPath>)typeof(AstarPath).GetMethod("InitializePathProcessor", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(Action<AstarPath>));
		InitializeProfilerDlg = (Action<AstarPath>)typeof(AstarPath).GetMethod("InitializeProfiler", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(Action<AstarPath>));
		InitializeAstarDataDlg = (Action<AstarPath>)typeof(AstarPath).GetMethod("InitializeAstarData", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(Action<AstarPath>));
	}

	public static void Initialize(this AstarPath path)
	{
		InitializePathProcessorDlg(path);
		InitializeProfilerDlg(path);
		path.ConfigureReferencesInternal();
		InitializeAstarDataDlg(path);
	}
}
