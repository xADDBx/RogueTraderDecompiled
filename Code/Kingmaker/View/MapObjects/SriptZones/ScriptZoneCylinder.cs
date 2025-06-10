using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.View.Mechanics;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects.SriptZones;

[KnowledgeDatabaseID("519b236c7f4dff540af422613b9656a0")]
public class ScriptZoneCylinder : ScriptZoneShape
{
	private int m_PrevRadius;

	private Vector3 m_PrevPosition;

	private Vector3 m_PrevLocalScale;

	private List<Vector3> m_NodePositions;

	[FormerlySerializedAs("Center")]
	public Vector3 ScriptZoneCenter;

	public int Height = 3;

	public int Radius = 3;

	public bool AlwaysShowPattern;

	private CustomGridNodeBase m_CenterNode;

	private NodeList m_Nodes = NodeList.Empty;

	private CustomGridNodeBase CenterNode => m_CenterNode ?? (m_CenterNode = base.transform.TransformPoint(ScriptZoneCenter).GetNearestNodeXZUnwalkable());

	public override NodeList CoveredNodes
	{
		get
		{
			if (!m_Nodes.IsEmpty)
			{
				return m_Nodes;
			}
			CustomGridGraph customGridGraph = (CustomGridGraph)CenterNode.Graph;
			int radius = (int)((float)Radius / customGridGraph.nodeSize);
			HashSet<Vector2Int> hashSet = TempHashSet.Get<Vector2Int>();
			GridPatterns.AddCircleNodes(hashSet, radius);
			PatternGridData patternGridData = PatternGridData.Create(hashSet, disposable: false);
			PatternGridData pattern = patternGridData.Move(CenterNode.CoordinatesInGrid);
			m_Nodes = new NodeList(customGridGraph, in pattern);
			return m_Nodes;
		}
	}

	public override bool Contains(Vector3 point, IntRect size = default(IntRect), Vector3 forward = default(Vector3))
	{
		using (ProfileScope.New("ScriptZoneCylinder.Contains"))
		{
			return (size.Width <= 1 && size.Height <= 1) ? ContainsPoint(point) : Contains(point.GetNearestNodeXZUnwalkable(), size, forward);
		}
	}

	public override bool Contains(CustomGridNodeBase node, IntRect size = default(IntRect), Vector3 forward = default(Vector3))
	{
		if ((double)forward.sqrMagnitude < 0.01)
		{
			forward = Vector3.forward;
		}
		foreach (CustomGridNodeBase node2 in GridAreaHelper.GetNodes(node, size, forward))
		{
			if (ContainsPoint(node2.Vector3Position))
			{
				return true;
			}
		}
		return false;
	}

	private bool ContainsPoint(Vector3 point)
	{
		Vector3 b = base.transform.InverseTransformPoint(point);
		if (Mathf.RoundToInt(Mathf.Abs(b.y - ScriptZoneCenter.y)) > Height)
		{
			return false;
		}
		return GetWarhammerCellDistanceSimple(ScriptZoneCenter, b) <= (float)Radius;
	}

	public override Bounds GetBounds()
	{
		Transform transform = base.transform;
		Vector3 lossyScale = transform.lossyScale;
		Bounds result = default(Bounds);
		result.center = transform.TransformPoint(ScriptZoneCenter);
		result.extents = new Vector3((float)Radius * Mathf.Max(lossyScale.x, lossyScale.z), (float)Height * lossyScale.y, (float)Radius * Mathf.Max(lossyScale.x, lossyScale.z));
		return result;
	}

	public override void DrawGizmos()
	{
		Vector3 vector = Vector3.up * Height;
		DebugDraw.DrawCircle(ScriptZoneCenter + vector, Vector3.up, Radius);
		DebugDraw.DrawCircle(ScriptZoneCenter - vector, Vector3.up, Radius);
		for (int i = 0; i < 6; i++)
		{
			Vector3 vector2 = Quaternion.Euler(0f, (float)i * 360f / 6f, 0f) * Vector3.forward;
			Vector3 vector3 = ScriptZoneCenter + Radius * vector2;
			DebugDraw.DrawLine(vector3 - vector, vector3 + vector);
		}
	}

	private void OnDrawGizmos()
	{
		if (AlwaysShowPattern)
		{
			GizmoHelper.ShowPointsInsideScriptZone(this);
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (!AlwaysShowPattern)
		{
			GizmoHelper.ShowPointsInsideScriptZone(this);
		}
	}

	public override Vector3 Center()
	{
		return base.transform.TransformPoint(ScriptZoneCenter);
	}

	private float GetWarhammerCellDistanceSimple(Vector3 a, Vector3 b)
	{
		return Vector3.Magnitude((b - a).ToXZ());
	}
}
