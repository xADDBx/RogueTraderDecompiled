using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Pathfinding;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects.SriptZones;

[ExecuteInEditMode]
[KnowledgeDatabaseID("a0d84e3ff7c445ccbc2023bddafd5acf")]
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
