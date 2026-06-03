using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class GraphUpdateRouter
{
	private static readonly List<Bounds> Disabled = new List<Bounds>();

	private static readonly List<GraphUpdateObject> ScheduledGuos = new List<GraphUpdateObject>();

	private static readonly Predicate<Bounds> QueueRectPredicate = (Bounds bounds) => QueueRect(bounds, 0);

	private static UpdateHook UpdateHook { get; set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Init()
	{
		AstarPath.OnLatePostScan = (OnScanDelegate)Delegate.Combine(AstarPath.OnLatePostScan, new OnScanDelegate(OnLatePostScan));
		NavmeshClipper.AddEnableCallback(OnEnable, OnDisable);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void AfterLoad()
	{
		UpdateHook = new GameObject().AddComponent<UpdateHook>();
		UnityEngine.Object.DontDestroyOnLoad(UpdateHook.gameObject);
		UpdateHook.gameObject.hideFlags = HideFlags.HideAndDontSave;
		UpdateHook.OnUpdate += UpdateAll;
	}

	private static void OnLatePostScan(AstarPath script)
	{
		ScheduledGuos.Clear();
		ForceUpdateAll();
	}

	public static void ForceUpdateAll()
	{
		Disabled.Clear();
		foreach (NavmeshClipper item in NavmeshClipper.allEnabled)
		{
			item.ForceUpdate();
		}
		UpdateAll();
	}

	private static void OnEnable(NavmeshClipper clipper)
	{
		if (UpdateCurrentPosImpl(clipper))
		{
			clipper.NotifyUpdated();
		}
	}

	private static void OnDisable(NavmeshClipper clipper)
	{
		if (!UpdateCurrentPosImpl(clipper))
		{
			Rect bounds = clipper.GetBounds(GraphTransform.identityTransform);
			Bounds item = new Bounds(bounds.center.To3D(), bounds.size.To3D(1000f));
			Disabled.Add(item);
		}
	}

	private static bool UpdateCurrentPosImpl(NavmeshClipper clipper)
	{
		int tag = 0;
		if (clipper is INodesTagProvider nodesTagProvider)
		{
			tag = nodesTagProvider.Tag;
		}
		return QueueRect(BoundsFromRect(clipper.GetBounds(GraphTransform.identityTransform)), tag);
	}

	private static void UpdatePrevPosImpl(NavmeshClipper clipper)
	{
		Bounds bounds;
		if (!(clipper is NavmeshCut navmeshCut))
		{
			if (!(clipper is GridNavmeshModifier gridNavmeshModifier))
			{
				return;
			}
			Rect lastUpdatedBounds = gridNavmeshModifier.GetLastUpdatedBounds(GraphTransform.identityTransform);
			if (lastUpdatedBounds == default(Rect))
			{
				return;
			}
			bounds = BoundsFromRect(lastUpdatedBounds);
			if (bounds == BoundsFromRect(clipper.GetBounds(GraphTransform.identityTransform)))
			{
				return;
			}
		}
		else
		{
			bounds = navmeshCut.lastBounds;
			if (bounds == default(Bounds))
			{
				return;
			}
		}
		if (!QueueRect(bounds, 0))
		{
			Disabled.Add(bounds);
		}
	}

	private static Bounds BoundsFromRect(Rect rect)
	{
		return new Bounds(rect.center.To3D(), rect.size.To3D(1000f));
	}

	private static bool QueueRect(Bounds bounds, int tag)
	{
		if (!AstarPath.active)
		{
			return false;
		}
		BlueprintAreaPart currentlyLoadedAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
		if (currentlyLoadedAreaPart == null || !currentlyLoadedAreaPart.Bounds)
		{
			return false;
		}
		if (currentlyLoadedAreaPart.Bounds.MechanicBounds.ContainsXZ(bounds.min) && currentlyLoadedAreaPart.Bounds.MechanicBounds.ContainsXZ(bounds.max))
		{
			GraphUpdateObject item = new GraphUpdateObject(bounds)
			{
				updatePhysics = true,
				modifyTag = true,
				setTag = tag
			};
			ScheduledGuos.Add(item);
			return true;
		}
		return false;
	}

	private static void Update(NavmeshClipper clipper)
	{
		UpdatePrevPosImpl(clipper);
		if (UpdateCurrentPosImpl(clipper))
		{
			clipper.NotifyUpdated();
		}
	}

	private static void UpdateAll()
	{
		for (int i = 0; i < NavmeshClipper.allEnabled.Count; i++)
		{
			NavmeshClipper navmeshClipper = NavmeshClipper.allEnabled[i];
			if (navmeshClipper.RequiresUpdate())
			{
				Update(navmeshClipper);
			}
		}
		Disabled.RemoveAll(QueueRectPredicate);
		foreach (GraphUpdateObject scheduledGuo in ScheduledGuos)
		{
			AstarPath.active.UpdateGraphs(scheduledGuo);
		}
		ScheduledGuos.Clear();
	}
}
