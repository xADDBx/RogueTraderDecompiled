using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Cheats;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.ProfilingCounters;

namespace Kingmaker.QA.Arbiter.Profiling;

public class Counters
{
	[CanBeNull]
	public static readonly Counter Runner;

	[CanBeNull]
	public static readonly Counter AnimationManager;

	[CanBeNull]
	public static readonly Counter StandardMaterialController;

	[CanBeNull]
	public static readonly Counter Trails;

	[CanBeNull]
	public static readonly Counter SnapController;

	[CanBeNull]
	public static readonly Counter ReflectionProbes;

	[CanBeNull]
	public static readonly Counter Particles;

	[CanBeNull]
	public static readonly Counter MeshSkinning;

	[CanBeNull]
	public static readonly Counter UIRaycast;

	[CanBeNull]
	public static readonly Counter Overtips;

	[CanBeNull]
	public static readonly Counter UIDecals;

	[CanBeNull]
	public static readonly Counter UpdateCanvases;

	[CanBeNull]
	public static readonly Counter Audio;

	[CanBeNull]
	public static readonly Counter ScriptsTotal;

	[CanBeNull]
	public static readonly Counter EntityPositionChanged;

	[CanBeNull]
	public static readonly Counter PhysicsOverlap;

	[CanBeNull]
	public static readonly Counter AnimatorDirector;

	[CanBeNull]
	public static readonly Counter CombatLogs;

	private static List<Counter> s_All;

	public static List<Counter> All
	{
		get
		{
			List<Counter> result = s_All ?? CreateList();
			s_All = result;
			return result;
		}
	}

	static Counters()
	{
		Runner = new Counter("Runner", 10.0);
		AnimationManager = new Counter("AnimationManager", 2.0);
		StandardMaterialController = new Counter("StandardMaterialController", 1.0);
		Trails = new Counter("Trails", 1.0);
		SnapController = new Counter("SnapController", 1.0);
		ReflectionProbes = new Counter("ReflectionProbes", 0.001);
		Particles = new Counter("Particles", 1.0);
		MeshSkinning = new Counter("MeshSkinning", 1.0);
		UIRaycast = new Counter("UIRaycast", 2.0);
		Overtips = new Counter("Overtips", 1.0);
		UIDecals = new Counter("UIDecals", 0.5);
		UpdateCanvases = new Counter("UpdateCanvases", 0.1);
		Audio = new Counter("Audio", 0.5);
		ScriptsTotal = new Counter("ScriptsTotal", 25.0);
		EntityPositionChanged = new Counter("EntityPositionChanged", 0.15);
		PhysicsOverlap = new Counter("PhysicsOverlap", 0.8);
		AnimatorDirector = new Counter("AnimatorDirector", 0.75);
		CombatLogs = new Counter("CombatLogs", 0.1);
	}

	private static List<Counter> CreateList()
	{
		s_All = new List<Counter>();
		foreach (FieldInfo item3 in from f in typeof(Counters).GetFields(BindingFlags.Static | BindingFlags.Public).Concat(typeof(Owlcat.Runtime.Core.ProfilingCounters.Counters).GetFields(BindingFlags.Static | BindingFlags.Public))
			where f.FieldType == typeof(Counter)
			select f)
		{
			if (item3.GetValue(null) is Counter item)
			{
				s_All.Add(item);
			}
		}
		foreach (PropertyInfo item4 in from f in typeof(Counters).GetProperties(BindingFlags.Static | BindingFlags.Public)
			where f.PropertyType == typeof(Counter)
			select f)
		{
			if (item4.GetValue(null) is Counter item2)
			{
				s_All.Add(item2);
			}
		}
		return s_All;
	}

	[Cheat(Name = "log_prof_counters", Description = "Takes current profiling counters and log them to console", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Log()
	{
		All.ForEach(Log);
	}

	private static void Log(Counter c)
	{
		double median = c.GetMedian();
		double max = c.GetMax();
		string messageFormat = $"{c.Name}: med {median:0.00} ms, max {max:0.00} ms";
		if (median > c.WarningLevel)
		{
			PFLog.SmartConsole.Error(messageFormat);
		}
		else if (max > c.WarningLevel)
		{
			PFLog.SmartConsole.Warning(messageFormat);
		}
		else
		{
			PFLog.SmartConsole.Log(messageFormat);
		}
	}
}
