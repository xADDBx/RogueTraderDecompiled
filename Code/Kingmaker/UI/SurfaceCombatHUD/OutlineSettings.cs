using System;

namespace Kingmaker.UI.SurfaceCombatHUD;

[Serializable]
public struct OutlineSettings
{
	public float lineThickness;

	public float turnSmoothDistance;

	public int turnSmoothSegmentsCount;

	public bool mergeSubMeshes;

	public static OutlineSettings Default
	{
		get
		{
			OutlineSettings result = default(OutlineSettings);
			result.lineThickness = 0.2f;
			result.turnSmoothDistance = 0.2f;
			result.turnSmoothSegmentsCount = 6;
			return result;
		}
	}
}
