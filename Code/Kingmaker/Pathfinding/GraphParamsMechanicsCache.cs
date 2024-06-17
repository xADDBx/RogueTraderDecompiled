using System;
using System.Linq;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class GraphParamsMechanicsCache : MonoBehaviour
{
	public const float DefaultGridCellSize = 1.35f;

	public const float DefaultMaxClimb = 1.35f;

	public static float GridCellSize { get; private set; } = 1.35f;


	public static float MaxClimb { get; private set; } = 1.35f;


	public static int GraphVersionIndex { get; private set; } = 0;


	private void OnEnable()
	{
		if (AstarPath.active != null)
		{
			Rebuild(AstarPath.active);
		}
		else
		{
			GridCellSize = 1.35f;
			MaxClimb = 1.35f;
		}
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Combine(AstarPath.OnGraphsUpdated, new OnScanDelegate(Rebuild));
	}

	private void OnDisable()
	{
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Remove(AstarPath.OnGraphsUpdated, new OnScanDelegate(Rebuild));
	}

	private void Rebuild(AstarPath path)
	{
		CustomGridGraph customGridGraph = AstarPath.active.graphs.OfType<CustomGridGraph>().FirstOrDefault();
		GridCellSize = customGridGraph?.nodeSize ?? 1.35f;
		MaxClimb = customGridGraph?.maxClimb ?? 1.35f;
		GraphVersionIndex++;
	}
}
