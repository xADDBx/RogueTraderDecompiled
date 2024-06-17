using System;
using UnityEngine;

namespace Kingmaker.Utility;

public static class SurfaceRaycastUtils
{
	private const float MAX_RAYCAST_DISTANCE = 75f;

	private static readonly RaycastHit[] RaycastHits = new RaycastHit[10];

	public static Vector3 RaycastPointToSurface(Vector3 point, float raycastHeightOffset, int raycastLayers)
	{
		int hitsCount = Physics.RaycastNonAlloc(point + Vector3.up * raycastHeightOffset, Vector3.down, RaycastHits, 75f, raycastLayers);
		RaycastHit? raycastHit = PickRaycastHit(RaycastHits, hitsCount);
		if (!raycastHit.HasValue)
		{
			Debug.LogError(string.Format("{0}.{1}: failed to raycast {2} {3} - no hits! Check {4} and {5}.", "SurfaceRaycastUtils", "RaycastPointToSurface", "point", point, "raycastHeightOffset", "raycastLayers"));
			return point;
		}
		return raycastHit.Value.point;
	}

	private static RaycastHit? PickRaycastHit(RaycastHit[] hits, int hitsCount)
	{
		RaycastHit? result = null;
		for (int i = 0; i < Math.Min(hits.Length, hitsCount); i++)
		{
			RaycastHit value = hits[i];
			if (!result.HasValue || value.distance < result.Value.distance)
			{
				result = value;
			}
		}
		return result;
	}
}
