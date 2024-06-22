using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Visual.FogOfWar;

public static class FogOfWarSettingsExtensions
{
	public static Vector2 CalculateHeightMinMax([NotNull] this FogOfWarSettings fogOfWarSettings, float y)
	{
		float num = y + fogOfWarSettings.ShadowCullingHeightOffset;
		float y2 = num + fogOfWarSettings.ShadowCullingHeight;
		return new Vector2(num, y2);
	}
}
