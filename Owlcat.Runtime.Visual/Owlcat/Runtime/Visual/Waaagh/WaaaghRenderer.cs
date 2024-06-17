using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.Passes.Base;
using Owlcat.Runtime.Visual.Waaagh.Passes.IndirectRendering;
using Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public class WaaaghRenderer : ScriptableRenderer
{
	private enum NoiseBasedPostProcessingStage
	{
		None,
		PostProcessPass,
		FinalPostProcessPass,
		UpscaleBlitPass
	}

	private WaaaghRendererData m_Settings;

	private readonly Material m_CopyDepthMaterial;

	private readonly Material m_ErrorMaterial;

	private readonly Material m_DeferredReflectionsMaterial;

	private readonly Material m_DeferredLightingMaterial;

	private readonly Material m_BlitMaterial;

	private readonly Material m_ColorPyramidMaterial;

	private readonly Material m_ApplyDistortionMaterial;

	private readonly Material m_DebugFullscreenMaterial;

	private readonly Material m_DBufferBlitMaterial;

	private readonly Material m_FogMaterial;

	private readonly Material m_HbaoMaterial;

	private readonly Material m_SsrResolveMaterial;

	private readonly Material m_CameraMotionVectorsMaterial;

	private readonly Material m_ObjectMotionVectorsMaterial;

	private readonly Material m_CopyShadowsMaterial;

	private WaaaghLights m_WaaaghLights;

	private CullingPass m_IRSCullingPass;

	private SubmitPass m_IRSSubmitPass;

	private ClearPass m_ClearPass;

	private ClearGBufferPass m_ClearGBufferPass;

	private NativeShadowCasterPass m_NativeShadowCasterPass;

	private GBufferPass m_GBufferPass;

	private CopyDepthPass m_CopyDepthAfterGBufferPass;

	private ComputeTilesMinMaxZPass m_ComputeTilesMinMaxZPass;

	private DrawDecalsPass m_DrawDecalsPass;

	private SetupLightDataPass m_SetupLightDataPass;

	private LightCullingPass m_LightCullingPass;

	private DeferredLightingPass m_DeferredLightingPass;

	private DeferredLightingComputePass m_DeferredLightingComputePass;

	private DrawObjectsWithErrorPass m_DrawObjectsWithUnsupportedMaterials;

	private DrawObjectsWithErrorPass m_DrawObjectsWithMissingMaterials;

	private HbaoPass m_HbaoPass;

	private DrawSkyboxPass m_DrawSkyboxPass;

	private DrawColorPyramidPass m_DrawColorPyramidAfterOpaquePass;

	private GBufferPass m_GBufferDistortionPass;

	private CopyDepthPass m_CopyDepthAfterOpaqueDistortion;

	private DrawObjectsPass m_DrawOpaqueDistortionPass;

	private DepthPyramidPass m_DepthPyramidPass;

	private CameraMotionVectorsPass m_CameraMotionVectorsPass;

	private ObjectMotionVectorsPass m_ObjectMotionVectorsPass;

	private StochasticScreenSpaceReflectionsPass m_StochasticSSRPass;

	private DeferredReflectionsPass m_DeferredReflectionsPass;

	private FogPass m_FogPass;

	private DrawObjectsPass m_DrawTransparentPass;

	private DrawColorPyramidPass m_DrawColorPyramidAfterTransparentPass;

	private DrawDistortionVectorsPass m_DrawDistortionVectorsPass;

	private readonly DrawObjectsPass m_RenderOverlayForwardPass;

	private DrawDecalsPass m_DrawGUIDecalsPass;

	private CapturePass m_CapturePass;

	private FinalBlitPass m_FinalBlitPass;

	private PostProcessPasses m_PostProcessPasses;

	private CameraSetupPass m_CameraSetupAfterTaa;

	private CopyDepthPass m_FinalCopyDepthPass;

	private InvokeOnRenderObjectCallbackPass m_InvokeOnRenderObjectCallbackPass;

	private ComputeBuffer m_DummyComputeBuffer;

	public WaaaghRendererData Settings => m_Settings;

	internal WaaaghLights WaaaghLights => m_WaaaghLights;

	public WaaaghRenderer(WaaaghRendererData settings)
		: base(settings)
	{
		m_Settings = settings;
		m_WaaaghLights = new WaaaghLights(this);
		IndirectRenderingSystem.Instance.Initialize(m_Settings.Shaders.IndirectRenderingCullShader);
		m_DummyComputeBuffer = CreateDummyComputeBuffer();
		m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.CopyDepthSimplePS);
		m_ErrorMaterial = CoreUtils.CreateEngineMaterial("Hidden/InternalErrorShader");
		m_DeferredReflectionsMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.DeferredReflectionsShader);
		m_DeferredLightingMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.DeferredLightingShader);
		m_BlitMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.BlitShader);
		m_ColorPyramidMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.ColorPyramidShader);
		m_ApplyDistortionMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.ApplyDistortionShader);
		m_DBufferBlitMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.DBufferBlitShader);
		m_FogMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.FogShader);
		m_HbaoMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.HbaoShader);
		m_SsrResolveMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.ScreenSpaceReflectionsShaderPS);
		m_CameraMotionVectorsMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.CameraMotionVectorsPS);
		m_ObjectMotionVectorsMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.ObjectMotionVectorsPS);
		m_CopyShadowsMaterial = CoreUtils.CreateEngineMaterial(settings.Shaders.CopyCachedShadowsPS);
		if (WaaaghPipeline.Asset.DebugData != null && WaaaghPipeline.Asset.DebugData.Shaders.DebugFullscreenPS != null)
		{
			m_DebugFullscreenMaterial = CoreUtils.CreateEngineMaterial(WaaaghPipeline.Asset.DebugData.Shaders.DebugFullscreenPS);
		}
		m_IRSCullingPass = new CullingPass(RenderPassEvent.BeforeRendering);
		m_IRSSubmitPass = new SubmitPass(RenderPassEvent.BeforeRendering);
		m_ClearPass = new ClearPass(RenderPassEvent.BeforeRenderingPrePasses);
		m_ClearGBufferPass = new ClearGBufferPass(RenderPassEvent.BeforeRenderingPrePasses);
		m_NativeShadowCasterPass = new NativeShadowCasterPass(RenderPassEvent.BeforeRenderingShadows, m_CopyShadowsMaterial);
		m_GBufferPass = new GBufferPass(RenderPassEvent.BeforeRenderingGbuffer, GBufferType.Opaque);
		m_CopyDepthAfterGBufferPass = new CopyDepthPass(RenderPassEvent.BeforeRenderingGbuffer, m_CopyDepthMaterial, CopyDepthPass.CopyDepthMode.Intermediate, CopyDepthPass.PassCullingCriteria.Opaque);
		m_ComputeTilesMinMaxZPass = new ComputeTilesMinMaxZPass(RenderPassEvent.BeforeRenderingGbuffer, settings.Shaders.ComputeTilesMinMaxZCS, m_WaaaghLights);
		m_DrawDecalsPass = new DrawDecalsPass(RenderPassEvent.BeforeRenderingGbuffer, m_DBufferBlitMaterial, drawGUIDecals: false);
		m_SetupLightDataPass = new SetupLightDataPass(RenderPassEvent.BeforeRenderingGbuffer, m_WaaaghLights);
		m_LightCullingPass = new LightCullingPass(RenderPassEvent.BeforeRenderingGbuffer, settings.Shaders.LightCullingShader, m_WaaaghLights);
		m_DeferredLightingPass = new DeferredLightingPass(RenderPassEvent.BeforeRenderingGbuffer, m_DeferredReflectionsMaterial, m_DeferredLightingMaterial);
		if (SystemSupportsWaveIntrinsics() && m_Settings.Shaders.DeferredLightingCS != null)
		{
			m_DeferredLightingComputePass = new DeferredLightingComputePass(RenderPassEvent.BeforeRenderingGbuffer, m_Settings.DeferredLightingComputeDispatchTileSize, m_Settings.Shaders.DeferredLightingCS);
		}
		m_DrawObjectsWithUnsupportedMaterials = new DrawObjectsWithErrorPass(RenderPassEvent.BeforeRenderingOpaques, m_ErrorMaterial, DrawObjectsWithErrorPass.ErrorType.UnsupportedMaterials);
		m_DrawObjectsWithMissingMaterials = new DrawObjectsWithErrorPass(RenderPassEvent.BeforeRenderingOpaques, m_ErrorMaterial, DrawObjectsWithErrorPass.ErrorType.MissingMaterial);
		m_HbaoPass = new HbaoPass(RenderPassEvent.BeforeRenderingOpaques, m_HbaoMaterial);
		m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);
		m_DrawColorPyramidAfterOpaquePass = new DrawColorPyramidPass(RenderPassEvent.BeforeRenderingSkybox, ColorPyramidType.OpaqueDistortion, m_ColorPyramidMaterial, m_BlitMaterial);
		m_GBufferDistortionPass = new GBufferPass(RenderPassEvent.AfterRenderingSkybox, GBufferType.OpaqueDistortion);
		m_DrawOpaqueDistortionPass = new DrawObjectsPass(RenderPassEvent.AfterRenderingSkybox, DrawObjectsPass.RendererListType.OpaqueDistortionForward);
		m_CopyDepthAfterOpaqueDistortion = new CopyDepthPass(RenderPassEvent.AfterRenderingSkybox, m_CopyDepthMaterial, CopyDepthPass.CopyDepthMode.Intermediate, CopyDepthPass.PassCullingCriteria.OpaqueDistortion);
		m_DepthPyramidPass = new DepthPyramidPass(RenderPassEvent.AfterRenderingSkybox, settings.Shaders.DepthPyramidCS, m_CopyDepthMaterial);
		m_CameraMotionVectorsPass = new CameraMotionVectorsPass(RenderPassEvent.AfterRenderingSkybox, m_CameraMotionVectorsMaterial);
		m_ObjectMotionVectorsPass = new ObjectMotionVectorsPass(RenderPassEvent.AfterRenderingSkybox, m_ObjectMotionVectorsMaterial);
		m_StochasticSSRPass = new StochasticScreenSpaceReflectionsPass(RenderPassEvent.AfterRenderingSkybox, settings.Shaders.StochasticScreenSpaceReflectionsCS, m_BlitMaterial, m_SsrResolveMaterial, WaaaghPipeline.Asset.Textures);
		m_DeferredReflectionsPass = new DeferredReflectionsPass(RenderPassEvent.AfterRenderingSkybox, m_DeferredReflectionsMaterial, settings.Shaders.BilateralUpsampleCS);
		m_FogPass = new FogPass(RenderPassEvent.AfterRenderingSkybox, m_FogMaterial);
		m_DrawTransparentPass = new DrawObjectsPass(RenderPassEvent.BeforeRenderingTransparents, DrawObjectsPass.RendererListType.Transparent);
		m_DrawColorPyramidAfterTransparentPass = new DrawColorPyramidPass(RenderPassEvent.BeforeRenderingTransparents, ColorPyramidType.TransparentDistortion, m_ColorPyramidMaterial, m_BlitMaterial);
		m_DrawDistortionVectorsPass = new DrawDistortionVectorsPass(RenderPassEvent.BeforeRenderingTransparents, m_ApplyDistortionMaterial);
		m_RenderOverlayForwardPass = new DrawObjectsPass(RenderPassEvent.BeforeRenderingTransparents, DrawObjectsPass.RendererListType.Overlay);
		m_DrawGUIDecalsPass = new DrawDecalsPass(RenderPassEvent.BeforeRenderingTransparents, null, drawGUIDecals: true);
		m_CapturePass = new CapturePass(RenderPassEvent.AfterRendering);
		m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering, settings.PostProcessData, settings.Shaders.FinalBlitShader, settings.Shaders.FsrEasuShader);
		m_PostProcessPasses = new PostProcessPasses(settings.PostProcessData, m_BlitMaterial);
		m_CameraSetupAfterTaa = new CameraSetupPass(RenderPassEvent.AfterRenderingPostProcessing, noJitter: true);
		m_FinalCopyDepthPass = new CopyDepthPass((RenderPassEvent)1009, m_CopyDepthMaterial, CopyDepthPass.CopyDepthMode.Final, CopyDepthPass.PassCullingCriteria.None);
		m_InvokeOnRenderObjectCallbackPass = new InvokeOnRenderObjectCallbackPass(RenderPassEvent.BeforeRenderingPostProcessing);
	}

	protected override void Dispose(bool disposing)
	{
		m_WaaaghLights.Dispose();
		CoreUtils.Destroy(m_CopyDepthMaterial);
		CoreUtils.Destroy(m_ErrorMaterial);
		CoreUtils.Destroy(m_DeferredReflectionsMaterial);
		CoreUtils.Destroy(m_DeferredLightingMaterial);
		CoreUtils.Destroy(m_BlitMaterial);
		CoreUtils.Destroy(m_ColorPyramidMaterial);
		CoreUtils.Destroy(m_ApplyDistortionMaterial);
		CoreUtils.Destroy(m_DebugFullscreenMaterial);
		CoreUtils.Destroy(m_DBufferBlitMaterial);
		CoreUtils.Destroy(m_FogMaterial);
		CoreUtils.Destroy(m_HbaoMaterial);
		CoreUtils.Destroy(m_SsrResolveMaterial);
		CoreUtils.Destroy(m_CameraMotionVectorsMaterial);
		CoreUtils.Destroy(m_ObjectMotionVectorsMaterial);
		CoreUtils.Destroy(m_CopyShadowsMaterial);
		m_FinalBlitPass.Dispose();
		m_DummyComputeBuffer.Release();
		m_PostProcessPasses.Dispose();
		IndirectRenderingSystem.Instance.Dispose();
	}

	protected override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		Camera camera = cameraData.Camera;
		bool isIndirectRenderingEnabled = cameraData.IsIndirectRenderingEnabled;
		EnqueuePass(m_IRSSubmitPass);
		if (isIndirectRenderingEnabled)
		{
			EnqueuePass(m_IRSCullingPass);
		}
		if (base.DebugHandler != null)
		{
			base.DebugHandler.Setup(context, ref renderingData);
			if (base.DebugHandler.IsCompletelyOverridesRendering)
			{
				return;
			}
		}
		bool isLightingEnabled = cameraData.IsLightingEnabled;
		if (isLightingEnabled)
		{
			using (new ProfilingScope(null, ProfilingSampler.Get(WaaaghProfileId.WaaaghLightsSetup)))
			{
				m_WaaaghLights.StartSetupJobs(context, ref renderingData, m_Settings.TileSize);
			}
		}
		for (int i = 0; i < base.RendererFeatures.Count; i++)
		{
			base.RendererFeatures[i].StartSetupJobs(ref renderingData);
		}
		SetupGlobalShaderKeywords(context, ref renderingData);
		bool flag = cameraData.PostProcessEnabled && m_PostProcessPasses.IsCreated;
		bool flag2 = flag && cameraData.Antialiasing == AntialiasingMode.FastApproximateAntialiasing;
		bool num = cameraData.CameraResolveRequired && cameraData.CameraRenderTargetBufferType == CameraRenderTargetType.Scaled;
		bool flag3 = !num && flag && cameraData.Antialiasing == AntialiasingMode.TemporalAntialiasing && cameraData.TemporalAntialiasingSharpness > 0f;
		NoiseBasedPostProcessingStage noiseBasedPostProcessingStage = (num ? NoiseBasedPostProcessingStage.UpscaleBlitPass : (flag2 ? NoiseBasedPostProcessingStage.FinalPostProcessPass : (flag ? NoiseBasedPostProcessingStage.PostProcessPass : NoiseBasedPostProcessingStage.None)));
		bool flag4 = cameraData.PostProcessEnabled && m_PostProcessPasses.IsCreated;
		bool isShadowsEnabled = cameraData.IsShadowsEnabled;
		bool isSSREnabled = cameraData.IsSSREnabled;
		bool isNeedDepthPyramid = cameraData.IsNeedDepthPyramid;
		bool flag5 = flag && renderingData.CameraData.Antialiasing == AntialiasingMode.TemporalAntialiasing;
		bool flag6 = renderingData.CaptureActions != null;
		bool flag7 = cameraData.RenderType == CameraRenderType.Base || !cameraData.ClearDepth || cameraData.IsLightingEnabled || renderingData.IrsHasOpaques || renderingData.IrsHasOpaqueDistortions;
		RTHandles.SetReferenceSize(cameraData.CameraTargetDescriptor.width, cameraData.CameraTargetDescriptor.height);
		for (int j = 0; j < base.RendererFeatures.Count; j++)
		{
			base.RendererFeatures[j].CompleteSetupJobs();
		}
		for (int k = 0; k < base.RendererFeatures.Count; k++)
		{
			base.RendererFeatures[k].AddRenderPasses(this, ref renderingData);
		}
		EnqueuePass(m_ClearPass);
		EnqueuePass(m_ClearGBufferPass);
		if (renderingData.ShadowData.ShadowQuality != ShadowQuality.Disable && isLightingEnabled && isShadowsEnabled)
		{
			EnqueuePass(m_NativeShadowCasterPass);
		}
		EnqueuePass(m_GBufferPass);
		if (flag7)
		{
			EnqueuePass(m_CopyDepthAfterGBufferPass);
			if (isLightingEnabled)
			{
				EnqueuePass(m_ComputeTilesMinMaxZPass);
			}
		}
		EnqueuePass(m_DrawDecalsPass);
		if (isLightingEnabled)
		{
			EnqueuePass(m_SetupLightDataPass);
			EnqueuePass(m_LightCullingPass);
			if (m_Settings.DeferredLightingMode == DeferredLightingMode.Compute && m_DeferredLightingComputePass != null)
			{
				EnqueuePass(m_DeferredLightingComputePass);
			}
			else
			{
				EnqueuePass(m_DeferredLightingPass);
			}
		}
		if (flag)
		{
			EnqueuePass(m_PostProcessPasses.BeforeTransparentPostProcessPass);
		}
		EnqueuePass(m_DrawObjectsWithUnsupportedMaterials);
		EnqueuePass(m_DrawObjectsWithMissingMaterials);
		if (flag)
		{
			EnqueuePass(m_HbaoPass);
		}
		if (flag4)
		{
			EnqueuePass(m_PostProcessPasses.ColorGradingLutPass);
		}
		if (camera.clearFlags == CameraClearFlags.Skybox && cameraData.RenderType != CameraRenderType.Overlay && (RenderSettings.skybox != null || (camera.TryGetComponent<Skybox>(out var component) && component.material != null)))
		{
			EnqueuePass(m_DrawSkyboxPass);
		}
		EnqueuePass(m_DrawColorPyramidAfterOpaquePass);
		EnqueuePass(m_GBufferDistortionPass);
		EnqueuePass(m_DrawOpaqueDistortionPass);
		if (flag7)
		{
			EnqueuePass(m_CopyDepthAfterOpaqueDistortion);
		}
		if (flag5 || (isLightingEnabled && isSSREnabled))
		{
			EnqueuePass(m_CameraMotionVectorsPass);
			EnqueuePass(m_ObjectMotionVectorsPass);
		}
		if (isLightingEnabled)
		{
			if (isSSREnabled)
			{
				if (isNeedDepthPyramid)
				{
					EnqueuePass(m_DepthPyramidPass);
				}
				EnqueuePass(m_StochasticSSRPass);
			}
			EnqueuePass(m_DeferredReflectionsPass);
		}
		if (cameraData.IsFogEnabled)
		{
			EnqueuePass(m_FogPass);
		}
		EnqueuePass(m_DrawTransparentPass);
		EnqueuePass(m_DrawColorPyramidAfterTransparentPass);
		EnqueuePass(m_DrawDistortionVectorsPass);
		EnqueuePass(m_RenderOverlayForwardPass);
		EnqueuePass(m_DrawGUIDecalsPass);
		if (flag)
		{
			m_PostProcessPasses.PostProcessPass.ApplyNoiseBasedEffects = noiseBasedPostProcessingStage == NoiseBasedPostProcessingStage.PostProcessPass;
			m_PostProcessPasses.PostProcessPass.ApplyTaaRcas = noiseBasedPostProcessingStage == NoiseBasedPostProcessingStage.PostProcessPass && flag3;
			EnqueuePass(m_PostProcessPasses.PostProcessPass);
		}
		if (flag2)
		{
			m_PostProcessPasses.FinalPostProcessPass.ApplyNoiseBasedEffects = noiseBasedPostProcessingStage == NoiseBasedPostProcessingStage.FinalPostProcessPass;
			m_PostProcessPasses.FinalPostProcessPass.ApplyTaaRcas = noiseBasedPostProcessingStage == NoiseBasedPostProcessingStage.FinalPostProcessPass && flag3;
			EnqueuePass(m_PostProcessPasses.FinalPostProcessPass);
		}
		EnqueuePass(m_InvokeOnRenderObjectCallbackPass);
		if (flag5)
		{
			EnqueuePass(m_CameraSetupAfterTaa);
		}
		if (flag6)
		{
			EnqueuePass(m_CapturePass);
		}
		if (cameraData.CameraResolveRequired)
		{
			m_FinalBlitPass.ApplyTaaRcas = noiseBasedPostProcessingStage == NoiseBasedPostProcessingStage.UpscaleBlitPass && flag3;
			m_FinalBlitPass.ApplyNoiseBasedEffects = flag && noiseBasedPostProcessingStage == NoiseBasedPostProcessingStage.UpscaleBlitPass;
			EnqueuePass(m_FinalBlitPass);
		}
		if (cameraData.TargetDepthTexture != null)
		{
			EnqueuePass(m_FinalCopyDepthPass);
		}
		if (isLightingEnabled)
		{
			m_WaaaghLights.CompleteSetupJobs(context, ref renderingData);
		}
		IndirectRenderingSystem.Instance.DebugCamera = null;
	}

	protected override void InitRenderGraphResources(ref RenderingData renderingData, RenderGraphResources resources)
	{
		TextureDesc textureDesc = RenderingUtils.CreateTextureDesc("GBuffer0", renderingData.CameraData.CameraTargetDescriptor);
		textureDesc.filterMode = FilterMode.Point;
		textureDesc.depthBufferBits = DepthBits.None;
		textureDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		TextureDesc desc = textureDesc;
		desc.name = "CameraAlbedoRT";
		resources.CameraAlbedoRT = resources.RenderGraph.CreateTexture(in desc);
		TextureDesc desc2 = textureDesc;
		desc2.name = "CameraSpecularRT";
		resources.CameraSpecularRT = resources.RenderGraph.CreateTexture(in desc2);
		TextureDesc desc3 = textureDesc;
		desc3.name = "CameraNormalsRT";
		resources.CameraNormalsRT = resources.RenderGraph.CreateTexture(in desc3);
		TextureDesc desc4 = textureDesc;
		desc4.name = "CameraBakedGIRT";
		desc4.colorFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		desc4.depthBufferBits = DepthBits.None;
		resources.CameraBakedGIRT = resources.RenderGraph.CreateTexture(in desc4);
		TextureDesc desc5 = textureDesc;
		desc5.name = "CameraShadowmaskRT";
		resources.CameraShadowmaskRT = resources.RenderGraph.CreateTexture(in desc5);
		TextureDesc desc6 = textureDesc;
		desc6.name = "CameraTranslucencyRT";
		resources.CameraTranslucencyRT = resources.RenderGraph.CreateTexture(in desc6);
		if (renderingData.CameraData.CameraBuffer.HistoryDepthFramesCount > 0)
		{
			resources.CameraDepthCopyRT = resources.CameraHistoryDepthBuffer;
		}
		else
		{
			TextureDesc desc7 = textureDesc;
			desc7.name = "CameraDepthCopyRT";
			desc7.colorFormat = GraphicsFormat.R32_SFloat;
			desc7.depthBufferBits = DepthBits.None;
			resources.CameraDepthCopyRT = resources.RenderGraph.CreateTexture(in desc7);
		}
		TextureDesc desc8 = textureDesc;
		desc8.name = "CameraColorPyramidRT";
		desc8.colorFormat = renderingData.CameraData.CameraTargetDescriptor.graphicsFormat;
		desc8.useMipMap = true;
		desc8.autoGenerateMips = false;
		desc8.filterMode = FilterMode.Bilinear;
		resources.CameraColorPyramidRT = resources.RenderGraph.CreateTexture(in desc8);
		resources.LightDataConstantBuffer = resources.RenderGraph.ImportComputeBuffer(m_WaaaghLights.LightDataConstantBuffer);
		resources.LightVolumeDataConstantBuffer = resources.RenderGraph.ImportComputeBuffer(m_WaaaghLights.LightVolumeDataConstantBuffer);
		resources.ZBinsConstantBuffer = resources.RenderGraph.ImportComputeBuffer(m_WaaaghLights.ZBinsConstantBuffer);
		resources.LightTilesBuffer = resources.RenderGraph.ImportComputeBuffer(m_WaaaghLights.LightTilesBuffer);
		resources.RendererLists.Init(ref renderingData);
	}

	private void SetupGlobalShaderKeywords(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		CoreUtils.SetKeyword(commandBuffer, ShaderKeywordStrings.SHADOWS_SHADOWMASK, m_WaaaghLights.ShadowmaskEnabled);
		commandBuffer.SetGlobalFloat(ShaderPropertyId._VolumetricLightingEnabled, 0f);
		bool num = WaaaghPipeline.Asset.ShadowSettings.ShadowQuality == ShadowQuality.Disable;
		bool flag = Mathf.Approximately(renderingData.CameraData.MaxShadowDistance, 0f);
		if (!num && !flag)
		{
			commandBuffer.SetGlobalFloat(ShaderPropertyId._GlobalShadowsEnabled, 1f);
		}
		else
		{
			commandBuffer.SetGlobalFloat(ShaderPropertyId._GlobalShadowsEnabled, 0f);
		}
		Texture2D texture2D = (SystemInfo.usesReversedZBuffer ? Texture2D.blackTexture : Texture2D.whiteTexture);
		commandBuffer.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, texture2D);
		commandBuffer.SetGlobalTexture(ShaderPropertyId._CameraDepthTexture, texture2D);
		commandBuffer.SetGlobalTexture(ShaderPropertyId._ShadowmapRT, texture2D);
		PBD.SetDummyComputeBuffer(commandBuffer, m_DummyComputeBuffer);
		IndirectRenderingSystem.SetupDummyComputeBufferStubs(commandBuffer, m_DummyComputeBuffer);
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
	{
		bool num = WaaaghPipeline.Asset.ShadowSettings.ShadowQuality == ShadowQuality.Disable;
		bool flag = Mathf.Approximately(cameraData.MaxShadowDistance, 0f);
		bool flag2 = !cameraData.IsLightingEnabled;
		if (num || flag || flag2)
		{
			cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
		}
		if (flag2)
		{
			cullingParameters.cullingOptions &= ~CullingOptions.NeedsLighting;
		}
		cullingParameters.cullingOptions &= ~CullingOptions.OcclusionCull;
		cullingParameters.cullingOptions |= CullingOptions.DisablePerObjectCulling;
		cullingParameters.shadowDistance = cameraData.MaxShadowDistance;
	}

	private static ComputeBuffer CreateDummyComputeBuffer()
	{
		ComputeBuffer obj = new ComputeBuffer(1, 64, ComputeBufferType.Structured)
		{
			name = "Dummy"
		};
		NativeArray<float> data = new NativeArray<float>(16, Allocator.Temp);
		obj.SetData(data);
		data.Dispose();
		return obj;
	}

	private static bool SystemSupportsWaveIntrinsics()
	{
		GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
		if (graphicsDeviceType != GraphicsDeviceType.Direct3D12 && graphicsDeviceType != GraphicsDeviceType.PlayStation5 && graphicsDeviceType != GraphicsDeviceType.PlayStation5NGGC && graphicsDeviceType != GraphicsDeviceType.XboxOneD3D12 && graphicsDeviceType != GraphicsDeviceType.GameCoreXboxOne)
		{
			return graphicsDeviceType == GraphicsDeviceType.GameCoreXboxSeries;
		}
		return true;
	}
}
