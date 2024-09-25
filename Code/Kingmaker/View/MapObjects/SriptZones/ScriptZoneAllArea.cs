using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Pathfinding;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects.SriptZones;

[KnowledgeDatabaseID("6f15d6aee87b62048a62947fe8581029")]
public class ScriptZoneAllArea : MonoBehaviour, IScriptZoneShape
{
	public bool ApplicationNodeExists => false;

	public NodeList CoveredNodes => default(NodeList);

	public Vector3 Center()
	{
		return Vector3.zero;
	}

	public bool Contains(Vector3 point, IntRect size, Vector3 forward = default(Vector3))
	{
		return true;
	}

	public bool Contains(CustomGridNodeBase node, IntRect size, Vector3 forward = default(Vector3))
	{
		return true;
	}

	public void DrawGizmos()
	{
	}

	public Bounds GetBounds()
	{
		return default(Bounds);
	}
}
