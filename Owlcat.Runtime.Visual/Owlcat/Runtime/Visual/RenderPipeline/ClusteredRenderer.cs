using JetBrains.Annotations;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using Owlcat.Runtime.Visual.RenderPipeline.Lighting;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Owlcat.Runtime.Visual.RenderPipeline.Passes.CameraIndependent;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public class ClusteredRenderer : ScriptableRenderer
{
	private const string kDebugDisplaySettingsTag = "Apply Display Settings";

	private const string kSetupGlobalShaderKeywordsTag = "Setup Global Shader Keywords";

	private ProfilingSampler m_DebugDisplayProfilingSampler = new ProfilingSampler("Apply Display Settings");

	private ProfilingSampler m_SetupGlobalShaderKeywordsProfilingSampler = new ProfilingSampler("Setup Global Shader Keywords");

	private IndirectRenderingSystemSubmitPass m_IndirectRenderingSystemSubmitPass;

	private GPUSkinningSytemSubmitPass m_GPUSkinningSystemSubmitPass;

	private IndirectiRenderingSystemCullingPass m_IndirectRenderingSystemCullingPass;

	private ClearPass m_ClearPass;

	private LightCullingPass m_LightCullingPass;

	private ShadowCasterPass m_ShadowCasterPass;

	private GBufferPass m_GBufferPass;

	private CopyDepthPass m_CopyDepthPass;

	private DrawObjectsPass m_RenderOpaqueForwardPass;

	private DeferredLightingPass m_DeferredLightingPass;

	private FogPass m_FogPass;

	private DrawSkyboxPass m_DrawSkyboxPass;

	private DrawDecalsPass m_DrawDecalsPass;

	private DrawColorPyramidPass m_ColorPyramidAfterOpaquePass;

	private GBufferPass m_GBufferDistortionPass;

	private CopyDepthPass m_CopyDepthRefractionPass;

	private DrawObjectsPass m_RenderOpaqueDistortionPass;

	private DrawObjectsPass m_RenderTransparentForwardPass;

	private DrawColorPyramidPass m_ColorPyramidAfterTransparents;

	private DistortionPass m_DistortionPassTransparent;

	private HbaoPass m_HbaoPass;

	private ScreenSpaceCloudShadowsPass m_ScreenSpaceCloudShadows;

	private DrawColorPyramidPass m_ColorPyramidAfterOpaqueDistortion;

	private DepthPyramidPass m_DepthPyramidPass;

	private ScreenSpaceReflectionsPass m_ScreenSpaceReflections;

	private DrawDecalsPass m_DrawGUIDecalsPass;

	private FinalBlitPass m_FinalBlitPass;

	private CopyDepthPass m_FinalCopyDepth;

	private VFXPass m_VFXPass;

	[CanBeNull]
	private ColorGradingLutPass m_ColorGradingLutPass;

	[CanBeNull]
	private BeforeTransparentPostProcessPass m_BeforeTransparentPostProcessPass;

	[CanBeNull]
	private PostProcessPass m_PostProcessPass;

	[CanBeNull]
	private PostProcessPass m_FinalPostProcessPass;

	[CanBeNull]
	private DebugPass m_DebugPass;

	private GBuffer m_GBuffer;

	private RenderTargetHandle m_ColorGradingLut;

	private RenderTargetHandle m_AfterPostProcessColor;

	private ClusteredLights m_ClusteredLights;

	private StencilState m_DefaultStencilState;

	private ClusteredRendererData m_Data;

	private bool m_IsFirstCameraInChain;

	private bool m_IsLastCameraInChain;

	private ComputeBuffer m_DummyComputeBuffer;

	private RenderTexture m_Shadowmap;

	public ClusteredRenderer(ClusteredRendererData data)
		: base(data)
	{
		m_Data = data;
		Material blitMaterial = CoreUtils.CreateEngineMaterial(data.Shaders.BlitShader);
		Material blitMaterial2 = CoreUtils.CreateEngineMaterial(data.Shaders.FinalBlitShader);
		CoreUtils.CreateEngineMaterial(data.Shaders.CopyDepthShader);
		Material material = CoreUtils.CreateEngineMaterial(data.Shaders.CopyDepthFastShader);
		Material colorPyramidMaterial = CoreUtils.CreateEngineMaterial(data.Shaders.ColorPyramidShader);
		Material deferredLightingMaterial = CoreUtils.CreateEngineMaterial(data.Shaders.DeferredLightingShader);
		Material deferredReflectionsMaterial = CoreUtils.CreateEngineMaterial(data.Shaders.DeferredReflectionsShader);
		Material applyDistortionMaterial = CoreUtils.CreateEngineMaterial(data.Shaders.ApplyDistortionShader);
		Material material2 = CoreUtils.CreateEngineMaterial(data.Shaders.FogShader);
		Material material3 = CoreUtils.CreateEngineMaterial(data.Shaders.HbaoShader);
		Material material4 = CoreUtils.CreateEngineMaterial(data.Shaders.ScreenSpaceCloudShadowsShader);
		Material ssrMaterial = CoreUtils.CreateEngineMaterial(data.Shaders.ScreenSpaceReflectionsShaderPS);
		Material dBufferBlitMaterial = CoreUtils.CreateEngineMaterial(data.Shaders.DBufferBlitShader);
		StencilStateData defaultStencilState = data.DefaultStencilState;
		m_DefaultStencilState = StencilState.defaultValue;
		m_DefaultStencilState.enabled = defaultStencilState.OverrideStencilState;
		m_DefaultStencilState.SetCompareFunction(defaultStencilState.StencilCompareFunction);
		m_DefaultStencilState.SetPassOperation(defaultStencilState.PassOperation);
		m_DefaultStencilState.SetFailOperation(defaultStencilState.FailOperation);
		m_DefaultStencilState.SetZFailOperation(defaultStencilState.ZFailOperation);
		m_IndirectRenderingSystemSubmitPass = new IndirectRenderingSystemSubmitPass(RenderPassEvent.BeforeRendering);
		m_GPUSkinningSystemSubmitPass = new GPUSkinningSytemSubmitPass(RenderPassEvent.BeforeRendering);
		m_IndirectRenderingSystemCullingPass = new IndirectiRenderingSystemCullingPass(RenderPassEvent.BeforeRendering);
		m_ShadowCasterPass = new ShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
		m_ClearPass = new ClearPass(RenderPassEvent.BeforeRenderingPrepasses);
		m_GBufferPass = new GBufferPass(RenderPassEvent.BeforeRenderingPrepasses, OwlcatRenderQueue.OpaquePreDistortion, data.OpaqueLayerMask);
		m_CopyDepthPass = new CopyDepthPass(RenderPassEvent.BeforeRenderingPrepasses, material);
		m_LightCullingPass = new LightCullingPass((RenderPassEvent)151, data.Shaders.LightCullingShader);
		m_RenderOpaqueForwardPass = new DrawObjectsPass("Render Opaques", opaque: true, clear: true, RenderPassEvent.BeforeRenderingOpaques, OwlcatRenderQueue.OpaquePreDistortion, data.OpaqueLayerMask, m_DefaultStencilState, defaultStencilState.StencilReference);
		m_DeferredLightingPass = new DeferredLightingPass(RenderPassEvent.BeforeRenderingOpaques, deferredLightingMaterial, deferredReflectionsMaterial, data.Shaders.DeferredLightingComputeShader, data.UseComputeInDeferredPath);
		m_DrawDecalsPass = new DrawDecalsPass(RenderPassEvent.BeforeRenderingOpaques, dBufferBlitMaterial);
		m_HbaoPass = new HbaoPass(RenderPassEvent.BeforeRenderingOpaques, material3);
		m_ScreenSpaceCloudShadows = new ScreenSpaceCloudShadowsPass(RenderPassEvent.BeforeRenderingOpaques, material4);
		m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);
		m_ColorPyramidAfterOpaquePass = new DrawColorPyramidPass(RenderPassEvent.AfterRenderingSkybox, colorPyramidMaterial, blitMaterial);
		m_RenderOpaqueDistortionPass = new DrawObjectsPass("Render Opaques Distortion", opaque: true, clear: false, RenderPassEvent.AfterRenderingSkybox, OwlcatRenderQueue.OpaqueDistortion, data.OpaqueLayerMask, m_DefaultStencilState, defaultStencilState.StencilReference);
		m_ColorPyramidAfterOpaqueDistortion = new DrawColorPyramidPass(RenderPassEvent.AfterRenderingSkybox, colorPyramidMaterial, blitMaterial);
		m_DepthPyramidPass = new DepthPyramidPass(RenderPassEvent.AfterRenderingSkybox, data.Shaders.DepthPyramidShader);
		m_ScreenSpaceReflections = new ScreenSpaceReflectionsPass(RenderPassEvent.AfterRenderingSkybox, ssrMaterial, deferredReflectionsMaterial, data.Shaders.ScreenSpaceReflectionsShaderCS);
		m_FogPass = new FogPass(RenderPassEvent.AfterRenderingSkybox, material2);
		m_GBufferDistortionPass = new GBufferPass(RenderPassEvent.AfterRenderingSkybox, OwlcatRenderQueue.OpaqueDistortion, data.OpaqueLayerMask);
		m_CopyDepthRefractionPass = new CopyDepthPass(RenderPassEvent.AfterRenderingSkybox, material);
		m_RenderTransparentForwardPass = new DrawObjectsPass("Render Transparent", opaque: false, clear: false, RenderPassEvent.BeforeRenderingTransparents, OwlcatRenderQueue.Transparent, data.TransparentLayerMask, m_DefaultStencilState, defaultStencilState.StencilReference);
		m_ColorPyramidAfterTransparents = new DrawColorPyramidPass(RenderPassEvent.BeforeRenderingTransparents, colorPyramidMaterial, blitMaterial);
		m_DistortionPassTransparent = new DistortionPass(RenderPassEvent.BeforeRenderingTransparents, applyDistortionMaterial);
		m_DrawGUIDecalsPass = new DrawDecalsPass(RenderPassEvent.AfterRendering, null, drawGUIDecals: true);
		m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering, blitMaterial2);
		m_FinalCopyDepth = new CopyDepthPass(RenderPassEvent.AfterRendering, material);
		m_VFXPass = new VFXPass(RenderPassEvent.AfterRendering, material);
		if (data.DebugData != null)
		{
			m_DebugPass = new DebugPass(RenderPassEvent.AfterRendering, data.DebugData);
		}
		if (data.PostProcessData != null)
		{
			m_ColorGradingLutPass = new ColorGradingLutPass(RenderPassEvent.BeforeRenderingOpaques, data.PostProcessData);
			m_BeforeTransparentPostProcessPass = new BeforeTransparentPostProcessPass(RenderPassEvent.BeforeRenderingOpaques, data.PostProcessData, blitMaterial);
			m_PostProcessPass = new PostProcessPass(RenderPassEvent.BeforeRenderingPostProcessing, data.PostProcessData);
			m_FinalPostProcessPass = new PostProcessPass(RenderPassEvent.AfterRenderingPostProcessing, data.PostProcessData);
		}
		m_GBuffer = new GBuffer();
		m_ColorGradingLut.Init("_InternalGradingLut");
		m_AfterPostProcessColor.Init("_AfterPostProcessColorRT");
		m_ClusteredLights = new ClusteredLights();
		IndirectRenderingSystem.Instance.Initialize(m_Data.Shaders.IndirectRenderingCullShader);
		m_DummyComputeBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Structured);
	}

	public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ValidateShadowmap(ref renderingData);
		m_IsFirstCameraInChain = renderingData.CameraData.IsFirstInChain;
		m_IsLastCameraInChain = renderingData.CameraData.IsLastInChain;
		RenderTextureDescriptor baseDescriptor = renderingData.CameraData.CameraTargetDescriptor;
		bool flag = m_Data.PostProcessData != null && renderingData.CameraData.IsPostProcessEnabled;
		bool isLightingEnabled = renderingData.CameraData.IsLightingEnabled;
		bool isDistortionEnabled = renderingData.CameraData.IsDistortionEnabled;
		bool isDecalsEnabled = renderingData.CameraData.IsDecalsEnabled;
		bool isFogEnabled = renderingData.CameraData.IsFogEnabled;
		bool isIndirectRenderingEnabled = renderingData.CameraData.IsIndirectRenderingEnabled;
		RenderPath renderPath = m_Data.RenderPath;
		bool flag2 = renderingData.CameraData.DepthTexture != BuiltinRenderTextureType.None;
		bool isScreenSpaceReflectionsEnabled = renderingData.CameraData.IsScreenSpaceReflectionsEnabled;
		bool flag3 = m_Data.DebugData != null && m_Data.DebugData.IsAnyDebugEnabled() && (renderingData.CameraData.Camera.cameraType == CameraType.Game || renderingData.CameraData.IsSceneViewCamera);
		if (isLightingEnabled)
		{
			m_ClusteredLights.StartSetupJobs(ref renderingData, m_Data.TileSize);
		}
		m_GBuffer.Initialize(m_Data.RenderPath, context, ref renderingData, m_Data.UseComputeInDeferredPath);
		SetupGlobalShaderKeywords(context, ref renderingData);
		ApplyDebugSettings(context, ref renderingData);
		EnqueuePass(m_GPUSkinningSystemSubmitPass);
		EnqueuePass(m_IndirectRenderingSystemSubmitPass);
		if (isIndirectRenderingEnabled)
		{
			EnqueuePass(m_IndirectRenderingSystemCullingPass);
		}
		for (int i = 0; i < base.RendererFeatures.Count; i++)
		{
			if (renderingData.CameraData.DisabledRendererFeatures.Contains(base.RendererFeatures[i].GetFeatureIdentifier()))
			{
				base.RendererFeatures[i].DisableFeature();
			}
			else
			{
				base.RendererFeatures[i].AddRenderPasses(this, ref renderingData);
			}
		}
		for (int num = base.ActiveRenderPassQueue.Count - 1; num >= 0; num--)
		{
			if (base.ActiveRenderPassQueue[num] == null)
			{
				base.ActiveRenderPassQueue.RemoveAt(num);
			}
		}
		base.ActiveRenderPassQueue.Find((ScriptableRenderPass x) => x.RenderPassEvent == RenderPassEvent.AfterRendering);
		if (renderingData.ShadowData.ShadowQuality != ShadowQuality.Disable && isLightingEnabled)
		{
			m_ShadowCasterPass.Setup(m_ClusteredLights.ClusteredShadows, m_Shadowmap);
			EnqueuePass(m_ShadowCasterPass);
		}
		m_ClearPass.Setup(m_GBuffer);
		EnqueuePass(m_ClearPass);
		m_GBufferPass.Setup(m_GBuffer);
		EnqueuePass(m_GBufferPass);
		if (isDistortionEnabled || isDecalsEnabled || flag || renderPath == RenderPath.Deferred)
		{
			m_CopyDepthPass.Setup(m_GBuffer.CameraDepthRt, m_GBuffer.CameraDepthCopyRt, baseDescriptor, m_GBuffer.DepthPyramidSamplingRatio);
			EnqueuePass(m_CopyDepthPass);
		}
		if (isLightingEnabled)
		{
			m_LightCullingPass.Setup(m_ClusteredLights);
			EnqueuePass(m_LightCullingPass);
		}
		if (flag)
		{
			m_ColorGradingLutPass?.Setup(in m_ColorGradingLut);
			EnqueuePass(m_ColorGradingLutPass);
		}
		if (isLightingEnabled && renderPath == RenderPath.Deferred)
		{
			m_DeferredLightingPass.Setup(baseDescriptor, m_GBuffer, m_ClusteredLights);
			EnqueuePass(m_DeferredLightingPass);
		}
		else
		{
			m_RenderOpaqueForwardPass.Setup(m_GBuffer);
			EnqueuePass(m_RenderOpaqueForwardPass);
		}
		if (isScreenSpaceReflectionsEnabled)
		{
			if (isDecalsEnabled)
			{
				m_DrawDecalsPass.Setup(m_GBuffer.CameraColorRt, m_GBuffer);
				EnqueuePass(m_DrawDecalsPass);
			}
			if (flag)
			{
				m_HbaoPass.Setup(m_GBuffer);
				EnqueuePass(m_HbaoPass);
			}
		}
		else
		{
			if (flag)
			{
				m_HbaoPass.Setup(m_GBuffer);
				EnqueuePass(m_HbaoPass);
			}
			if (isDecalsEnabled)
			{
				m_DrawDecalsPass.Setup(m_GBuffer.CameraColorRt, m_GBuffer);
				EnqueuePass(m_DrawDecalsPass);
			}
		}
		if (flag)
		{
			m_BeforeTransparentPostProcessPass?.Setup(baseDescriptor, m_GBuffer.CameraColorRt, m_GBuffer.CameraColorRt, m_GBuffer.CameraDepthRt);
			EnqueuePass(m_BeforeTransparentPostProcessPass);
			m_ScreenSpaceCloudShadows.Setup(m_GBuffer.CameraColorRt);
			EnqueuePass(m_ScreenSpaceCloudShadows);
		}
		if (renderingData.CameraData.Camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
		{
			m_DrawSkyboxPass.Setup(m_GBuffer.CameraColorRt, m_GBuffer.CameraDepthRt);
			EnqueuePass(m_DrawSkyboxPass);
		}
		if (isDistortionEnabled)
		{
			m_ColorPyramidAfterOpaquePass.Setup(baseDescriptor, m_GBuffer.ColorPyramidFormat, m_GBuffer.CameraColorRt, m_GBuffer.CameraColorPyramidRt);
			EnqueuePass(m_ColorPyramidAfterOpaquePass);
			m_GBufferDistortionPass.Setup(m_GBuffer);
			EnqueuePass(m_GBufferDistortionPass);
		}
		if (isDistortionEnabled)
		{
			m_RenderOpaqueDistortionPass.Setup(m_GBuffer);
			EnqueuePass(m_RenderOpaqueDistortionPass);
			m_CopyDepthRefractionPass.Setup(m_GBuffer.CameraDepthRt, m_GBuffer.CameraDepthCopyRt, baseDescriptor, m_GBuffer.DepthPyramidSamplingRatio);
			EnqueuePass(m_CopyDepthRefractionPass);
		}
		if (flag && renderPath == RenderPath.Deferred)
		{
			if (renderingData.CameraData.IsNeedDepthPyramid)
			{
				m_DepthPyramidPass.Setup(m_GBuffer);
				EnqueuePass(m_DepthPyramidPass);
			}
			if (renderingData.CameraData.IsScreenSpaceReflectionsEnabled)
			{
				m_ColorPyramidAfterOpaqueDistortion.Setup(baseDescriptor, m_GBuffer.ColorPyramidFormat, m_GBuffer.CameraColorRt, m_GBuffer.CameraColorPyramidRt);
				EnqueuePass(m_ColorPyramidAfterOpaqueDistortion);
				m_ScreenSpaceReflections.Setup(m_GBuffer, baseDescriptor, isDistortionEnabled);
				EnqueuePass(m_ScreenSpaceReflections);
			}
		}
		if (isFogEnabled)
		{
			m_FogPass.Setup(m_GBuffer.CameraColorRt);
			EnqueuePass(m_FogPass);
		}
		m_VFXPass.Setup(m_GBuffer.CameraDepthCopyRt);
		EnqueuePass(m_VFXPass);
		m_RenderTransparentForwardPass.Setup(m_GBuffer);
		EnqueuePass(m_RenderTransparentForwardPass);
		if (isDistortionEnabled)
		{
			m_ColorPyramidAfterTransparents.Setup(baseDescriptor, m_GBuffer.ColorPyramidFormat, m_GBuffer.CameraColorRt, m_GBuffer.CameraColorPyramidRt);
			EnqueuePass(m_ColorPyramidAfterTransparents);
			m_DistortionPassTransparent.Setup(m_GBuffer.CameraColorRt, m_GBuffer.CameraDepthRt);
			EnqueuePass(m_DistortionPassTransparent);
		}
		bool flag4 = flag && renderingData.CameraData.Antialiasing == AntialiasingMode.FastApproximateAntialiasing;
		RenderTargetHandle source = m_GBuffer.CameraColorRt;
		if (flag)
		{
			m_PostProcessPass?.Setup(in baseDescriptor, in m_GBuffer.CameraColorRt, in m_AfterPostProcessColor, in m_GBuffer.CameraDepthRt, in m_ColorGradingLut, flag4);
			source = m_AfterPostProcessColor;
			EnqueuePass(m_PostProcessPass);
			if (flag4)
			{
				m_FinalPostProcessPass?.SetupFinalPass(in m_AfterPostProcessColor, in m_GBuffer.CameraColorRt);
				source = m_GBuffer.CameraColorRt;
				EnqueuePass(m_FinalPostProcessPass);
			}
			else if (!m_IsLastCameraInChain)
			{
				m_FinalBlitPass.Setup(baseDescriptor, in m_AfterPostProcessColor, in m_GBuffer.CameraColorRt);
				source = m_GBuffer.CameraColorRt;
				EnqueuePass(m_FinalBlitPass);
			}
		}
		if (isDecalsEnabled)
		{
			m_DrawGUIDecalsPass.Setup(source, m_GBuffer);
			EnqueuePass(m_DrawGUIDecalsPass);
		}
		if (m_IsLastCameraInChain)
		{
			m_FinalBlitPass.Setup(baseDescriptor, in source);
			EnqueuePass(m_FinalBlitPass);
		}
		if (flag2)
		{
			m_FinalCopyDepth.Setup(m_GBuffer.CameraDepthRt, renderingData.CameraData.DepthTexture, baseDescriptor, m_GBuffer.DepthPyramidSamplingRatio);
			EnqueuePass(m_FinalCopyDepth);
		}
		if (flag3)
		{
			m_DebugPass?.Setup(baseDescriptor, m_GBuffer.CameraDepthRt);
			EnqueuePass(m_DebugPass);
		}
		if (isLightingEnabled)
		{
			m_ClusteredLights.CompleteSetupJobs();
		}
	}

	private void ValidateShadowmap(ref RenderingData renderingData)
	{
		bool flag = false;
		switch (renderingData.ShadowData.ShadowQuality)
		{
		case ShadowQuality.Disable:
			if (m_Shadowmap != null)
			{
				m_Shadowmap.Release();
				m_Shadowmap = null;
			}
			break;
		case ShadowQuality.HardOnly:
		case ShadowQuality.All:
			flag = m_Shadowmap == null || m_Shadowmap.width != renderingData.ShadowData.AtlasSize;
			break;
		}
		if (flag)
		{
			if (m_Shadowmap != null)
			{
				m_Shadowmap.Release();
			}
			m_Shadowmap = new RenderTexture(renderingData.ShadowData.AtlasSize, renderingData.ShadowData.AtlasSize, 16, RenderTextureFormat.Shadowmap);
			m_Shadowmap.name = "_ShadowmapRT";
			m_Shadowmap.filterMode = FilterMode.Bilinear;
			m_Shadowmap.wrapMode = TextureWrapMode.Clamp;
		}
	}

	private void SetupGlobalShaderKeywords(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_SetupGlobalShaderKeywordsProfilingSampler))
		{
			CoreUtils.SetKeyword(commandBuffer, ShaderKeywordStrings.SHADOWS_SHADOWMASK, m_ClusteredLights.ShadowmaskEnabled);
			CoreUtils.SetKeyword(commandBuffer, ShaderKeywordStrings.DEFERRED_ON, m_Data.RenderPath == RenderPath.Deferred);
			PBD.SetDummyComputeBuffer(commandBuffer, m_DummyComputeBuffer);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	private void ApplyDebugSettings(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		bool flag = m_Data.DebugData != null && m_Data.DebugData.IsDebugDisplayEnabled() && (renderingData.CameraData.Camera.cameraType == CameraType.Game || renderingData.CameraData.IsSceneViewCamera);
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_DebugDisplayProfilingSampler))
		{
			CoreUtils.SetKeyword(commandBuffer, ShaderKeywordStrings.DEBUG_DISPLAY, flag);
			if (flag)
			{
				if (m_Data.DebugData.RenderingDebug.DebugMipMap)
				{
					m_Data.DebugData.MipMapDebug.Prepare(commandBuffer);
				}
				commandBuffer.SetGlobalInt(DebugBuffer._DebugMipMap, m_Data.DebugData.RenderingDebug.DebugMipMap ? 1 : 0);
				commandBuffer.SetGlobalInt(DebugBuffer._DebugLightingMode, (int)m_Data.DebugData.LightingDebug.DebugLightingMode);
				commandBuffer.SetGlobalInt(DebugBuffer._DebugTerrain, (int)m_Data.DebugData.TerrainDebug.DebugTerrain);
				commandBuffer.SetGlobalInt(DebugBuffer._DebugMaterial, (int)m_Data.DebugData.RenderingDebug.DebugMaterial);
				commandBuffer.SetGlobalInt(DebugBuffer._DebugVertexAttribute, (int)m_Data.DebugData.RenderingDebug.DebugVertexAttribute);
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
		if (m_Data.DebugData != null && m_Data.DebugData.IndirectRenderingDebug.UseGameViewCameraForCulling)
		{
			Camera camera = renderingData.CameraData.Camera;
			if (camera.cameraType == CameraType.Game)
			{
				IndirectRenderingSystem.Instance.DebugCamera = camera;
			}
		}
		else
		{
			IndirectRenderingSystem.Instance.DebugCamera = null;
		}
	}

	public override void FinishRendering(CommandBuffer cmd)
	{
		if (m_IsLastCameraInChain)
		{
			m_GBuffer.Release(cmd);
		}
	}

	public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
	{
		bool num = OwlcatRenderPipeline.Asset.ShadowSettings.ShadowQuality == ShadowQuality.Disable;
		bool flag = Mathf.Approximately(cameraData.ShadowDistance, 0f);
		bool flag2 = !cameraData.IsLightingEnabled;
		if (num || flag || flag2)
		{
			cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
		}
		cullingParameters.shadowDistance = cameraData.ShadowDistance;
	}

	protected internal override void DisposeInternal()
	{
		IndirectRenderingSystem.Instance.Dispose();
		m_ClusteredLights.Dispose();
		VFXPass.ClearBuffers();
		m_DummyComputeBuffer.Release();
		if (m_Shadowmap != null)
		{
			m_Shadowmap.Release();
			m_Shadowmap = null;
		}
	}

	internal RenderTargetHandle GetCurrentCameraDepthTexture()
	{
		return m_GBuffer.CameraDepthRt;
	}

	internal RenderTargetHandle GetCurrentCameraColorTexture()
	{
		return m_GBuffer.CameraColorRt;
	}

	internal RenderTargetHandle GetCurrentCameraFinalColorTexture(ref RenderingData renderingData)
	{
		m_IsLastCameraInChain = renderingData.CameraData.IsLastInChain;
		bool isPostProcessEnabled = renderingData.CameraData.IsPostProcessEnabled;
		bool flag = isPostProcessEnabled && renderingData.CameraData.Antialiasing == AntialiasingMode.FastApproximateAntialiasing;
		RenderTargetHandle result = m_GBuffer.CameraColorRt;
		if (isPostProcessEnabled)
		{
			result = m_AfterPostProcessColor;
			if (flag)
			{
				result = m_GBuffer.CameraColorRt;
			}
		}
		return result;
	}
}
