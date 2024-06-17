using System;

namespace Kingmaker.UI.SurfaceCombatHUD;

[Serializable]
public struct PathLineSettings
{
	public float thickness;

	public int smoothSegmentsCount;

	public float edgeSmoothDistance;

	public float turnSmoothDistance;

	public float stepSmoothDistance;

	public float hardTurnSmoothDistanceFactor;

	public float edgePenetrationThreshold;

	public float edgeHoverThreshold;

	public float stepHeightDeltaThreshold;

	public float stepOffset;

	public static PathLineSettings Default
	{
		get
		{
			PathLineSettings result = default(PathLineSettings);
			result.thickness = 0.2f;
			result.smoothSegmentsCount = 6;
			result.edgeSmoothDistance = 0.3f;
			result.turnSmoothDistance = 0.3f;
			result.stepSmoothDistance = 0.1f;
			result.hardTurnSmoothDistanceFactor = 1f;
			result.edgePenetrationThreshold = 0f;
			result.edgeHoverThreshold = 10f;
			result.stepHeightDeltaThreshold = 0.1f;
			result.stepOffset = 0.2f;
			return result;
		}
	}
}
