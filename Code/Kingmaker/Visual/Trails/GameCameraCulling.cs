using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Visual.Trails;

public static class GameCameraCulling
{
	private const float CullDistance = 2f;

	[NotNull]
	private static readonly Plane[] s_CullingPlanes = new Plane[6];

	private static int s_LastUpdateFrame = -1;

	public static bool IsCulled(Vector3 p)
	{
		if (s_LastUpdateFrame != Time.frameCount)
		{
			s_LastUpdateFrame = Time.frameCount;
			Camera camera = Game.GetCamera();
			if (camera != null)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, s_CullingPlanes);
			}
		}
		Plane[] array = s_CullingPlanes;
		foreach (Plane plane in array)
		{
			if (plane.GetDistanceToPoint(p) < -2f)
			{
				return true;
			}
		}
		return false;
	}
}
