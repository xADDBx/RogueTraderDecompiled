using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public struct AreaSourceData
{
	public IAreaSource source;

	public Texture2D icon;

	public AreaSourceData(IAreaSource source, Texture2D icon)
	{
		this.source = source;
		this.icon = icon;
	}
}
