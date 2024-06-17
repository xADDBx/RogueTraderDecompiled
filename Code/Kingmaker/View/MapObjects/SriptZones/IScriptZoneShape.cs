using Kingmaker.Pathfinding;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects.SriptZones;

public interface IScriptZoneShape
{
	NodeList CoveredNodes { get; }

	bool Contains(Vector3 point, IntRect size = default(IntRect));

	bool Contains(CustomGridNodeBase node, IntRect size = default(IntRect));

	Bounds GetBounds();

	Vector3 Center();
}
