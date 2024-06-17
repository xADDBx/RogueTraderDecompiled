using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public interface IAreaSource
{
	int EstimateCount();

	void GetCellIdentifiers<T>(Vector2Int gridDimensions, ref T container) where T : struct, IIdentifierContainer;
}
