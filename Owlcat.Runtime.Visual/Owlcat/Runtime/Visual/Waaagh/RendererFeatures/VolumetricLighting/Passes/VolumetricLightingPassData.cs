using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

public class VolumetricLightingPassData : PassDataBase
{
	public Material ShadowmapDownsampleMaterial;

	public ComputeShader VoxelizationShader;

	public ComputeShader LightingShader;

	public ComputeShader ScatterShader;

	public TextureHandle VoxelizedSceneTexture;

	public TextureHandle LightingHistoryTexture;

	public TextureHandle ScatterTexture;

	public Texture2D BlueNoiseTexture;

	public TextureHandle CameraDepthCopy;

	public TextureHandle Shadowmap;

	public TextureHandle ShadowmapDownsampled;

	public TextureHandle TilesMinMaxZ;

	public int3 VoxelizationDispatchSize;

	public int3 LightingDispatchSize;

	public int3 ScatterDispatchSize;

	public Vector4 LightingTextureSize;

	public Matrix4x4 ViewProjMatrix;

	public Matrix4x4 InvViewProjMatrix;

	public Matrix4x4 PrevViewProjMatrix;

	public Vector4 VolumetricProjectionParams;

	public Vector4 HeightFogParams;

	public Vector4 DownsampledShadowmapSize;

	public float BlueNoiseTextureSize;

	public Vector4 ScatteringExtinction;

	public float Anisotropy;

	public bool TricubicDeferred;

	public bool TricubicForward;

	public float LightShadows;

	public float TemporalFeedback;

	public float AmbientLightScale;

	public bool HighRes;

	public bool TemporalAccumulation;

	public bool UseDownsampledShadowmap;

	public bool LocalVolumesEnabled;

	public Vector4 LocalVolumetricFogClusteringParams;

	public ComputeBufferHandle LocalFogBoundsBuffer;

	public ComputeBufferHandle LocalFogGpuDataBuffer;

	public ComputeBufferHandle LocalFogTilesBuffer;

	public ComputeBufferHandle LocalFogZBinsBuffer;

	public Matrix4x4 ScreenProjMatrix;

	public Texture VolumeMaskAtlas;

	public bool SkipFirstFrameTemporalAccumulation;
}
