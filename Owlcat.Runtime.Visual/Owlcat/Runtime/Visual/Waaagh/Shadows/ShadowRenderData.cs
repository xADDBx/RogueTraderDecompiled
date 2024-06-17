using Owlcat.Runtime.Visual.Collections;
using Unity.Burst;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowRenderData
{
	public FixedArray4<ShadowSplitData> SplitDataArray;

	public FixedArray4<ShadowFaceData> FaceDataArray;

	public FixedArray4<float> GPUDepthBiasArray;

	public FixedArray4<float> GPUNormalBiasArray;
}
