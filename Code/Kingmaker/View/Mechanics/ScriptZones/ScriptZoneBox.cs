using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.View.Mechanics;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View.MapObjects.SriptZones;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.Mechanics.ScriptZones;

[KnowledgeDatabaseID("84a8060c827087c4dbb941d7ba2f1e9a")]
public class ScriptZoneBox : ScriptZoneShape
{
	private Bounds m_PrevBounds;

	private Vector3 m_PrevPosition;

	private Vector3 m_PrevLocalScale;

	private List<Vector3> m_NodePositions;

	public Bounds Bounds = new Bounds(Vector3.zero, Vector3.one * 3f);

	public bool AlwaysShowPattern;

	public override NodeList CoveredNodes
	{
		get
		{
			int xmax = (int)(Bounds.size.x / GraphParamsMechanicsCache.GridCellSize);
			int ymax = (int)(Bounds.size.z / GraphParamsMechanicsCache.GridCellSize);
			return GridAreaHelper.GetNodes(base.transform.TransformPoint(Bounds.min), new IntRect(0, 0, xmax, ymax));
		}
	}

	public override bool Contains(Vector3 point, IntRect size = default(IntRect), Vector3 forward = default(Vector3))
	{
		using (ProfileScope.New("ScriptZoneBox.Contains"))
		{
			if (size.Width <= 1 && size.Height <= 1)
			{
				return ContainsPoint(point);
			}
			CustomGridNodeBase nearestNodeXZUnwalkable = point.GetNearestNodeXZUnwalkable();
			Vector3 vector = point - nearestNodeXZUnwalkable.Vector3Position;
			if ((double)forward.sqrMagnitude < 0.01)
			{
				forward = Vector3.forward;
			}
			foreach (CustomGridNodeBase node in GridAreaHelper.GetNodes(nearestNodeXZUnwalkable, size, forward))
			{
				if (ContainsPoint(node.Vector3Position + vector))
				{
					return true;
				}
			}
			return false;
		}
	}

	public override bool Contains(CustomGridNodeBase node, IntRect size = default(IntRect), Vector3 forward = default(Vector3))
	{
		using (ProfileScope.New("ScriptZoneBox.Contains"))
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
	}

	private bool ContainsPoint(Vector3 point)
	{
		Vector3 point2 = base.transform.InverseTransformPoint(point);
		return Bounds.Contains(point2);
	}

	public override Bounds GetBounds()
	{
		Vector3 lossyScale = base.transform.lossyScale;
		float num = Bounds.size.x * lossyScale.x;
		float num2 = Bounds.size.z * lossyScale.z;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		return new Bounds(base.transform.position, new Vector3(num3, Bounds.size.y * lossyScale.y, num3));
	}

	public override void DrawGizmos()
	{
		Gizmos.DrawWireCube(Bounds.center, Bounds.size);
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
		return base.transform.TransformPoint(Bounds.center);
	}
}
