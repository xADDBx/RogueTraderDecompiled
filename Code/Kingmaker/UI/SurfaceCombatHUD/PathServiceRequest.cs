using Kingmaker.Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public sealed class PathServiceRequest
{
	public IAreaSource source;

	public Material material;

	public Vector3 positionOffset;

	public Transform progressTrackingTransform;

	public GridSettings GridSettings;

	public PathLineSettings PathLineSettings;

	public CustomGridGraph Graph;

	public void Clear()
	{
		source = null;
		material = null;
		positionOffset = default(Vector3);
		progressTrackingTransform = null;
		GridSettings = default(GridSettings);
		PathLineSettings = default(PathLineSettings);
		Graph = null;
	}
}
