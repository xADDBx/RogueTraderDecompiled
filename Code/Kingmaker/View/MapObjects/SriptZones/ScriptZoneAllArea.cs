using Kingmaker.Pathfinding;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects.SriptZones;

public class ScriptZoneAllArea : MonoBehaviour, IScriptZoneShape
{
	public bool ApplicationNodeExists => false;

	public NodeList CoveredNodes => default(NodeList);

	public Vector3 Center()
	{
		return Vector3.zero;
	}

	public bool Contains(Vector3 point, IntRect size)
	{
		return true;
	}

	public bool Contains(CustomGridNodeBase node, IntRect size)
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
