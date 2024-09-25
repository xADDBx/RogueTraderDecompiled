using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

public class LocalVolumetricFogCullingPassData : PassDataBase
{
	public Texture3DAtlas Atlas;

	public NativeArray<LocalVolumetricFogBounds> VisibleVolumeBoundsList;

	public NativeArray<LocalVolumetricFogEngineData> VisibleVolumeDataList;

	public NativeArray<ZBin> ZBins;

	public int VisibleVolumesCount;

	public ComputeBufferHandle VisibleVolumeBoundsBuffer;

	public ComputeBufferHandle VisibleVolumeDataBuffer;

	public ComputeBufferHandle FogTilesBuffer;

	public ComputeBufferHandle ZBinsBuffer;

	public int FogTilesBufferSize;

	public ComputeShader CullingShader;

	public ComputeShaderKernelDescriptor BuildFogTilesKernelDesc;

	public TextureHandle TileMinMaxZTexture;

	public Vector4 ClusteringParams;

	public Matrix4x4 ScreenProjMatrix;

	public int3 DispatchSize;
}
