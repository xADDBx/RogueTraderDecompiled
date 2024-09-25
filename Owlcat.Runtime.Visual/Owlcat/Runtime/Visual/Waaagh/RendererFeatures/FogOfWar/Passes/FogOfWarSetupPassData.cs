using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar.Passes;

public class FogOfWarSetupPassData : PassDataBase
{
	public FogOfWarArea Area;

	public FogOfWarSettings Settings;

	public Vector2 MaskSize;

	public Texture2D DefaultFogOfWarMask;
}
