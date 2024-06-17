using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.View.Mechanics;
using UnityEngine;

namespace Kingmaker.Code.View.Mechanics;

public static class GizmoHelper
{
	private static List<Vector3> s_MechanicEntityNodePositions;

	private static readonly Dictionary<IScriptZoneShape, ScriptZonesData> AllScriptZones = new Dictionary<IScriptZoneShape, ScriptZonesData>();

	[Conditional("UNITY_EDITOR")]
	public static void Add(IScriptZoneShape shape, Vector3 position, Vector3 scale)
	{
		AllScriptZones.Add(shape, new ScriptZonesData(position, scale));
	}

	[Conditional("UNITY_EDITOR")]
	public static void Remove(IScriptZoneShape shape)
	{
		AllScriptZones.Remove(shape);
	}

	public static void ShowPointsInsideScriptZone(IScriptZoneShape shape)
	{
		ScriptZonesData scriptZonesData = AllScriptZones[shape];
		if (scriptZonesData.Position.Equals(shape.Center()) || scriptZonesData.Scale != shape.GetBounds().extents)
		{
			scriptZonesData.Position = shape.Center();
			scriptZonesData.Scale = shape.GetBounds().extents;
			scriptZonesData.NodePositions = EditorGridHelper.GetPointsInsideScriptZone(shape, shape.Center()).EmptyIfNull().ToList();
		}
		DrawCircles(scriptZonesData.NodePositions);
	}

	private static void DrawCircles(IEnumerable<Vector3> nodePositions)
	{
		foreach (Vector3 nodePosition in nodePositions)
		{
			DebugDraw.DrawCircle(nodePosition, Vector3.up, 1.Cells().Meters / 2f);
		}
	}

	public static void ShowCellsCoveredByMechanicEntity([CanBeNull] MechanicEntity data, MechanicEntityView view)
	{
		s_MechanicEntityNodePositions = EditorGridHelper.GetCellsCoveredByMechanicEntity(data, view)?.ToList();
		if (s_MechanicEntityNodePositions != null)
		{
			DrawCubes(s_MechanicEntityNodePositions);
		}
	}

	private static void DrawCubes(IEnumerable<Vector3> nodePositions)
	{
		Gizmos.color = Color.magenta;
		foreach (Vector3 nodePosition in nodePositions)
		{
			Gizmos.DrawWireCube(nodePosition + Vector3.up * 1.Cells().Meters / 2f, Vector3.one * 1.Cells().Meters);
		}
	}
}
