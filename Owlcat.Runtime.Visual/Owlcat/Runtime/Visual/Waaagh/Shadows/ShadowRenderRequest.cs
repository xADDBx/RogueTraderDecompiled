using Owlcat.Runtime.Visual.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowRenderRequest
{
	public int ConstantBufferIndex;

	public int VisibleLightIndex;

	public ShadowRenderData RenderData;

	public FixedArray4<float4> ShadowMapViewports;

	public int FaceCount;

	public LightType LightType;

	public float DepthBias;

	public BatchCullingProjectionType ProjectionType;

	public FixedArray4<RendererList> RendererListArray;

	public ShadowObjectsFilter ObjectsFilter;

	public bool NeedClear;
}
