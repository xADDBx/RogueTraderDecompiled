using Kingmaker.Pathfinding;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects.SriptZones;

[ExecuteInEditMode]
public abstract class ScriptZoneShape : MonoBehaviour, IScriptZoneShape
{
	public virtual NodeList CoveredNodes => NodeList.Empty;

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	public abstract bool Contains(Vector3 point, IntRect size = default(IntRect));

	public abstract bool Contains(CustomGridNodeBase node, IntRect size = default(IntRect));

	public abstract Bounds GetBounds();

	public abstract void DrawGizmos();

	public abstract Vector3 Center();
}
