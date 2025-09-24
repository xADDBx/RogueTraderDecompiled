using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class SetupLightDataPassData : PassDataBase
{
	public BufferHandle LightDataConstantBufferHandle;

	public BufferHandle LightVolumeDataConstantBufferHandle;

	public BufferHandle ZBinsConstantBufferHandle;

	public NativeArray<float4> LightDataRaw;

	public NativeArray<float4> LightVolumeDataRaw;

	public NativeArray<ZBin> ZBins;

	public Vector4 LightDataParams;

	public Vector4 ClusteringParams;
}
