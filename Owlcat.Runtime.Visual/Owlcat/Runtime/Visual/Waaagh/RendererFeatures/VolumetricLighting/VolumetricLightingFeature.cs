using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Jobs;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Volumetric Lighting")]
public class VolumetricLightingFeature : ScriptableRendererFeature
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/LocalVolumetricFogCulling.compute", ReloadAttribute.Package.Root)]
		public ComputeShader LocalVolumetricFogCullingCS;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/DebugLocalVolumetricFog.shader", ReloadAttribute.Package.Root)]
		public Shader DebugLocalVolumetricFogPS;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/VolumetricShadowmapDownsample.shader", ReloadAttribute.Package.Root)]
		public Shader ShadowmapDownsampleShader;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/VolumetricSceneVoxelization.compute", ReloadAttribute.Package.Root)]
		public ComputeShader VoxelizationShader;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/VolumetricLighting.compute", ReloadAttribute.Package.Root)]
		public ComputeShader LightingShader;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/VolumetricScatter.compute", ReloadAttribute.Package.Root)]
		public ComputeShader ScatterShader;

		[SerializeField]
		[Reload("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/VolumetricApplyOpaque.shader", ReloadAttribute.Package.Root)]
		public Shader ApplyOpaqueShader;
	}

	[Serializable]
	[ReloadGroup]
	public sealed class TextureResources
	{
		[Reload("Shaders/PostProcessing/Textures/BlueNoise16/L/LDR_LLL1_{0}.png", 0, 32, ReloadAttribute.Package.Root)]
		public Texture2D[] BlueNoise16LTex;
	}

	internal const int kMaxLocalVolumetricFogCount = 512;

	private LocalVolumetricFogCullingPass m_LocalVolumetricFogCullingPass;

	private VolumetricLightingPass m_VolumetricLightingPass;

	private VolumetricLightingApplyOpaquePass m_VolumetricLightingApplyOpaquePass;

	private DebugLocalVolumetricFogPass m_DebugLocalVolumetricFogPass;

	private Material m_ShadowmapDownsampleMaterial;

	private Material m_ApplyOpaqueMaterial;

	private Material m_LocalVolumetricFogDebugMaterial;

	private ComputeBuffer m_VisibleVolumeBoundsBuffer;

	private ComputeBuffer m_VisibleVolumeDataBuffer;

	private ComputeBuffer m_FogTilesBuffer;

	private ComputeBuffer m_ZBinsBuffer;

	private JobHandle m_SetupJobHandle;

	private NativeArray<LocalVolumetricFogBounds> m_VisibleVolumeBoundsList;

	private NativeArray<LocalVolumetricFogEngineData> m_VisibleVolumeDataList;

	private NativeArray<int> m_VisibleCounter;

	private NativeArray<LocalFogDescriptor> m_LocalFogDescriptors;

	private NativeArray<ZBin> m_ZBins;

	private Frustum m_Frustum;

	private LocalFogComparer m_LocalFogComparer;

	private Vector4 m_FogClusteringParams;

	public ShaderResources Shaders;

	public TextureResources Textures;

	public VolumetricLightingSettings Settings;

	internal ComputeBufferHandle FogTilesBufferHandle;

	internal ComputeBufferHandle VisibleVolumesBoundsBufferHandle;

	internal ComputeBufferHandle VisibleVolumesDataBufferHandle;

	internal ComputeBufferHandle ZBinsBufferHandle;

	public Material ShadowmapDownsampleMaterial => m_ShadowmapDownsampleMaterial;

	public ComputeBuffer VisibleVolumeBoundsBuffer => m_VisibleVolumeBoundsBuffer;

	public ComputeBuffer VisibleVolumeDataBuffer => m_VisibleVolumeDataBuffer;

	public ComputeBuffer FogTilesBuffer => m_FogTilesBuffer;

	public ComputeBuffer ZBinsBuffer => m_ZBinsBuffer;

	public NativeArray<LocalVolumetricFogBounds> VisibleVolumeBoundsList => m_VisibleVolumeBoundsList;

	public NativeArray<LocalVolumetricFogEngineData> VisibleVolumeDataList => m_VisibleVolumeDataList;

	public NativeArray<ZBin> ZBins => m_ZBins;

	public Vector4 FogClusteringParams => m_FogClusteringParams;

	public int VisibleVolumesCount
	{
		get
		{
			if (m_VisibleCounter.IsCreated)
			{
				return m_VisibleCounter[0];
			}
			return 0;
		}
	}

	public override void Create()
	{
		m_ShadowmapDownsampleMaterial = CoreUtils.CreateEngineMaterial(Shaders.ShadowmapDownsampleShader);
		m_ApplyOpaqueMaterial = CoreUtils.CreateEngineMaterial(Shaders.ApplyOpaqueShader);
		m_LocalVolumetricFogDebugMaterial = CoreUtils.CreateEngineMaterial(Shaders.DebugLocalVolumetricFogPS);
		m_LocalVolumetricFogCullingPass = new LocalVolumetricFogCullingPass(RenderPassEvent.AfterRenderingDeferredLights, this);
		m_VolumetricLightingPass = new VolumetricLightingPass(RenderPassEvent.AfterRenderingDeferredLights, this);
		m_VolumetricLightingApplyOpaquePass = new VolumetricLightingApplyOpaquePass((RenderPassEvent)401, m_ApplyOpaqueMaterial);
		m_DebugLocalVolumetricFogPass = new DebugLocalVolumetricFogPass((RenderPassEvent)401, this, m_LocalVolumetricFogDebugMaterial);
		if (m_VisibleVolumeBoundsBuffer == null)
		{
			m_VisibleVolumeBoundsBuffer = new ComputeBuffer(512, Marshal.SizeOf<LocalVolumetricFogBounds>(), ComputeBufferType.Structured);
			m_VisibleVolumeBoundsBuffer.name = "VisibleLocalVolumetricFogBoundsBuffer";
		}
		if (m_VisibleVolumeDataBuffer == null)
		{
			m_VisibleVolumeDataBuffer = new ComputeBuffer(512, Marshal.SizeOf<LocalVolumetricFogEngineData>(), ComputeBufferType.Structured);
			m_VisibleVolumeDataBuffer.name = "VisibleLocalVolumetricFogDataBuffer";
		}
		if (m_ZBinsBuffer == null)
		{
			m_ZBinsBuffer = new ComputeBuffer(1024, Marshal.SizeOf<float4>(), ComputeBufferType.Structured);
			m_ZBinsBuffer.name = "VisibleZBinsBuffer";
		}
		if (!m_VisibleVolumeBoundsList.IsCreated)
		{
			m_VisibleVolumeBoundsList = new NativeArray<LocalVolumetricFogBounds>(512, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		if (!m_VisibleVolumeDataList.IsCreated)
		{
			m_VisibleVolumeDataList = new NativeArray<LocalVolumetricFogEngineData>(512, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		if (!m_VisibleCounter.IsCreated)
		{
			m_VisibleCounter = new NativeArray<int>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		if (!m_LocalFogDescriptors.IsCreated)
		{
			m_LocalFogDescriptors = new NativeArray<LocalFogDescriptor>(512, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		if (!m_ZBins.IsCreated)
		{
			m_ZBins = new NativeArray<ZBin>(4096, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			CoreUtils.Destroy(m_ShadowmapDownsampleMaterial);
			CoreUtils.Destroy(m_ApplyOpaqueMaterial);
			CoreUtils.Destroy(m_LocalVolumetricFogDebugMaterial);
			VolumetricCameraBuffers.Cleanup();
			if (m_VisibleVolumeBoundsBuffer != null)
			{
				m_VisibleVolumeBoundsBuffer.Release();
				m_VisibleVolumeBoundsBuffer = null;
			}
			if (m_VisibleVolumeDataBuffer != null)
			{
				m_VisibleVolumeDataBuffer.Release();
				m_VisibleVolumeDataBuffer = null;
			}
			if (m_FogTilesBuffer != null)
			{
				m_FogTilesBuffer.Release();
				m_FogTilesBuffer = null;
			}
			if (m_ZBinsBuffer != null)
			{
				m_ZBinsBuffer.Release();
				m_ZBinsBuffer = null;
			}
			if (m_VisibleVolumeBoundsList.IsCreated)
			{
				m_VisibleVolumeBoundsList.Dispose();
			}
			if (m_VisibleVolumeDataList.IsCreated)
			{
				m_VisibleVolumeDataList.Dispose();
			}
			if (m_VisibleCounter.IsCreated)
			{
				m_VisibleCounter.Dispose();
			}
			if (m_LocalFogDescriptors.IsCreated)
			{
				m_LocalFogDescriptors.Dispose();
			}
			if (m_ZBins.IsCreated)
			{
				m_ZBins.Dispose();
			}
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		VolumetricCameraBuffers.CleanUnused();
		if (!renderingData.CameraData.IsLightingEnabled || !(renderer is WaaaghRenderer waaaghRenderer))
		{
			return;
		}
		VolumetricFog component = VolumeManager.instance.stack.GetComponent<VolumetricFog>();
		if (component != null && component.IsActive())
		{
			if (Settings.LocalVolumesEnabled)
			{
				renderer.EnqueuePass(m_LocalVolumetricFogCullingPass);
			}
			m_VolumetricLightingPass.TileSize = waaaghRenderer.Settings.TileSize;
			m_VolumetricLightingPass.VolumetricFog = component;
			renderer.EnqueuePass(m_VolumetricLightingPass);
			renderer.EnqueuePass(m_VolumetricLightingApplyOpaquePass);
			if (Settings.DebugLocalVolumetricFog)
			{
				renderer.EnqueuePass(m_DebugLocalVolumetricFogPass);
			}
		}
	}

	internal override void StartSetupJobs(ref RenderingData renderingData)
	{
		if (renderingData.CameraData.IsLightingEnabled)
		{
			Camera camera = renderingData.CameraData.Camera;
			Matrix4x4 viewMatrix = renderingData.CameraData.GetViewMatrix();
			Matrix4x4 viewProjMatrix = CoreMatrixUtils.MultiplyProjectionMatrix(renderingData.CameraData.GetGPUProjectionMatrix(), viewMatrix, camera.orthographic);
			Frustum.Create(ref m_Frustum, viewProjMatrix, camera.transform.position, camera.transform.forward, camera.nearClipPlane, camera.farClipPlane);
			List<LocalVolumetricFog> volumes = LocalVolumetricFogManager.Instance.Volumes;
			int num = math.min(volumes.Count, 512);
			for (int i = 0; i < num; i++)
			{
				LocalVolumetricFog localVolumetricFog = volumes[i];
				localVolumetricFog.PrepareParameters(renderingData.TimeData.Time);
				m_LocalFogDescriptors[i] = new LocalFogDescriptor
				{
					Data = localVolumetricFog.Parameters.ConvertToEngineData(),
					Position = localVolumetricFog.transform.position,
					Rotation = localVolumetricFog.transform.rotation,
					Size = localVolumetricFog.Parameters.Size,
					IsVisible = false,
					MinZ = float.MaxValue,
					MaxZ = float.MaxValue,
					MeanZ = float.MaxValue
				};
			}
			TileSize tileSize = TileSize.Tile16;
			if (renderingData.CameraData.Renderer is WaaaghRenderer waaaghRenderer)
			{
				tileSize = waaaghRenderer.Settings.TileSize;
			}
			InitFogTilesBuffer(ref renderingData, tileSize);
			CullingJob jobData = default(CullingJob);
			jobData.TotalVolumesCount = num;
			jobData.CameraFrustum = m_Frustum;
			jobData.FogDescs = m_LocalFogDescriptors;
			jobData.VisibleCounter = m_VisibleCounter;
			jobData.Run();
			Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * camera.worldToCameraMatrix;
			MinMaxZJob jobData2 = default(MinMaxZJob);
			jobData2.WorldToViewMatrix = matrix4x;
			jobData2.LocalFogDescs = m_LocalFogDescriptors;
			m_SetupJobHandle = IJobParallelForExtensions.Schedule(jobData2, num, 32);
			m_SetupJobHandle = m_LocalFogDescriptors.SortJob(m_LocalFogComparer).Schedule(m_SetupJobHandle);
			int num2 = m_VisibleCounter[0];
			ZBinningJob zBinningJob = default(ZBinningJob);
			zBinningJob.CameraNearClip = camera.nearClipPlane;
			zBinningJob.ZBinFactor = m_FogClusteringParams.w;
			zBinningJob.VisibleCount = num2;
			zBinningJob.FogDescs = m_LocalFogDescriptors;
			zBinningJob.ZBins = m_ZBins;
			ZBinningJob jobData3 = zBinningJob;
			m_SetupJobHandle = jobData3.Schedule(num2, m_SetupJobHandle);
			ExtractLocalFogDataJob extractLocalFogDataJob = default(ExtractLocalFogDataJob);
			extractLocalFogDataJob.WorldToViewMatrix = matrix4x;
			extractLocalFogDataJob.LocalFogDescriptors = m_LocalFogDescriptors;
			extractLocalFogDataJob.LocalFogEngineData = m_VisibleVolumeDataList;
			extractLocalFogDataJob.Obbs = m_VisibleVolumeBoundsList;
			ExtractLocalFogDataJob jobData4 = extractLocalFogDataJob;
			m_SetupJobHandle = IJobParallelForExtensions.Schedule(jobData4, num2, 32, m_SetupJobHandle);
		}
	}

	private void InitFogTilesBuffer(ref RenderingData renderingData, TileSize tileSize)
	{
		ref RenderTextureDescriptor cameraTargetDescriptor = ref renderingData.CameraData.CameraTargetDescriptor;
		int2 @int = 1;
		@int.x = RenderingUtils.DivRoundUp(cameraTargetDescriptor.width, (int)tileSize);
		@int.y = RenderingUtils.DivRoundUp(cameraTargetDescriptor.height, (int)tileSize);
		int num = 16;
		int num2 = @int.x * @int.y * num;
		if (m_FogTilesBuffer == null || !m_FogTilesBuffer.IsValid() || m_FogTilesBuffer.count < num2)
		{
			if (m_FogTilesBuffer != null)
			{
				m_FogTilesBuffer.Release();
			}
			if (num2 > 0)
			{
				m_FogTilesBuffer = new ComputeBuffer(num2, Marshal.SizeOf<uint>(), ComputeBufferType.Structured);
				m_FogTilesBuffer.name = "_LightTilesBuffer";
			}
		}
		Camera camera = renderingData.CameraData.Camera;
		float w = 4096f / (camera.farClipPlane - camera.nearClipPlane);
		m_FogClusteringParams = new Vector4(@int.x, @int.y, (float)tileSize, w);
	}

	internal override void CompleteSetupJobs()
	{
		m_SetupJobHandle.Complete();
	}
}
