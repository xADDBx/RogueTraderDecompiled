using Kingmaker.Pathfinding;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects.SriptZones;

public interface IScriptZoneShape
{
	NodeList CoveredNodes { get; }

	bool Contains(Vector3 point, IntRect size = default(IntRect), Vector3 forward = default(Vector3));

	bool Contains(CustomGridNodeBase node, IntRect size = default(IntRect), Vector3 forward = default(Vector3));

	Bounds GetBounds();

	Vector3 Center();
}
