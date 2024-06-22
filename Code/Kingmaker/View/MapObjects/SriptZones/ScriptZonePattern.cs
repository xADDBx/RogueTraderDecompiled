using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects.SriptZones;

[KnowledgeDatabaseID("1186500031e84dd4ab2e3f90455c05e3")]
public class ScriptZonePattern : MonoBehaviour, IScriptZoneShape
{
	private NodeList m_GridData = NodeList.Empty;

	private CustomGridNodeBase m_PatternApplicationNode;

	private Bounds m_Bounds;

	public bool ApplicationNodeExists => m_PatternApplicationNode != null;

	public NodeList CoveredNodes => m_GridData;

	public Vector3 Center()
	{
		return m_PatternApplicationNode.Vector3Position;
	}

	public void SetPattern([NotNull] CustomGridNodeBase appliedNode, float appliedPositionY, in OrientedPatternData pattern)
	{
		m_PatternApplicationNode = appliedNode;
		CustomGridGraph customGridGraph = (CustomGridGraph)appliedNode.Graph;
		PatternGridData pattern2 = PatternGridData.Create(pattern.Nodes.Select((CustomGridNodeBase i) => i.CoordinatesInGrid).ToTempHashSet(), disposable: false);
		try
		{
			m_GridData = new NodeList(customGridGraph, in pattern2);
			Vector3 min = new Vector3((float)pattern2.Bounds.xmin - 0.5f, -1f, (float)pattern2.Bounds.ymin - 0.5f);
			Vector3 max = new Vector3((float)pattern2.Bounds.xmax + 0.5f, 1f, (float)pattern2.Bounds.ymax + 0.5f);
			Bounds bounds = default(Bounds);
			bounds.SetMinMax(min, max);
			m_Bounds = customGridGraph.transform.Transform(bounds);
		}
		finally
		{
			((IDisposable)pattern2).Dispose();
		}
	}

	public bool Contains(Vector3 point, IntRect size)
	{
		using (ProfileScope.New("ScriptZonePattern.Contains"))
		{
			CustomGridNodeBase nearestNodeXZUnwalkable = point.GetNearestNodeXZUnwalkable();
			return Contains(nearestNodeXZUnwalkable, size);
		}
	}

	public bool Contains(CustomGridNodeBase node, IntRect size)
	{
		if (m_PatternApplicationNode == null)
		{
			return false;
		}
		using (ProfileScope.New("ScriptZonePattern.Contains"))
		{
			if (size.Width <= 1 && size.Height <= 1)
			{
				return m_GridData.Contains(node);
			}
			foreach (CustomGridNodeBase occupiedNode in GridAreaHelper.GetOccupiedNodes(node, size))
			{
				if (m_GridData.Contains(occupiedNode))
				{
					return true;
				}
			}
			return false;
		}
	}

	public void DrawGizmos()
	{
	}

	public Bounds GetBounds()
	{
		return m_Bounds;
	}
}
