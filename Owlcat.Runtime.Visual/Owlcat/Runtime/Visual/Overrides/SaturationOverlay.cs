using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.ObjectSaturationAura;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

public class SaturationOverlay : VolumeComponent, IPostProcessComponent
{
	public bool IsActive()
	{
		return SaturationAuraCarrier.All.Count > 0;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
