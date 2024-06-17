using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

public class VolumetricLightingPass : ScriptableRenderPass<VolumetricLightingPassData>
{
	private static readonly int _ResultUAV = Shader.PropertyToID("_ResultUAV");

	private static readonly int _VoxelizedSceneTexture = Shader.PropertyToID("_VoxelizedSceneTexture");

	private static readonly int _HistoryTexture = Shader.PropertyToID("_HistoryTexture");

	private static readonly int _BlueNoiseTex_Size = Shader.PropertyToID("_BlueNoiseTex_Size");

	private static readonly int _VolumetricViewProj = Shader.PropertyToID("_VolumetricViewProj");

	private static readonly int _VolumetricInvViewProj = Shader.PropertyToID("_VolumetricInvViewProj");

	private static readonly int _VolumetricPrevViewProj = Shader.PropertyToID("_VolumetricPrevViewProj");

	private static readonly int _VolumetricProjectionParams = Shader.PropertyToID("_VolumetricProjectionParams");

	private static readonly int _VolumetricScatteringExtinction = Shader.PropertyToID("_VolumetricScatteringExtinction");

	private static readonly int _VolumetricHeightFogScatteringExtinction = Shader.PropertyToID("_VolumetricHeightFogScatteringExtinction");

	private static readonly int _VolumetricHeightFogParams = Shader.PropertyToID("_VolumetricHeightFogParams");

	private static readonly int _Anisotropy = Shader.PropertyToID("_Anisotropy");

	private static readonly int _VolumeScatter_Size = Shader.PropertyToID("_VolumeScatter_Size");

	private static readonly int _VolumtricTricubicDeferred = Shader.PropertyToID("_VolumtricTricubicDeferred");

	private static readonly int _VolumtricTricubicForward = Shader.PropertyToID("_VolumtricTricubicForward");

	private static readonly int _VolumetricLightShadows = Shader.PropertyToID("_VolumetricLightShadows");

	private static readonly int _TemporalFeedback = Shader.PropertyToID("_TemporalFeedback");

	private static readonly int _VolumetricAmbientLightScale = Shader.PropertyToID("_VolumetricAmbientLightScale");

	private static readonly int _OutputSize = Shader.PropertyToID("_OutputSize");

	private static readonly int _VolumeInject = Shader.PropertyToID("_VolumeInject");

	private static readonly int _BlueNoiseTex = Shader.PropertyToID("_BlueNoiseTex");

	private static readonly int _VolumeMaskAtlas = Shader.PropertyToID("_VolumeMaskAtlas");

	private static readonly string TEMPORAL_ACCUMULATION = "TEMPORAL_ACCUMULATION";

	private static readonly string _LOCAL_VOLUMETRIC_VOLUMES_ENABLED = "_LOCAL_VOLUMETRIC_VOLUMES_ENABLED";

	private static readonly int3 m_VoxelizationGroupSize = new int3(8, 8, 8);

	private static readonly int3 m_LightingGroupSize = new int3(8, 8, 8);

	private static readonly int3 m_ScatterGroupSize = new int3(8, 8, 1);

	private Material m_ShadowmapDownsampleMaterial;

	private ComputeShader m_VoxelizationShader;

	private ComputeShader m_LightingShader;

	private ComputeShader m_ScatterShader;

	private VolumetricLightingFeature m_Feature;

	public override string Name => "VolumetricLightingPass";

	public TileSize TileSize { get; internal set; }

	public VolumetricFog VolumetricFog { get; internal set; }

	public VolumetricLightingPass(RenderPassEvent evt, VolumetricLightingFeature feature)
		: base(evt)
	{
		m_ShadowmapDownsampleMaterial = feature.ShadowmapDownsampleMaterial;
		m_VoxelizationShader = feature.Shaders.VoxelizationShader;
		m_LightingShader = feature.Shaders.LightingShader;
		m_ScatterShader = feature.Shaders.ScatterShader;
		m_Feature = feature;
	}

	protected override void Setup(RenderGraphBuilder builder, VolumetricLightingPassData data, ref RenderingData renderingData)
	{
		data.ShadowmapDownsampleMaterial = m_ShadowmapDownsampleMaterial;
		data.VoxelizationShader = m_VoxelizationShader;
		data.LightingShader = m_LightingShader;
		data.ScatterShader = m_ScatterShader;
		ref CameraData cameraData = ref renderingData.CameraData;
		Vector2Int cameraRenderPixelSize = new Vector2Int(cameraData.CameraTargetDescriptor.width, cameraData.CameraTargetDescriptor.height);
		int3 textureSize = new int3(cameraData.CameraTargetDescriptor.width, cameraData.CameraTargetDescriptor.height, (int)m_Feature.Settings.Slices);
		int tileSize = (int)TileSize;
		textureSize.x = RenderingUtils.DivRoundUp(textureSize.x, tileSize);
		textureSize.y = RenderingUtils.DivRoundUp(textureSize.y, tileSize);
		VolumetricCameraBuffer volumetricCameraBuffer = VolumetricCameraBuffers.EnsureCamera(cameraData.Camera, cameraRenderPixelSize, textureSize, m_Feature.Settings.TemporalAccumulation ? 1 : 0);
		data.SkipFirstFrameTemporalAccumulation = !volumetricCameraBuffer.IsFirstFrame;
		TextureDesc desc = new TextureDesc(textureSize.x, textureSize.y);
		desc.slices = textureSize.z;
		desc.depthBufferBits = DepthBits.None;
		desc.dimension = TextureDimension.Tex3D;
		desc.enableRandomWrite = true;
		desc.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
		desc.name = "VoxelizedScene";
		data.VoxelizedSceneTexture = builder.CreateTransientTexture(in desc);
		desc.name = "VolumetricScatter";
		data.Resources.VolumetricScatter = data.Resources.RenderGraph.CreateTexture(in desc);
		data.ScatterTexture = builder.WriteTexture(in data.Resources.VolumetricScatter);
		data.TemporalAccumulation = m_Feature.Settings.TemporalAccumulation;
		data.TemporalFeedback = m_Feature.Settings.TemporalFeedback;
		if (m_Feature.Settings.TemporalAccumulation)
		{
			data.LightingHistoryTexture = data.Resources.RenderGraph.ImportTexture(volumetricCameraBuffer.GetCurrentFrameRT());
			data.BlueNoiseTexture = m_Feature.Textures.BlueNoise16LTex[renderingData.TimeData.FrameId % m_Feature.Textures.BlueNoise16LTex.Length];
		}
		else
		{
			data.BlueNoiseTexture = m_Feature.Textures.BlueNoise16LTex[0];
		}
		data.CameraDepthCopy = builder.ReadTexture(in data.Resources.CameraDepthCopyRT);
		bool num = WaaaghPipeline.Asset.ShadowSettings.ShadowQuality == ShadowQuality.Disable;
		bool flag = Mathf.Approximately(renderingData.CameraData.MaxShadowDistance, 0f);
		int num2;
		TextureHandle input;
		if (!num)
		{
			num2 = ((!flag) ? 1 : 0);
			if (num2 != 0)
			{
				input = data.Resources.NativeShadowmap;
				data.Shadowmap = builder.ReadTexture(in input);
				goto IL_02b9;
			}
		}
		else
		{
			num2 = 0;
		}
		input = renderingData.RenderGraph.defaultResources.defaultShadowTexture;
		data.Shadowmap = builder.ReadTexture(in input);
		goto IL_02b9;
		IL_02b9:
		if (num2 != 0 && m_Feature.Settings.UseDownsampledShadowmap && m_Feature.Settings.DownsampledShadowmapSize < WaaaghPipeline.Asset.ShadowSettings.AtlasSize)
		{
			data.UseDownsampledShadowmap = true;
			int downsampledShadowmapSize = (int)m_Feature.Settings.DownsampledShadowmapSize;
			TextureDesc desc2 = new TextureDesc(downsampledShadowmapSize, downsampledShadowmapSize);
			desc2.colorFormat = GraphicsFormat.D16_UNorm;
			desc2.isShadowMap = true;
			desc2.depthBufferBits = DepthBits.Depth16;
			desc2.filterMode = FilterMode.Bilinear;
			desc2.wrapMode = TextureWrapMode.Clamp;
			desc2.dimension = TextureDimension.Tex2D;
			desc2.useMipMap = false;
			desc2.name = "ShadowmapDownsampledRT";
			data.ShadowmapDownsampled = builder.CreateTransientTexture(in desc2);
			data.DownsampledShadowmapSize = new Vector4(downsampledShadowmapSize, downsampledShadowmapSize, 1f / (float)downsampledShadowmapSize, 1f / (float)downsampledShadowmapSize);
		}
		else
		{
			data.UseDownsampledShadowmap = false;
		}
		data.TilesMinMaxZ = builder.ReadTexture(in data.Resources.TilesMinMaxZTexture);
		Camera camera = cameraData.Camera;
		float nearClipPlane = camera.nearClipPlane;
		float num3 = math.min(m_Feature.Settings.FarClip, camera.farClipPlane);
		float z = nearClipPlane / num3;
		float w = num3 / nearClipPlane;
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 proj = Matrix4x4.Perspective(camera.fieldOfView, camera.aspect, nearClipPlane, num3);
		bool flag2 = true;
		bool renderIntoTexture = SystemInfo.graphicsUVStartsAtTop && flag2;
		proj = GL.GetGPUProjectionMatrix(proj, renderIntoTexture);
		Matrix4x4 viewProjMatrix = CoreMatrixUtils.MultiplyProjectionMatrix(proj, worldToCameraMatrix, camera.orthographic);
		Matrix4x4 matrix4x = Matrix4x4.Inverse(worldToCameraMatrix);
		Matrix4x4 matrix4x2 = Matrix4x4.Inverse(proj);
		Matrix4x4 invViewProjMatrix = matrix4x * matrix4x2;
		data.ViewProjMatrix = viewProjMatrix;
		data.InvViewProjMatrix = invViewProjMatrix;
		data.PrevViewProjMatrix = volumetricCameraBuffer.PrevViewProjMatrix;
		data.VolumetricProjectionParams = new Vector4(nearClipPlane, num3, z, w);
		float num4 = ScaleHeightFromLayerDepth(Mathf.Max(0.01f, VolumetricFog.FogHeight.value));
		data.HeightFogParams = new Vector4(VolumetricFog.BaseHeight.value, 1f / num4, num4, VolumetricFog.HeightFogEnabled.value ? 1 : 0);
		data.LightingTextureSize = new Vector4(textureSize.x, textureSize.y, textureSize.z);
		data.VoxelizationDispatchSize = new int3(RenderingUtils.DivRoundUp(textureSize.x, m_VoxelizationGroupSize.x), RenderingUtils.DivRoundUp(textureSize.y, m_VoxelizationGroupSize.y), RenderingUtils.DivRoundUp(textureSize.z, m_VoxelizationGroupSize.z));
		data.LightingDispatchSize = new int3(RenderingUtils.DivRoundUp(textureSize.x, m_LightingGroupSize.x), RenderingUtils.DivRoundUp(textureSize.y, m_LightingGroupSize.y), RenderingUtils.DivRoundUp(textureSize.z, m_LightingGroupSize.z));
		data.ScatterDispatchSize = new int3(RenderingUtils.DivRoundUp(textureSize.x, m_ScatterGroupSize.x), RenderingUtils.DivRoundUp(textureSize.y, m_ScatterGroupSize.y), 1);
		data.BlueNoiseTextureSize = data.BlueNoiseTexture.width;
		float num5 = 1f / VolumetricFog.FogDistanceAttenuation.value;
		Vector4 scatteringExtinction = (Vector4)VolumetricFog.Albedo.value * num5;
		data.ScatteringExtinction = scatteringExtinction;
		data.ScatteringExtinction.w = num5;
		data.Anisotropy = VolumetricFog.Anisotropy.value;
		data.TricubicDeferred = m_Feature.Settings.TricubicFilteringDeferred;
		data.TricubicForward = m_Feature.Settings.TricubicFilteringForward;
		data.LightShadows = (float)m_Feature.Settings.LightShadows;
		data.AmbientLightScale = VolumetricFog.AmbientLightMultiplier.value;
		data.HighRes = m_Feature.Settings.Slices == VolumetricLightingSlices.x128;
		data.LocalVolumesEnabled = m_Feature.Settings.LocalVolumesEnabled;
		if (data.LocalVolumesEnabled)
		{
			data.LocalVolumetricFogClusteringParams = m_Feature.FogClusteringParams;
			data.LocalFogBoundsBuffer = builder.ReadComputeBuffer(in m_Feature.VisibleVolumesBoundsBufferHandle);
			data.LocalFogTilesBuffer = builder.ReadComputeBuffer(in m_Feature.FogTilesBufferHandle);
			data.LocalFogGpuDataBuffer = builder.ReadComputeBuffer(in m_Feature.VisibleVolumesDataBufferHandle);
			data.LocalFogZBinsBuffer = builder.ReadComputeBuffer(in m_Feature.ZBinsBufferHandle);
			data.ScreenProjMatrix = GetScreenProjMatrix(ref cameraData);
			data.VolumeMaskAtlas = LocalVolumetricFogManager.Instance.VolumeAtlas.GetAtlas();
			if (data.VolumeMaskAtlas == null)
			{
				data.VolumeMaskAtlas = CoreUtils.blackVolumeTexture;
			}
		}
		volumetricCameraBuffer.Swap(data.ViewProjMatrix);
	}

	private Matrix4x4 GetScreenProjMatrix(ref CameraData cameraData)
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		float num = cameraData.CameraTargetDescriptor.width;
		float num2 = cameraData.CameraTargetDescriptor.height;
		matrix4x.SetRow(0, new Vector4(0.5f * num, 0f, 0f, 0.5f * num));
		matrix4x.SetRow(1, new Vector4(0f, 0.5f * num2, 0f, 0.5f * num2));
		matrix4x.SetRow(2, new Vector4(0f, 0f, 0.5f, 0.5f));
		matrix4x.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		return matrix4x * cameraData.GetProjectionMatrix();
	}

	protected override void Render(VolumetricLightingPassData data, RenderGraphContext context)
	{
		if (data.UseDownsampledShadowmap)
		{
			context.cmd.SetRenderTarget(data.ShadowmapDownsampled);
			context.cmd.SetGlobalTexture(ShaderPropertyId._ShadowmapRT, data.Shadowmap);
			context.cmd.SetGlobalVector(_OutputSize, data.DownsampledShadowmapSize);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.ShadowmapDownsampleMaterial, 0, MeshTopology.Triangles, 3);
			context.cmd.SetGlobalTexture(ShaderPropertyId._ShadowmapRT, data.ShadowmapDownsampled);
		}
		else
		{
			context.cmd.SetGlobalTexture(ShaderPropertyId._ShadowmapRT, data.Shadowmap);
		}
		context.cmd.SetComputeTextureParam(data.VoxelizationShader, 0, _ResultUAV, data.VoxelizedSceneTexture);
		context.cmd.SetComputeTextureParam(data.VoxelizationShader, 0, ShaderPropertyId._TilesMinMaxZTexture, data.TilesMinMaxZ);
		context.cmd.SetComputeVectorParam(data.VoxelizationShader, _VolumeScatter_Size, data.LightingTextureSize);
		context.cmd.SetComputeVectorParam(data.VoxelizationShader, _VolumetricScatteringExtinction, data.ScatteringExtinction);
		context.cmd.SetComputeMatrixParam(data.VoxelizationShader, _VolumetricInvViewProj, data.InvViewProjMatrix);
		context.cmd.SetComputeVectorParam(data.VoxelizationShader, _VolumetricProjectionParams, data.VolumetricProjectionParams);
		context.cmd.SetComputeVectorParam(data.VoxelizationShader, _VolumetricHeightFogParams, data.HeightFogParams);
		if (data.LocalVolumesEnabled)
		{
			context.cmd.SetComputeVectorParam(data.VoxelizationShader, ShaderPropertyId._LocalVolumetricFogClusteringParams, data.LocalVolumetricFogClusteringParams);
			context.cmd.SetComputeBufferParam(data.VoxelizationShader, 0, ShaderPropertyId._VisibleVolumeBoundsBuffer, data.LocalFogBoundsBuffer);
			context.cmd.SetComputeBufferParam(data.VoxelizationShader, 0, ShaderPropertyId._VisibleVolumeDataBuffer, data.LocalFogGpuDataBuffer);
			context.cmd.SetComputeBufferParam(data.VoxelizationShader, 0, ShaderPropertyId._FogTilesBuffer, data.LocalFogTilesBuffer);
			context.cmd.SetComputeBufferParam(data.VoxelizationShader, 0, ShaderPropertyId._LocalFogZBinsBuffer, data.LocalFogZBinsBuffer);
			context.cmd.SetComputeMatrixParam(data.VoxelizationShader, ShaderPropertyId._ScreenProjMatrix, data.ScreenProjMatrix);
			context.cmd.SetComputeTextureParam(data.VoxelizationShader, 0, _VolumeMaskAtlas, data.VolumeMaskAtlas);
			context.cmd.EnableShaderKeyword(_LOCAL_VOLUMETRIC_VOLUMES_ENABLED);
		}
		else
		{
			context.cmd.DisableShaderKeyword(_LOCAL_VOLUMETRIC_VOLUMES_ENABLED);
		}
		context.cmd.DispatchCompute(data.VoxelizationShader, 0, data.VoxelizationDispatchSize.x, data.VoxelizationDispatchSize.y, data.VoxelizationDispatchSize.z);
		if (data.TemporalAccumulation && data.SkipFirstFrameTemporalAccumulation)
		{
			context.cmd.EnableShaderKeyword(TEMPORAL_ACCUMULATION);
		}
		else
		{
			context.cmd.DisableShaderKeyword(TEMPORAL_ACCUMULATION);
		}
		context.cmd.SetComputeTextureParam(data.LightingShader, 0, _ResultUAV, data.ScatterTexture);
		context.cmd.SetComputeTextureParam(data.LightingShader, 0, _VoxelizedSceneTexture, data.VoxelizedSceneTexture);
		if (data.TemporalAccumulation)
		{
			context.cmd.SetComputeTextureParam(data.LightingShader, 0, _HistoryTexture, data.LightingHistoryTexture);
		}
		context.cmd.SetComputeTextureParam(data.LightingShader, 0, _BlueNoiseTex, data.BlueNoiseTexture);
		context.cmd.SetComputeVectorParam(data.LightingShader, _VolumeScatter_Size, data.LightingTextureSize);
		context.cmd.SetComputeMatrixParam(data.LightingShader, _VolumetricViewProj, data.ViewProjMatrix);
		context.cmd.SetComputeMatrixParam(data.LightingShader, _VolumetricInvViewProj, data.InvViewProjMatrix);
		context.cmd.SetComputeMatrixParam(data.LightingShader, _VolumetricPrevViewProj, data.PrevViewProjMatrix);
		context.cmd.SetComputeVectorParam(data.LightingShader, _VolumetricProjectionParams, data.VolumetricProjectionParams);
		context.cmd.SetComputeFloatParam(data.LightingShader, _BlueNoiseTex_Size, data.BlueNoiseTextureSize);
		context.cmd.SetComputeFloatParam(data.LightingShader, _Anisotropy, data.Anisotropy);
		context.cmd.SetComputeFloatParam(data.LightingShader, _VolumetricLightShadows, data.LightShadows);
		context.cmd.SetComputeFloatParam(data.LightingShader, _TemporalFeedback, data.TemporalFeedback);
		context.cmd.SetComputeFloatParam(data.LightingShader, _VolumetricAmbientLightScale, data.AmbientLightScale);
		context.cmd.SetComputeVectorParam(data.LightingShader, _VolumetricScatteringExtinction, data.ScatteringExtinction);
		RenderingUtils.SetLightProbe(context.cmd, RenderSettings.ambientProbe);
		context.cmd.DispatchCompute(data.LightingShader, 0, data.LightingDispatchSize.x, data.LightingDispatchSize.y, data.LightingDispatchSize.z);
		TextureHandle textureHandle = ((!data.TemporalAccumulation) ? data.VoxelizedSceneTexture : data.LightingHistoryTexture);
		context.cmd.SetComputeTextureParam(data.ScatterShader, 1, _VolumeInject, data.ScatterTexture);
		context.cmd.SetComputeTextureParam(data.ScatterShader, 1, _ResultUAV, textureHandle);
		context.cmd.SetComputeVectorParam(data.ScatterShader, _VolumeScatter_Size, data.LightingTextureSize);
		context.cmd.DispatchCompute(data.ScatterShader, 1, data.LightingDispatchSize.x, data.LightingDispatchSize.y, data.LightingDispatchSize.z);
		context.cmd.SetComputeTextureParam(data.ScatterShader, 0, _VolumeInject, textureHandle);
		context.cmd.SetComputeTextureParam(data.ScatterShader, 0, _ResultUAV, data.ScatterTexture);
		context.cmd.SetComputeVectorParam(data.ScatterShader, _VolumetricProjectionParams, data.VolumetricProjectionParams);
		context.cmd.SetComputeVectorParam(data.ScatterShader, _VolumeScatter_Size, data.LightingTextureSize);
		context.cmd.DispatchCompute(data.ScatterShader, 0, data.ScatterDispatchSize.x, data.ScatterDispatchSize.y, data.ScatterDispatchSize.z);
		context.cmd.SetGlobalTexture(ShaderPropertyId._VolumeScatter, data.ScatterTexture);
		context.cmd.SetGlobalFloat(ShaderPropertyId._VolumetricLightingEnabled, 1f);
		context.cmd.SetGlobalVector(_VolumeScatter_Size, data.LightingTextureSize);
		context.cmd.SetGlobalFloat(_VolumtricTricubicDeferred, data.TricubicDeferred ? 1 : 0);
		context.cmd.SetGlobalFloat(_VolumtricTricubicForward, data.TricubicForward ? 1 : 0);
		context.cmd.SetGlobalVector(_VolumetricProjectionParams, data.VolumetricProjectionParams);
		context.cmd.SetGlobalMatrix(_VolumetricViewProj, data.ViewProjMatrix);
		if (data.UseDownsampledShadowmap)
		{
			context.cmd.SetGlobalTexture(ShaderPropertyId._ShadowmapRT, data.Shadowmap);
		}
	}

	private static float ScaleHeightFromLayerDepth(float d)
	{
		return d * 0.144765f;
	}
}
