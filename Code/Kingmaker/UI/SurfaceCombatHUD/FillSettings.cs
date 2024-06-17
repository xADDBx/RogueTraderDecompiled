using System;

namespace Kingmaker.UI.SurfaceCombatHUD;

[Serializable]
public struct FillSettings
{
	public float borderCutSize;

	public float borderFadeSize;

	public bool mergeSubMeshes;

	public static FillSettings Default => default(FillSettings);
}
