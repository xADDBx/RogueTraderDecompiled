using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.SpaceCombat.Debug;

public class FootprintGizmo : MonoBehaviour
{
	[Header("Layers")]
	public bool ShowRealFootprint = true;

	public bool ShowDistanceCheckFootprint = true;

	public bool ShowRawDecomposition;

	public bool ShowBbox;

	[Header("Filters")]
	[Tooltip("Skip 1x1 entities — single-cell footprints can't drift, drawing them is noise.")]
	public bool SkipSquare1x1 = true;

	[Header("Appearance")]
	public float BoxHeight = 0.1f;

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying || Game.Instance?.State == null || AstarPath.active == null)
		{
			return;
		}
		CustomGridGraph customGridGraph = null;
		NavGraph[] graphs = AstarPath.active.graphs;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] is CustomGridGraph customGridGraph2)
			{
				customGridGraph = customGridGraph2;
				break;
			}
		}
		if (customGridGraph == null)
		{
			return;
		}
		float gridCellSize = GraphParamsMechanicsCache.GridCellSize;
		Vector3 boxSize = new Vector3(gridCellSize * 0.95f, BoxHeight, gridCellSize * 0.95f);
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (allUnit == null)
			{
				continue;
			}
			IntRect sizeRect = allUnit.SizeRect;
			if (SkipSquare1x1 && sizeRect.Width == 1 && sizeRect.Height == 1)
			{
				continue;
			}
			if (ShowRealFootprint)
			{
				DrawRealFootprint(allUnit, boxSize);
			}
			CustomGridNodeBase nearestNodeXZ = allUnit.Position.GetNearestNodeXZ();
			if (nearestNodeXZ != null)
			{
				if (ShowDistanceCheckFootprint)
				{
					DrawDistanceCheckFootprint(allUnit, nearestNodeXZ, customGridGraph, boxSize);
				}
				if (ShowRawDecomposition)
				{
					DrawRawDecomposition(allUnit, nearestNodeXZ, customGridGraph, boxSize);
				}
				if (ShowBbox)
				{
					DrawRealBbox(allUnit, nearestNodeXZ, customGridGraph, boxSize);
				}
			}
		}
	}

	private void DrawRealFootprint(MechanicEntity unit, Vector3 boxSize)
	{
		Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
		foreach (CustomGridNodeBase occupiedNode in unit.GetOccupiedNodes())
		{
			if (occupiedNode != null)
			{
				Gizmos.DrawCube(occupiedNode.Vector3Position + Vector3.up * (boxSize.y * 0.5f), boxSize);
			}
		}
	}

	private void DrawDistanceCheckFootprint(MechanicEntity unit, CustomGridNodeBase anchor, CustomGridGraph graph, Vector3 boxSize)
	{
		WarhammerGeometryUtils.SquaredFootprint footprint = new WarhammerGeometryUtils.SquaredFootprint(unit.SizeRect, unit.Forward);
		Vector2Int vector2Int = WarhammerGeometryUtils.ComputeCenteringShiftCells(unit.SizeRect, footprint);
		Gizmos.color = new Color(0.2f, 0.4f, 1f, 0.95f);
		DrawSquaredFootprintCells(graph, footprint, anchor.XCoordinateInGrid + vector2Int.x, anchor.ZCoordinateInGrid + vector2Int.y, boxSize, boxSize.y * 1.5f);
	}

	private void DrawRawDecomposition(MechanicEntity unit, CustomGridNodeBase anchor, CustomGridGraph graph, Vector3 boxSize)
	{
		WarhammerGeometryUtils.SquaredFootprint footprint = new WarhammerGeometryUtils.SquaredFootprint(unit.SizeRect, unit.Forward);
		Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
		DrawSquaredFootprintCells(graph, footprint, anchor.XCoordinateInGrid, anchor.ZCoordinateInGrid, boxSize, boxSize.y * 1f);
	}

	private void DrawRealBbox(MechanicEntity unit, CustomGridNodeBase anchor, CustomGridGraph graph, Vector3 boxSize)
	{
		int direction = CustomGraphHelper.GuessDirection(unit.Forward);
		IntRect bounds = GridAreaHelper.GetOffsets(unit.SizeRect, direction).Bounds;
		Gizmos.color = new Color(1f, 0.95f, 0.2f, 0.9f);
		for (int i = bounds.xmin; i <= bounds.xmax; i++)
		{
			for (int j = bounds.ymin; j <= bounds.ymax; j++)
			{
				CustomGridNodeBase node = graph.GetNode(anchor.XCoordinateInGrid + i, anchor.ZCoordinateInGrid + j);
				if (node != null)
				{
					Gizmos.DrawWireCube(node.Vector3Position + Vector3.up * (boxSize.y * 2f), boxSize);
				}
			}
		}
	}

	private static void DrawSquaredFootprintCells(CustomGridGraph graph, WarhammerGeometryUtils.SquaredFootprint footprint, int anchorX, int anchorZ, Vector3 boxSize, float verticalOffset)
	{
		for (int i = 0; i < footprint.Count; i++)
		{
			Vector2Int vector2Int = footprint.SubRectOffset(i);
			int num = anchorX + vector2Int.x;
			int num2 = anchorZ + vector2Int.y;
			for (int j = footprint.SubRect.xmin; j <= footprint.SubRect.xmax; j++)
			{
				for (int k = footprint.SubRect.ymin; k <= footprint.SubRect.ymax; k++)
				{
					CustomGridNodeBase node = graph.GetNode(num + j, num2 + k);
					if (node != null)
					{
						Gizmos.DrawWireCube(node.Vector3Position + Vector3.up * verticalOffset, boxSize);
					}
				}
			}
		}
	}
}
