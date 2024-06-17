using Unity.Burst;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowProcessData
{
	public int LightDescriptorIndex;

	public ShadowData ShadowData;
}
