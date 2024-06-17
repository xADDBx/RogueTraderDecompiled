using System;
using System.Collections.Generic;
using System.Reflection;
using Owlcat.Runtime.Core.ProfilingCounters;
using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Waaagh;

public class WaaaghPipeline : UnityEngine.Rendering.RenderPipeline
{
	private readonly struct CameraStackData
	{
		public readonly int lastCameraIndex;

		public readonly int lastScaledCameraIndex;

		public readonly bool hasCameraWithPostProcessingEnabled;

		public readonly bool hasCameraWithVolumetricLightingEnabled;

		public readonly bool hasCameraWithHdrEnabled;

		public CameraStackData(int lastCameraIndex, int lastScaledCameraIndex, bool hasCameraWithPostProcessingEnabled, bool hasCameraWithVolumetricLightingEnabled, bool hasCameraWithHdrEnabled)
		{
			this.lastCameraIndex = lastCameraIndex;
			this.lastScaledCameraIndex = lastScaledCameraIndex;
			this.hasCameraWithPostProcessingEnabled = hasCameraWithPostProcessingEnabled;
			this.hasCameraWithVolumetricLightingEnabled = hasCameraWithVolumetricLightingEnabled;
			this.hasCameraWithHdrEnabled = hasCameraWithHdrEnabled;
		}
	}

	private const int k_DefaultRenderingLayerMask = 1;

	private const float kHexRatio = 1.7320508f;

	private const int kWrapTime = 14400;

	private const float kRenderScaleThreshold = 0.05f;

	public const string kRenderPipelineName = "OwlcatPipeline";

	private WaaaghPipelineGlobalSettings m_GlobalSettings;

	private RenderGraph m_RenderGraph;

	private ShadowManager m_ShadowManager;

	private WaaaghDebugData m_DegubData;

	private LightCookieManager m_LightCookieManager;

	private LocalVolumetricFogManager m_LocalVolumetricFogManager;

	private Scene m_ActiveScene;

	public static Action<Camera> VolumeManagerUpdated;

	private Comparison<Camera> m_CameraComparison = (Camera camera1, Camera camera2) => (int)camera1.depth - (int)camera2.depth;

	public static WaaaghPipelineAsset Asset => GraphicsSettings.currentRenderPipeline as WaaaghPipelineAsset;

	public static float MinRenderScale => 0.1f;

	public static float MaxRenderScale => 2f;

	public static float MaxShadowBias => 10f;

	public WaaaghPipeline(WaaaghPipelineAsset asset)
	{
		m_GlobalSettings = WaaaghPipelineGlobalSettings.Instance;
		SetSupportedRenderingFeatures();
		Shader.globalRenderPipeline = "OwlcatPipeline";
		Lightmapping.SetDelegate(LightmappingDelegate);
		RenderingUtils.ClearSystemInfoCache();
		m_RenderGraph = new RenderGraph("WaaaghGraph");
		typeof(RenderGraph).GetField("m_RendererListCulling", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(m_RenderGraph, true);
		m_ShadowManager = new ShadowManager(asset);
		DebugManager.instance.RefreshEditor();
		m_DegubData = asset.DebugData;
		if (m_DegubData != null)
		{
			m_DegubData.RegisterDebug();
		}
		RTHandles.Initialize(Screen.width, Screen.height);
		DebugManager.instance.enableRuntimeUI = false;
		RenderGraph renderGraph = m_RenderGraph;
		LightCookieManager.Settings settings = new LightCookieManager.Settings
		{
			atlasTextureResolution = asset.LightCookieSettings.Resolution,
			atlasTextureFormat = asset.LightCookieSettings.Format
		};
		m_LightCookieManager = new LightCookieManager(renderGraph, in settings);
		m_LocalVolumetricFogManager = new LocalVolumetricFogManager(asset.LocalVolumetricFogSettings, asset.LocalVolumetricFogSettings.Texture3DAtlasCS);
	}

	protected override void Dispose(bool disposing)
	{
		if (m_DegubData != null)
		{
			m_DegubData.UnregisterDebug();
			m_DegubData = null;
		}
		Blitter.Cleanup();
		base.Dispose(disposing);
		Shader.globalRenderPipeline = "";
		SupportedRenderingFeatures.active = new SupportedRenderingFeatures();
		Lightmapping.ResetDelegate();
		m_RenderGraph.Cleanup();
		m_RenderGraph = null;
		m_ShadowManager.Dispose();
		m_ShadowManager = null;
		WaaaghCameraBuffers.Cleanup();
		ConstantBuffer.ReleaseAll();
		m_LightCookieManager.Dispose();
		m_LocalVolumetricFogManager.ReleaseAtlas();
	}

	protected override void Render(ScriptableRenderContext context, Camera[] cameras)
	{
		Render(context, new List<Camera>(cameras));
	}

	protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
	{
		using (Counters.Render?.Measure())
		{
			using (new ProfilingScope(null, ProfilingSampler.Get(WaaaghProfileId.PipelineBeginContextRendering)))
			{
				UnityEngine.Rendering.RenderPipeline.BeginContextRendering(context, cameras);
			}
			UpdateRealtimeGI();
			UpdateFrameCount(cameras);
			GraphicsSettings.lightsUseLinearIntensity = QualitySettings.activeColorSpace == ColorSpace.Linear;
			GraphicsSettings.lightsUseColorTemperature = true;
			GraphicsSettings.useScriptableRenderPipelineBatching = Asset.UseSRPBatcher;
			GraphicsSettings.defaultRenderingLayerMask = 1u;
			SetupPerFrameShaderConstants();
			WaaaghCameraBuffers.CleanUnused();
			SortCameras(cameras);
			SetupGlobalGPUState(context);
			foreach (Camera camera in cameras)
			{
				if (camera == null)
				{
					continue;
				}
				if (IsGameCamera(camera) && camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
				{
					List<WaaaghAdditionalCameraData> value;
					using (ListPool<WaaaghAdditionalCameraData>.Get(out value))
					{
						if (TryBuildCameraStack(component, value))
						{
							RenderCameraStack(context, value);
						}
					}
				}
				else
				{
					RenderSingleCamera(context, camera);
				}
			}
			m_RenderGraph.EndFrame();
			using (new ProfilingScope(null, ProfilingSampler.Get(WaaaghProfileId.PipelineEndContextRendering)))
			{
				UnityEngine.Rendering.RenderPipeline.EndContextRendering(context, cameras);
			}
		}
	}

	private void SetupGlobalGPUState(ScriptableRenderContext context)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		commandBuffer.BeginSample("SetupGlobalGPUState");
		commandBuffer.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarMask, Texture2D.blackTexture);
		commandBuffer.EndSample("SetupGlobalGPUState");
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	private void UpdateRealtimeGI()
	{
		Scene activeScene = SceneManager.GetActiveScene();
		if (activeScene != m_ActiveScene)
		{
			m_ActiveScene = activeScene;
			UpdateLigths();
		}
	}

	private static void UpdateLigths()
	{
		Light[] array = UnityEngine.Object.FindObjectsOfType<Light>(includeInactive: true);
		Dictionary<Light, bool> dictionary = new Dictionary<Light, bool>();
		Light[] array2 = array;
		foreach (Light light in array2)
		{
			dictionary[light] = light.enabled;
		}
		array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].enabled = false;
		}
		array2 = array;
		foreach (Light light2 in array2)
		{
			light2.enabled = dictionary[light2];
		}
	}

	private void RenderCameraStack(ScriptableRenderContext context, List<WaaaghAdditionalCameraData> additionalCameraDataList)
	{
		CameraStackData cameraStackData = GetCameraStackData(additionalCameraDataList);
		CameraData cameraData = default(CameraData);
		int i = 0;
		for (int num = additionalCameraDataList.Count - 1; i <= num; i++)
		{
			WaaaghAdditionalCameraData waaaghAdditionalCameraData = additionalCameraDataList[i];
			Camera camera = waaaghAdditionalCameraData.camera;
			using (new ProfilingScope(null, ProfilingSampler.Get(WaaaghProfileId.PipelineBeginCameraRendering)))
			{
				UnityEngine.Rendering.RenderPipeline.BeginCameraRendering(context, camera);
			}
			UpdateVolumeFramework(camera, waaaghAdditionalCameraData);
			if (i == 0)
			{
				InitializeCameraData(additionalCameraDataList[0].camera, additionalCameraDataList[0], in cameraStackData, i, cameraStackData.hasCameraWithPostProcessingEnabled && IsScreenSpaceReflectionsEnabled(), out cameraData);
			}
			else
			{
				InitializeAdditionalCameraData(camera, waaaghAdditionalCameraData, in cameraStackData, i, ref cameraData);
			}
			RenderSingleCamera(context, ref cameraData);
			using (new ProfilingScope(null, ProfilingSampler.Get(WaaaghProfileId.PipelineEndCameraRendering)))
			{
				UnityEngine.Rendering.RenderPipeline.EndCameraRendering(context, camera);
			}
		}
	}

	private static bool TryBuildCameraStack(WaaaghAdditionalCameraData baseCameraAdditionalData, List<WaaaghAdditionalCameraData> additionalCameraDataList)
	{
		if (baseCameraAdditionalData.RenderType != 0)
		{
			return false;
		}
		additionalCameraDataList.Add(baseCameraAdditionalData);
		baseCameraAdditionalData.UpdateCameraStack();
		List<Camera> cameraStack = baseCameraAdditionalData.CameraStack;
		if (cameraStack != null)
		{
			Type type = baseCameraAdditionalData.ScriptableRenderer.GetType();
			foreach (Camera item in cameraStack)
			{
				if (!(item == null) && item.isActiveAndEnabled && item.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
				{
					if (component.RenderType != CameraRenderType.Overlay)
					{
						Debug.LogWarning("Stack can only contain Overlay cameras. " + item.name + " will skip rendering.", item);
					}
					else if (type != component.ScriptableRenderer.GetType())
					{
						Debug.LogWarning("Only cameras with compatible renderer types can be stacked. " + item.name + " will skip rendering", item);
					}
					else
					{
						additionalCameraDataList.Add(component);
					}
				}
			}
		}
		return true;
	}

	private static CameraStackData GetCameraStackData(Camera camera, WaaaghAdditionalCameraData additionalCameraData)
	{
		return new CameraStackData(0, -1, hasCameraWithPostProcessingEnabled: false, GetRendererForCamera(camera, additionalCameraData).IsVolumetricLightingEnabled, camera.allowHDR);
	}

	private static CameraStackData GetCameraStackData(List<WaaaghAdditionalCameraData> additionalCameraDataList)
	{
		bool hasCameraWithPostProcessingEnabled = false;
		bool hasCameraWithVolumetricLightingEnabled = false;
		bool hasCameraWithHdrEnabled = false;
		int lastScaledCameraIndex = -1;
		int i = 0;
		for (int count = additionalCameraDataList.Count; i < count; i++)
		{
			WaaaghAdditionalCameraData waaaghAdditionalCameraData = additionalCameraDataList[i];
			if (waaaghAdditionalCameraData.RenderPostProcessing)
			{
				hasCameraWithPostProcessingEnabled = true;
			}
			if (GetRendererForCamera(waaaghAdditionalCameraData.camera, waaaghAdditionalCameraData).IsVolumetricLightingEnabled)
			{
				hasCameraWithVolumetricLightingEnabled = true;
			}
			if (waaaghAdditionalCameraData.camera.allowHDR)
			{
				hasCameraWithHdrEnabled = true;
			}
			if (waaaghAdditionalCameraData.AllowRenderScaling)
			{
				lastScaledCameraIndex = i;
			}
		}
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2)
		{
			hasCameraWithPostProcessingEnabled = false;
		}
		return new CameraStackData(additionalCameraDataList.Count - 1, lastScaledCameraIndex, hasCameraWithPostProcessingEnabled, hasCameraWithVolumetricLightingEnabled, hasCameraWithHdrEnabled);
	}

	public void RenderSingleCamera(ScriptableRenderContext context, Camera camera)
	{
		using (new ProfilingScope(null, ProfilingSampler.Get(WaaaghProfileId.PipelineBeginCameraRendering)))
		{
			UnityEngine.Rendering.RenderPipeline.BeginCameraRendering(context, camera);
		}
		UpdateVolumeFramework(camera, null);
		WaaaghAdditionalCameraData component = null;
		if (IsGameCamera(camera))
		{
			camera.gameObject.TryGetComponent<WaaaghAdditionalCameraData>(out component);
		}
		if (component != null && component.RenderType != 0)
		{
			Debug.LogWarning("Only Base cameras can be rendered with standalone RenderSingleCamera. Camera will be skipped.");
			return;
		}
		WaaaghAdditionalCameraData additionalCameraData = component;
		CameraStackData cameraStackData = GetCameraStackData(camera, component);
		InitializeCameraData(camera, additionalCameraData, in cameraStackData, 0, isSsrEnabledInStack: false, out var cameraData);
		RenderSingleCamera(context, ref cameraData);
		using (new ProfilingScope(null, ProfilingSampler.Get(WaaaghProfileId.PipelineEndCameraRendering)))
		{
			UnityEngine.Rendering.RenderPipeline.EndCameraRendering(context, camera);
		}
	}

	private void RenderSingleCamera(ScriptableRenderContext context, ref CameraData cameraData)
	{
		Camera camera = cameraData.Camera;
		ScriptableRenderer renderer = cameraData.Renderer;
		if (renderer == null)
		{
			Debug.LogWarning($"Trying to render {camera.name} with an invalid renderer. Camera rendering will be skipped.");
			return;
		}
		VFXManager.PrepareCamera(camera);
		if (cameraData.Camera.TryGetCullingParameters(out var cullingParameters))
		{
			ScriptableRenderer.Current = renderer;
			_ = cameraData.IsSceneViewCamera;
			using (new ProfilingScope(null, ProfilingSampler.Get(WaaaghProfileId.RendererSetupCullingParameters)))
			{
				renderer.SetupCullingParameters(ref cullingParameters, ref cameraData);
			}
			CullingResults cullResults = context.Cull(ref cullingParameters);
			InitializeRenderingData(Asset, ref cameraData, ref cullResults, out var renderingData);
			using (new ProfilingScope(null, ProfilingSampler.Get(WaaaghProfileId.RendererSetup)))
			{
				renderer.SetupInternal(context, ref renderingData);
			}
			using (new ProfilingScope(null, ProfilingSampler.Get(WaaaghProfileId.RendererExecute)))
			{
				renderer.Execute(context, ref renderingData);
			}
			context.Submit();
			cameraData.CameraBuffer.PostRender(ref cameraData);
		}
	}

	private void UpdateFrameCount(List<Camera> cameras)
	{
		FrameId.Update();
	}

	private static void UpdateVolumeFramework(Camera camera, WaaaghAdditionalCameraData additionalCameraData)
	{
		if (!((camera.cameraType == CameraType.SceneView) | (additionalCameraData != null && additionalCameraData.RequiresVolumeFrameworkUpdate)) && (bool)additionalCameraData)
		{
			if (additionalCameraData.VolumeStack == null)
			{
				camera.UpdateVolumeStack(additionalCameraData);
			}
			VolumeManager.instance.stack = additionalCameraData.VolumeStack;
		}
		else
		{
			camera.GetVolumeLayerMaskAndTrigger(additionalCameraData, out var layerMask, out var trigger);
			VolumeManager.instance.ResetMainStack();
			VolumeManager.instance.Update(trigger, layerMask);
			OnVolumeManagerUpdate(camera);
		}
	}

	private static void OnVolumeManagerUpdate(Camera camera)
	{
		VolumeManagerUpdated?.Invoke(camera);
	}

	private void SetupPerFrameShaderConstants()
	{
		SphericalHarmonicsL2 ambientProbe = UnityEngine.RenderSettings.ambientProbe;
		Color color = CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * UnityEngine.RenderSettings.reflectionIntensity);
		Shader.SetGlobalVector(ShaderPropertyId._GlossyEnvironmentColor, color);
		Shader.SetGlobalVector(ShaderPropertyId._GlossyEnvironmentCubeMapHDR, ReflectionProbe.defaultTextureHDRDecodeValues);
		Shader.SetGlobalTexture(ShaderPropertyId._GlossyEnvironmentCubeMap, ReflectionProbe.defaultTexture);
		Shader.SetGlobalVector(ShaderPropertyId.unity_AmbientSky, CoreUtils.ConvertSRGBToActiveColorSpace(UnityEngine.RenderSettings.ambientSkyColor));
		Shader.SetGlobalVector(ShaderPropertyId.unity_AmbientEquator, CoreUtils.ConvertSRGBToActiveColorSpace(UnityEngine.RenderSettings.ambientEquatorColor));
		Shader.SetGlobalVector(ShaderPropertyId.unity_AmbientGround, CoreUtils.ConvertSRGBToActiveColorSpace(UnityEngine.RenderSettings.ambientGroundColor));
		Shader.SetGlobalVector(ShaderPropertyId._SubtractiveShadowColor, CoreUtils.ConvertSRGBToActiveColorSpace(UnityEngine.RenderSettings.subtractiveShadowColor));
		Shader.SetGlobalVector(ShaderPropertyId._HexRatio, new Vector4(1f, 1.7320508f));
		if (Asset.TerrainSettings.TriplanarEnabled)
		{
			Shader.EnableKeyword(ShaderKeywordStrings._TRIPLANAR);
		}
		else
		{
			Shader.DisableKeyword(ShaderKeywordStrings._TRIPLANAR);
		}
	}

	private void InitializeCameraData(Camera camera, WaaaghAdditionalCameraData additionalCameraData, in CameraStackData cameraStackData, int cameraIndex, bool isSsrEnabledInStack, out CameraData cameraData)
	{
		cameraData = default(CameraData);
		cameraData.IsLightingEnabled = true;
		cameraData.IsShadowsEnabled = true;
		cameraData.IsSSREnabled = false;
		cameraData.IsSSREnabledInStack = isSsrEnabledInStack;
		cameraData.IsNeedDepthPyramid = false;
		cameraData.IsFogEnabled = UnityEngine.RenderSettings.fog;
		cameraData.IsIndirectRenderingEnabled = true;
		cameraData.IsSceneViewInPrefabEditMode = false;
		InitializeStackedCameraData(camera, additionalCameraData, in cameraStackData, ref cameraData);
		InitializeAdditionalCameraData(camera, additionalCameraData, in cameraStackData, cameraIndex, ref cameraData);
	}

	private void InitializeStackedCameraData(Camera baseCamera, WaaaghAdditionalCameraData baseAdditionalCameraData, in CameraStackData cameraStackData, ref CameraData cameraData)
	{
		WaaaghPipelineAsset asset = Asset;
		cameraData.TargetTexture = baseCamera.targetTexture;
		cameraData.CameraType = baseCamera.cameraType;
		if (cameraData.IsSceneViewCamera)
		{
			cameraData.IsStopNaNEnabled = false;
		}
		else if (baseAdditionalCameraData != null)
		{
			cameraData.IsLightingEnabled = baseAdditionalCameraData.IsLightingEnabled;
			cameraData.IsShadowsEnabled = baseAdditionalCameraData.RenderShadows;
			cameraData.IsStopNaNEnabled = baseAdditionalCameraData.StopNaN && SystemInfo.graphicsShaderLevel >= 35;
			cameraData.IsIndirectRenderingEnabled = baseAdditionalCameraData.AllowIndirectRendering;
			cameraData.IsSSREnabled = cameraData.PostProcessEnabled && IsScreenSpaceReflectionsEnabled();
			cameraData.IsNeedDepthPyramid = cameraData.PostProcessEnabled && IsDepthPyramidNeed();
			cameraData.IsStochasticSSR = cameraData.IsSSREnabled && IsStochasticSSR();
			cameraData.TargetDepthTexture = baseAdditionalCameraData.TargetDepthTexture;
		}
		else
		{
			cameraData.IsStopNaNEnabled = false;
		}
		cameraData.IsHdrEnabled = cameraStackData.hasCameraWithHdrEnabled && asset.SupportsHDR;
		cameraData.HDRColorBufferPrecision = asset.HDRColorBufferPrecision;
		cameraData.FinalTargetViewport = baseCamera.pixelRect;
		cameraData.FinalTargetAspectRatio = baseCamera.aspect;
		if (baseCamera.cameraType == CameraType.Game && cameraStackData.lastScaledCameraIndex >= 0 && Mathf.Abs(1f - Asset.RenderScale) > 0.05f && baseCamera.targetTexture == null)
		{
			cameraData.RenderScale = Asset.RenderScale;
			cameraData.ScalingMode = (((double)cameraData.RenderScale < 1.0) ? ImageScalingMode.Upscaling : ImageScalingMode.Downscaling);
			cameraData.UpscalingFilter = ResolveUpscalingFilterSelection(baseCamera.pixelRect.size, cameraData.RenderScale, Asset.UpscalingFilter);
			cameraData.FsrSharpness = (Asset.FsrOverrideSharpness ? Asset.FsrSharpness : 0.92f);
		}
		else
		{
			cameraData.RenderScale = 1f;
			cameraData.ScalingMode = ImageScalingMode.None;
			cameraData.UpscalingFilter = ImageUpscalingFilter.Linear;
			cameraData.FsrSharpness = 0f;
		}
		SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
		SortingCriteria sortingCriteria2 = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
		bool hasHiddenSurfaceRemovalOnGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU;
		bool flag = (baseCamera.opaqueSortMode == OpaqueSortMode.Default && hasHiddenSurfaceRemovalOnGPU) || baseCamera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;
		cameraData.DefaultOpaqueSortFlags = (flag ? sortingCriteria2 : sortingCriteria);
	}

	private static void InitializeAdditionalCameraData(Camera camera, WaaaghAdditionalCameraData additionalCameraData, in CameraStackData cameraStackData, int cameraIndex, ref CameraData cameraData)
	{
		WaaaghPipelineAsset asset = Asset;
		cameraData.Camera = camera;
		bool flag = asset.ShadowSettings.ShadowQuality != ShadowQuality.Disable;
		cameraData.MaxShadowDistance = Mathf.Min(asset.ShadowSettings.ShadowDistance, camera.farClipPlane);
		cameraData.MaxShadowDistance = ((flag && cameraData.MaxShadowDistance >= camera.nearClipPlane) ? cameraData.MaxShadowDistance : 0f);
		cameraData.Renderer = GetRendererForCamera(camera, additionalCameraData);
		bool isSceneViewCamera = cameraData.IsSceneViewCamera;
		if (isSceneViewCamera)
		{
			cameraData.RenderType = CameraRenderType.Base;
			cameraData.ClearDepth = true;
			cameraData.PostProcessEnabled = CoreUtils.ArePostProcessesEnabled(camera);
			cameraData.IsSSREnabled = cameraData.PostProcessEnabled && IsScreenSpaceReflectionsEnabled();
			cameraData.IsNeedDepthPyramid = cameraData.PostProcessEnabled && IsDepthPyramidNeed();
			cameraData.IsStochasticSSR = cameraData.IsSSREnabled && IsStochasticSSR();
			cameraData.RequiresDepthTexture = asset.SupportsCameraDepthTexture;
			cameraData.RequiresOpaqueTexture = asset.SupportsCameraOpaqueTexture;
			cameraData.Antialiasing = AntialiasingMode.None;
			cameraData.AntialiasingQuality = AntialiasingQuality.High;
			cameraData.TemporalAntialiasingSharpness = 0f;
			cameraData.IsDitheringEnabled = false;
		}
		else if (additionalCameraData != null)
		{
			cameraData.RenderType = additionalCameraData.RenderType;
			cameraData.ClearDepth = additionalCameraData.RenderType == CameraRenderType.Base || additionalCameraData.ClearDepth;
			cameraData.IsLightingEnabled = additionalCameraData.IsLightingEnabled;
			cameraData.IsShadowsEnabled = additionalCameraData.RenderShadows;
			cameraData.PostProcessEnabled = additionalCameraData.RenderPostProcessing;
			cameraData.MaxShadowDistance = (additionalCameraData.RenderShadows ? cameraData.MaxShadowDistance : 0f);
			cameraData.RequiresDepthTexture = additionalCameraData.RequiresDepthTexture;
			cameraData.RequiresOpaqueTexture = additionalCameraData.RequiresColorTexture;
			cameraData.IsIndirectRenderingEnabled = additionalCameraData.AllowIndirectRendering;
			cameraData.IsSSREnabled = cameraData.PostProcessEnabled && IsScreenSpaceReflectionsEnabled();
			cameraData.IsNeedDepthPyramid = cameraData.PostProcessEnabled && IsDepthPyramidNeed();
			cameraData.IsStochasticSSR = cameraData.IsSSREnabled && IsStochasticSSR();
			cameraData.Antialiasing = additionalCameraData.Antialiasing;
			cameraData.AntialiasingQuality = additionalCameraData.AntialiasingQuality;
			cameraData.TemporalAntialiasingSharpness = ((additionalCameraData.Antialiasing == AntialiasingMode.TemporalAntialiasing) ? GetTemporalAntialiasingSharpness() : 0f);
			cameraData.IsDitheringEnabled = additionalCameraData.Dithering;
		}
		else
		{
			cameraData.RenderType = CameraRenderType.Base;
			cameraData.ClearDepth = true;
			cameraData.PostProcessEnabled = false;
			cameraData.RequiresDepthTexture = asset.SupportsCameraDepthTexture;
			cameraData.RequiresOpaqueTexture = asset.SupportsCameraOpaqueTexture;
			cameraData.Antialiasing = AntialiasingMode.None;
			cameraData.AntialiasingQuality = AntialiasingQuality.High;
			cameraData.TemporalAntialiasingSharpness = 0f;
			cameraData.IsDitheringEnabled = false;
		}
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2)
		{
			cameraData.PostProcessEnabled = false;
		}
		if (camera.cameraType == CameraType.Reflection)
		{
			cameraData.PostProcessEnabled = false;
		}
		cameraData.RequiresDepthTexture |= isSceneViewCamera || CheckPostProcessForDepth(in cameraData);
		bool num = cameraData.RenderType == CameraRenderType.Overlay;
		if (num)
		{
			cameraData.RequiresDepthTexture = false;
			cameraData.RequiresOpaqueTexture = false;
		}
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		if (num && !camera.orthographic && !Mathf.Approximately(cameraData.FinalTargetAspectRatio, camera.aspect))
		{
			float m = camera.projectionMatrix.m00 * camera.aspect / cameraData.FinalTargetAspectRatio;
			projectionMatrix.m00 = m;
		}
		cameraData.WorldSpaceCameraPos = camera.transform.position;
		bool flag2 = !Mathf.Approximately(cameraData.RenderScale, 1f);
		cameraData.CameraRenderTargetBufferType = ((!flag2 || cameraIndex > cameraStackData.lastScaledCameraIndex) ? CameraRenderTargetType.NonScaled : CameraRenderTargetType.Scaled);
		if (cameraIndex == cameraStackData.lastCameraIndex)
		{
			cameraData.CameraResolveTargetBufferType = CameraResolveTargetType.Backbuffer;
			cameraData.CameraResolveRequired = true;
		}
		else if (!flag2 || cameraIndex >= cameraStackData.lastScaledCameraIndex)
		{
			cameraData.CameraResolveTargetBufferType = CameraResolveTargetType.NonScaled;
			cameraData.CameraResolveRequired = cameraData.CameraRenderTargetBufferType == CameraRenderTargetType.Scaled;
		}
		else
		{
			cameraData.CameraResolveTargetBufferType = CameraResolveTargetType.None;
			cameraData.CameraResolveRequired = false;
		}
		Vector2 viewportSize = ((cameraData.CameraRenderTargetBufferType == CameraRenderTargetType.Scaled) ? cameraData.ScaledCameraTargetViewportSize : cameraData.NonScaledCameraTargetViewportSize);
		cameraData.CameraTargetDescriptor = CreateRenderTextureDescriptor(in camera, in viewportSize, cameraData.IsHdrEnabled, cameraData.HDRColorBufferPrecision, needsAlphaChannel: false);
		cameraData.CameraBuffer = WaaaghCameraBuffers.EnsureCamera(ref cameraData);
		cameraData.CameraBuffer.Update();
		cameraData.SetViewProjectionAndJitterMatrix(camera.worldToCameraMatrix, projectionMatrix, cameraData.CameraBuffer.JitterMatrix);
	}

	private static ScriptableRenderer GetRendererForCamera(Camera camera, WaaaghAdditionalCameraData additionalCameraData)
	{
		if (!(additionalCameraData != null))
		{
			return Asset.ScriptableRenderer;
		}
		return additionalCameraData.ScriptableRenderer;
	}

	private static float GetTemporalAntialiasingSharpness()
	{
		return VolumeManager.instance.stack.GetComponent<TaaSharpness>().GetSharpness();
	}

	private static bool IsScreenSpaceReflectionsEnabled()
	{
		return VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>().IsActive();
	}

	private static bool IsDepthPyramidNeed()
	{
		return VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>().IsActive();
	}

	private static bool IsStochasticSSR()
	{
		ScreenSpaceReflections component = VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>();
		if (component.IsActive())
		{
			return component.StochasticSSR.value;
		}
		return false;
	}

	private static bool CheckPostProcessForDepth(in CameraData cameraData)
	{
		if (!cameraData.PostProcessEnabled)
		{
			return false;
		}
		if (cameraData.Antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing)
		{
			return true;
		}
		return false;
	}

	private static RenderTextureDescriptor CreateRenderTextureDescriptor(in Camera camera, in Vector2 viewportSize, bool hdrEnabled, HDRColorBufferPrecision hdrColorBufferPrecision, bool needsAlphaChannel)
	{
		RenderTextureDescriptor result;
		if (camera.targetTexture == null)
		{
			result = new RenderTextureDescriptor((int)viewportSize.x, (int)viewportSize.y);
			result.graphicsFormat = MakeRenderTextureGraphicsFormat(hdrEnabled, hdrColorBufferPrecision, needsAlphaChannel);
			result.depthBufferBits = 32;
			result.msaaSamples = 1;
			result.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
		}
		else
		{
			result = camera.targetTexture.descriptor;
			result.width = (int)viewportSize.x;
			result.height = (int)viewportSize.y;
			if (camera.cameraType == CameraType.SceneView && !hdrEnabled)
			{
				result.graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
			}
		}
		result.width = Mathf.Max(1, result.width);
		result.height = Mathf.Max(1, result.height);
		result.enableRandomWrite = false;
		result.bindMS = false;
		result.useDynamicScale = camera.allowDynamicResolution;
		return result;
	}

	internal static GraphicsFormat MakeRenderTextureGraphicsFormat(bool isHdrEnabled, HDRColorBufferPrecision hdrColorBufferPrecision, bool needsAlpha)
	{
		if (isHdrEnabled)
		{
			if (!needsAlpha && hdrColorBufferPrecision != HDRColorBufferPrecision._64Bits && RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Blend))
			{
				return GraphicsFormat.B10G11R11_UFloatPack32;
			}
			if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Blend))
			{
				return GraphicsFormat.R16G16B16A16_SFloat;
			}
			return SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
		}
		return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
	}

	private void InitializeRenderingData(WaaaghPipelineAsset settings, ref CameraData cameraData, ref CullingResults cullResults, out RenderingData renderingData)
	{
		renderingData.CameraData = cameraData;
		renderingData.RenderGraph = m_RenderGraph;
		renderingData.CullingResults = cullResults;
		InitializeLightData(settings, ref cullResults, out renderingData.LightData);
		InitializeTimeData(out renderingData.TimeData);
		InitializeShadowData(out renderingData.ShadowData);
		InitializePostProcessingData(settings, out renderingData.PostProcessingData);
		renderingData.SupportsDynamicBatching = settings.SupportsDynamicBatching;
		renderingData.PerObjectData = GetPerObjectData(ref cameraData);
		renderingData.IrsHasOpaques = cameraData.IsIndirectRenderingEnabled && IndirectRenderingSystem.Instance.HasOpaqueObjects();
		renderingData.IrsHasTransparents = cameraData.IsIndirectRenderingEnabled && IndirectRenderingSystem.Instance.HasTransparentObjects();
		renderingData.IrsHasOpaqueDistortions = cameraData.IsIndirectRenderingEnabled && IndirectRenderingSystem.Instance.HasOpaqueDistortion();
		renderingData.CaptureActions = CameraCaptureBridge.GetCaptureActions(cameraData.Camera);
		renderingData.lightCookieManager = m_LightCookieManager;
	}

	private void InitializePostProcessingData(WaaaghPipelineAsset settings, out PostProcessingData postProcessingData)
	{
		postProcessingData.GradingMode = (settings.SupportsHDR ? settings.PostProcessSettings.ColorGradingMode : ColorGradingMode.LowDynamicRange);
		postProcessingData.LutSize = settings.PostProcessSettings.ColorGradingLutSize;
	}

	private void InitializeLightData(WaaaghPipelineAsset settings, ref CullingResults cullResults, out LightData lightData)
	{
		lightData.VisibleLights = cullResults.visibleLights;
	}

	private void InitializeShadowData(out ShadowData shadowData)
	{
		shadowData = default(ShadowData);
		shadowData.StaticShadowsCacheEnabled = Asset.ShadowSettings.StaticShadowsCacheEnabled;
		shadowData.ShadowManager = m_ShadowManager;
		shadowData.AtlasSize = Asset.ShadowSettings.AtlasSize;
		shadowData.CacheAtlasSize = Asset.ShadowSettings.CacheAtlasSize;
		shadowData.SpotLightResolution = Asset.ShadowSettings.SpotLightResolution;
		shadowData.DirectionalLightCascades = Asset.ShadowSettings.DirectionalLightCascades;
		shadowData.DirectionalLightCascadeResolution = Asset.ShadowSettings.DirectionalLightCascadeResolution;
		shadowData.PointLightResolution = Asset.ShadowSettings.PointLightResolution;
		shadowData.ShadowNearPlane = Asset.ShadowSettings.ShadowNearPlane;
		shadowData.ShadowQuality = Asset.ShadowSettings.ShadowQuality;
		shadowData.DepthBias = Asset.ShadowSettings.DepthBias;
		shadowData.NormalBias = Asset.ShadowSettings.NormalBias;
		shadowData.ReceiverNormalBias = Asset.ShadowSettings.ReceiverNormalBias;
		shadowData.ShadowUpdateDistances = Asset.ShadowSettings.ShadowUpdateDistances;
	}

	private void InitializeTimeData(out TimeData timeData)
	{
		timeData.Time = Time.time % 14400f;
		timeData.DeltaTime = Time.deltaTime;
		timeData.SmoothDeltaTime = Time.smoothDeltaTime;
		timeData.FrameId = FrameId.FrameCount;
		timeData.UnscaledTime = Time.unscaledTime;
	}

	private static PerObjectData GetPerObjectData(ref CameraData cameraData)
	{
		return PerObjectData.LightProbe | PerObjectData.ReflectionProbes | PerObjectData.Lightmaps;
	}

	private void SortCameras(List<Camera> cameras)
	{
		if (cameras.Count > 1)
		{
			cameras.Sort(m_CameraComparison);
		}
	}

	public static bool IsGameCamera(Camera camera)
	{
		if (camera == null)
		{
			throw new ArgumentNullException("camera");
		}
		if (camera.cameraType != CameraType.Game)
		{
			return camera.cameraType == CameraType.VR;
		}
		return true;
	}

	private static void SetSupportedRenderingFeatures()
	{
	}

	private void LightmappingDelegate(Light[] requests, NativeArray<LightDataGI> lightsOutput)
	{
		LightDataGI value = default(LightDataGI);
		if (!SupportedRenderingFeatures.active.enlighten || (SupportedRenderingFeatures.active.lightmapBakeTypes | LightmapBakeType.Realtime) == (LightmapBakeType)0)
		{
			for (int i = 0; i < requests.Length; i++)
			{
				Light light = requests[i];
				value.InitNoBake(light.GetInstanceID());
				lightsOutput[i] = value;
			}
			return;
		}
		for (int j = 0; j < requests.Length; j++)
		{
			Light light2 = requests[j];
			switch (light2.type)
			{
			case UnityEngine.LightType.Directional:
			{
				DirectionalLight dir = default(DirectionalLight);
				LightmapperUtils.Extract(light2, ref dir);
				value.Init(ref dir);
				break;
			}
			case UnityEngine.LightType.Point:
			{
				PointLight point = default(PointLight);
				LightmapperUtils.Extract(light2, ref point);
				value.Init(ref point);
				break;
			}
			case UnityEngine.LightType.Spot:
			{
				SpotLight spot = default(SpotLight);
				LightmapperUtils.Extract(light2, ref spot);
				spot.innerConeAngle = light2.innerSpotAngle * (MathF.PI / 180f);
				spot.angularFalloff = AngularFalloffType.AnalyticAndInnerAngle;
				value.Init(ref spot);
				break;
			}
			case UnityEngine.LightType.Area:
				value.InitNoBake(light2.GetInstanceID());
				break;
			case UnityEngine.LightType.Disc:
				value.InitNoBake(light2.GetInstanceID());
				break;
			default:
				value.InitNoBake(light2.GetInstanceID());
				break;
			}
			value.falloff = FalloffType.InverseSquared;
			lightsOutput[j] = value;
		}
	}

	private static ImageUpscalingFilter ResolveUpscalingFilterSelection(Vector2 imageSize, float renderScale, UpscalingFilterSelection selection)
	{
		ImageUpscalingFilter result = ImageUpscalingFilter.Linear;
		if (selection == UpscalingFilterSelection.FSR && !FSRUtils.IsSupported())
		{
			selection = UpscalingFilterSelection.Auto;
		}
		switch (selection)
		{
		case UpscalingFilterSelection.Auto:
		{
			float num = 1f / renderScale;
			if (Mathf.Approximately(num - Mathf.Floor(num), 0f))
			{
				float num2 = imageSize.x / num;
				float num3 = imageSize.y / num;
				if (Mathf.Approximately(num2 - Mathf.Floor(num2), 0f) && Mathf.Approximately(num3 - Mathf.Floor(num3), 0f))
				{
					result = ImageUpscalingFilter.Point;
				}
			}
			break;
		}
		case UpscalingFilterSelection.Point:
			result = ImageUpscalingFilter.Point;
			break;
		case UpscalingFilterSelection.FSR:
			result = ImageUpscalingFilter.FSR;
			break;
		}
		return result;
	}
}
