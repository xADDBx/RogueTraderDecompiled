using Owlcat.Runtime.UI.VirtualListSystem.Grid;
using Owlcat.Runtime.UI.VirtualListSystem.Horizontal;
using Owlcat.Runtime.UI.VirtualListSystem.Vertical;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal static class LayoutEngineFabric
{
	public static IVirtualListLayoutEngine CreateLayoutEngine(IVirtualListLayoutSettings settings, VirtualListLayoutEngineContext engineContext, VirtualListDistancesCalculator distancesCalculator, RectTransform viewport, RectTransform content)
	{
		if (!(settings is VirtualListLayoutSettingsVertical settings2))
		{
			if (!(settings is VirtualListLayoutSettingsHorizontal settings3))
			{
				if (settings is VirtualListLayoutSettingsGrid settings4)
				{
					return new VirtualListLayoutEngineGrid(settings4, engineContext, distancesCalculator);
				}
				return null;
			}
			return new VirtualListLayoutEngineHorizontal(settings3, engineContext, distancesCalculator);
		}
		return new VirtualListLayoutEngineVertical(settings2, engineContext, distancesCalculator);
	}
}
