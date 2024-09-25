using Owlcat.Runtime.Visual.Lighting;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Lighting;

public struct LightDescriptor
{
	public VisibleLight VisibleLight;

	public float MinZ;

	public float MaxZ;

	public float ShadowStrength;

	public float InnerRadius;

	public bool IsBaked;

	public LightFalloffType LightFalloffType;

	public bool SnapSpecularToInnerRadius;

	public float InnerSpotAngle;

	public int ShadowDataIndex;

	public int ShadowmaskChannel;
}
