using Owlcat.Runtime.Visual.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct PrecalculatedDirectionalShadowData
{
	public float4 FrustumSizeArray;

	public FixedArray4<ShadowSplitData> SplitDataArray;

	public FixedArray4<ShadowFaceData> FaceDataArray;
}
