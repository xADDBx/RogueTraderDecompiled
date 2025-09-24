using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Core.ProfilingCounters;
using Owlcat.Runtime.Visual.Effects;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using Owlcat.Runtime.Visual.RenderPipeline.PostProcess;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public class OwlcatRenderPipeline : UnityEngine.Rendering.RenderPipeline
{
	private const float kHexRatio = 1.7320508f;

	private static Dictionary<Camera, ProfilingSampler> s_CameraSamplers = new Dictionary<Camera, ProfilingSampler>();

	private CameraChain m_CameraChain = new CameraChain();

	private Scene m_ActiveScene;

	private static HashSet<string> s_DisabledRendererFeatures = new HashSet<string>();

	public static Action<Camera> VolumeManagerUpdated;

	public static OwlcatRenderPipelineAsset Asset => GraphicsSettings.defaultRenderPipeline as OwlcatRenderPipelineAsset;

	public OwlcatRenderPipeline(OwlcatRenderPipelineAsset asset)
	{
		SetSupportedRenderingFeatures();
		QualitySettings.antiAliasing = 1;
		Shader.globalRenderPipeline = "OwlcatPipeline";
		Lightmapping.SetDelegate(LightmappingDelegate);
		RenderingUtils.ClearSystemInfoCache();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		Shader.globalRenderPipeline = "";
		SupportedRenderingFeatures.active = new SupportedRenderingFeatures();
		Lightmapping.ResetDelegate();
		if (Asset != null && Asset.ScriptableRenderer != null)
		{
			Asset.ScriptableRenderer.Dispose();
		}
		RenderingUtils.ClearSystemInfoCache();
	}

	private static void SetSupportedRenderingFeatures()
	{
	}

	protected override void Render(ScriptableRenderContext context, Camera[] cameras)
	{
		using (Counters.Render?.Measure())
		{
			FrameId.Update();
			UpdateRealtimeGI();
			UnityEngine.Rendering.RenderPipeline.BeginFrameRendering(context, cameras);
			GraphicsSettings.lightsUseLinearIntensity = QualitySettings.activeColorSpace == ColorSpace.Linear;
			GraphicsSettings.useScriptableRenderPipelineBatching = Asset.UseSRPBatcher;
			SetupPerFrameShaderConstants();
			m_CameraChain.Update(cameras);
			foreach (CameraChainDescriptor item in m_CameraChain.EnumerateCameras())
			{
				UnityEngine.Rendering.RenderPipeline.BeginCameraRendering(context, item.Camera);
				RenderSingleCamera(context, item);
				UnityEngine.Rendering.RenderPipeline.EndCameraRendering(context, item.Camera);
			}
			UnityEngine.Rendering.RenderPipeline.EndFrameRendering(context, cameras);
		}
	}

	private static void UpdateVolumeFramework(Camera camera, OwlcatAdditionalCameraData additionalCameraData)
	{
		LayerMask layerMask = 1;
		Transform transform = camera.transform;
		if (additionalCameraData != null)
		{
			layerMask = additionalCameraData.VolumeLayerMask;
			transform = ((additionalCameraData.VolumeTrigger != null) ? additionalCameraData.VolumeTrigger : transform);
		}
		else if (camera.cameraType == CameraType.SceneView)
		{
			Camera main = Camera.main;
			OwlcatAdditionalCameraData component = null;
			if (main != null && main.TryGetComponent<OwlcatAdditionalCameraData>(out component))
			{
				layerMask = component.VolumeLayerMask;
			}
			transform = ((component != null && component.VolumeTrigger != null) ? component.VolumeTrigger : transform);
		}
		VolumeManager.instance.Update(transform, layerMask);
		OnVolumeManagerUpdate(camera);
	}

	private void UpdateRealtimeGI()
	{
		Scene activeScene = SceneManager.GetActiveScene();
		if (!(activeScene != m_ActiveScene))
		{
			return;
		}
		m_ActiveScene = activeScene;
		IEnumerable<Light> enumerable = from l in UnityEngine.Object.FindObjectsOfType<Light>()
			where l.type == UnityEngine.LightType.Point
			select l;
		Dictionary<Light, bool> dictionary = new Dictionary<Light, bool>();
		foreach (Light item in enumerable)
		{
			dictionary[item] = item.enabled;
		}
		foreach (Light item2 in enumerable)
		{
			item2.enabled = false;
		}
		foreach (Light item3 in enumerable)
		{
			item3.enabled = dictionary[item3];
		}
	}

	public static void RenderSingleCamera(ScriptableRenderContext context, CameraChainDescriptor cameraDesc)
	{
		if (!cameraDesc.Camera.TryGetCullingParameters(stereoAware: false, out var cullingParameters))
		{
			return;
		}
		OwlcatRenderPipelineAsset asset = Asset;
		OwlcatAdditionalCameraData component = null;
		if (cameraDesc.Camera.cameraType == CameraType.Game || cameraDesc.Camera.cameraType == CameraType.VR)
		{
			cameraDesc.Camera.gameObject.TryGetComponent<OwlcatAdditionalCameraData>(out component);
		}
		UpdateVolumeFramework(cameraDesc.Camera, component);
		InitializeCameraData(asset, cameraDesc, component, out var cameraData);
		if (cameraData.IsVfxEnabled)
		{
			VFXManager.PrepareCamera(cameraDesc.Camera);
		}
		SetupPerCameraShaderConstants(ref cameraData);
		ScriptableRenderer scriptableRenderer = asset.ScriptableRenderer;
		if (scriptableRenderer == null)
		{
			Debug.LogWarning($"Trying to render {cameraDesc.Camera.name} with an invalid renderer. Camera rendering will be skipped.");
			return;
		}
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, GetProfilingSampler(cameraDesc.Camera)))
		{
			scriptableRenderer.SetupCullingParameters(ref cullingParameters, ref cameraData);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			ProceduralMesh.UpdateCameraDependencyAllBegin(cameraDesc.Camera);
			ProceduralMesh.UpdateCameraDependencyAllEnd();
			CullingResults cullResults = context.Cull(ref cullingParameters);
			InitializeRenderingData(asset, ref cameraData, ref cullResults, out var renderingData);
			scriptableRenderer.Setup(context, ref renderingData);
			scriptableRenderer.Execute(context, ref renderingData);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
		context.Submit();
	}

	private static void InitializeRenderingData(OwlcatRenderPipelineAsset settings, ref CameraData cameraData, ref CullingResults cullResults, out RenderingData renderingData)
	{
		renderingData.CameraData = cameraData;
		renderingData.CullResults = cullResults;
		renderingData.SupportsDynamicBatching = settings.SupportsDynamicBatching;
		renderingData.PerObjectData = GetPerObjectData(ref cameraData);
		renderingData.ColorSpace = QualitySettings.activeColorSpace;
		renderingData.RenderPath = (settings.ScriptableRendererData as ClusteredRendererData)?.RenderPath ?? RenderPath.Forward;
		InitializeLightData(out renderingData.LightData, ref cullResults);
		InitializeShadowData(out renderingData.ShadowData);
		InitializePostProcessingData(settings, out renderingData.PostProcessingData);
	}

	private static void InitializePostProcessingData(OwlcatRenderPipelineAsset settings, out PostProcessingData postProcessingData)
	{
		postProcessingData.GradingMode = (settings.SupportsHDR ? settings.PostProcessSettings.ColorGradingMode : ColorGradingMode.LowDynamicRange);
		postProcessingData.LutSize = settings.PostProcessSettings.ColorGradingLutSize;
	}

	private static void InitializeShadowData(out ShadowingData shadowData)
	{
		shadowData = default(ShadowingData);
		shadowData.ScreenSpaceShadowsTextureCount = 0;
		shadowData.AtlasSize = Asset.ShadowSettings.AtlasSize;
		shadowData.SpotLightResolution = Asset.ShadowSettings.SpotLightResolution;
		shadowData.DirectionalLightCascades = Asset.ShadowSettings.DirectionalLightCascades;
		shadowData.DirectionalLightCascadeResolution = Asset.ShadowSettings.DirectionalLightCascadeResolution;
		shadowData.PointLightResolution = Asset.ShadowSettings.PointLightResolution;
		shadowData.ShadowNearPlane = Asset.ShadowSettings.ShadowNearPlane;
		shadowData.ShadowQuality = Asset.ShadowSettings.ShadowQuality;
		shadowData.DepthBias = Asset.ShadowSettings.DepthBias;
		shadowData.NormalBias = Asset.ShadowSettings.NormalBias;
	}

	private static void InitializeLightData(out LightingData lightData, ref CullingResults cullResults)
	{
		lightData.VisibleLights = cullResults.visibleLights;
		lightData.MainLightIndex = -1;
	}

	private static PerObjectData GetPerObjectData(ref CameraData cameraData)
	{
		if (cameraData.IsLightingEnabled)
		{
			return PerObjectData.LightProbe | PerObjectData.LightProbeProxyVolume | PerObjectData.Lightmaps;
		}
		return PerObjectData.None;
	}

	private static void SetupPerCameraShaderConstants(ref CameraData cameraData)
	{
		Camera camera = cameraData.Camera;
		float num = cameraData.Camera.pixelWidth;
		float num2 = cameraData.Camera.pixelHeight;
		Shader.SetGlobalVector(CameraBuffer._ScreenSize, new Vector4(num, num2, 1f / num, 1f / num2));
		Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true);
		Matrix4x4 inverse = gPUProjectionMatrix.inverse;
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 value = Matrix4x4.Inverse(gPUProjectionMatrix * worldToCameraMatrix);
		Shader.SetGlobalMatrix(CameraBuffer._InvCameraViewProj, value);
		Shader.SetGlobalMatrix(CameraBuffer._InvProjMatrix, inverse);
		Shader.SetGlobalMatrix(CameraBuffer.unity_MatrixInvP, inverse);
		Matrix4x4 inverse2 = worldToCameraMatrix.inverse;
		Vector3 vector = inverse2.GetColumn(1);
		Vector3 vector2 = inverse2.GetColumn(0);
		Vector3 vector3 = -inverse2.GetColumn(2);
		Shader.SetGlobalVector(CameraBuffer._CamBasisUp, vector);
		Shader.SetGlobalVector(CameraBuffer._CamBasisSide, vector2);
		Shader.SetGlobalVector(CameraBuffer._CamBasisFront, vector3);
	}

	private static void InitializeCameraData(OwlcatRenderPipelineAsset settings, CameraChainDescriptor cameraDesc, OwlcatAdditionalCameraData additionalCameraData, out CameraData cameraData)
	{
		Camera camera = (cameraData.Camera = cameraDesc.Camera);
		cameraData.IsFirstInChain = cameraDesc.IsFirst;
		cameraData.IsLastInChain = cameraDesc.IsLast;
		cameraData.IsSceneViewCamera = camera.cameraType == CameraType.SceneView;
		cameraData.IsSceneViewInPrefabEditMode = false;
		if (cameraData.IsSceneViewCamera || cameraData.IsSceneViewInPrefabEditMode)
		{
			cameraData.IsHdrEnabled = settings.SupportsHDR;
		}
		else
		{
			cameraData.IsHdrEnabled = camera.allowHDR && settings.SupportsHDR;
		}
		bool flag = Asset.ShadowSettings.ShadowQuality != ShadowQuality.Disable;
		cameraData.ShadowDistance = (flag ? Asset.ShadowSettings.ShadowDistance : 0f);
		cameraData.IsStereoEnabled = false;
		Rect rect = camera.rect;
		cameraData.IsDefaultViewport = !(Math.Abs(rect.x) > 0f) && !(Math.Abs(rect.y) > 0f) && !(Math.Abs(rect.width) < 1f) && !(Math.Abs(rect.height) < 1f);
		float renderScale = settings.RenderScale;
		cameraData.RenderScale = ((Mathf.Abs(1f - renderScale) < 0.05f) ? 1f : renderScale);
		cameraData.RenderScale = ((camera.cameraType == CameraType.Game) ? cameraData.RenderScale : 1f);
		SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
		SortingCriteria sortingCriteria2 = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
		bool hasHiddenSurfaceRemovalOnGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU;
		bool flag2 = (camera.opaqueSortMode == OpaqueSortMode.Default && hasHiddenSurfaceRemovalOnGPU) || camera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;
		cameraData.DefaultOpaqueSortFlags = (flag2 ? sortingCriteria2 : sortingCriteria);
		cameraData.CameraTargetDescriptor = CreateRenderTextureDescriptor(camera, cameraData.RenderScale, cameraData.IsStereoEnabled, cameraData.IsHdrEnabled);
		cameraData.IsDistortionEnabled = settings.SupportsDistortion;
		cameraData.IsLightingEnabled = true;
		cameraData.IsDecalsEnabled = settings.DecalSettings.Enabled;
		cameraData.IsIndirectRenderingEnabled = true;
		cameraData.IsFogEnabled = UnityEngine.RenderSettings.fog;
		cameraData.IsVfxEnabled = true;
		cameraData.IsPostProcessEnabled = CoreUtils.ArePostProcessesEnabled(camera) && camera.cameraType != CameraType.Reflection && camera.cameraType != CameraType.Preview && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2;
		s_DisabledRendererFeatures.Clear();
		cameraData.DisabledRendererFeatures = s_DisabledRendererFeatures;
		if (additionalCameraData != null)
		{
			cameraData.VolumeLayerMask = additionalCameraData.VolumeLayerMask;
			cameraData.VolumeTrigger = ((additionalCameraData.VolumeTrigger == null) ? camera.transform : additionalCameraData.VolumeTrigger);
			cameraData.IsPostProcessEnabled &= additionalCameraData.RenderPostProcessing;
			cameraData.IsStopNaNEnabled = cameraData.IsPostProcessEnabled;
			cameraData.IsDitheringEnabled = cameraData.IsPostProcessEnabled && additionalCameraData.Dithering;
			cameraData.Antialiasing = (cameraData.IsPostProcessEnabled ? additionalCameraData.Antialiasing : AntialiasingMode.None);
			cameraData.AntialiasingQuality = additionalCameraData.AntialiasingQuality;
			cameraData.IsLightingEnabled &= additionalCameraData.AllowLighting;
			cameraData.IsDecalsEnabled &= additionalCameraData.AllowDecals;
			cameraData.IsDistortionEnabled &= additionalCameraData.AllowDistortion;
			cameraData.IsIndirectRenderingEnabled &= additionalCameraData.AllowIndirectRendering;
			cameraData.IsFogEnabled &= additionalCameraData.AllowFog;
			cameraData.IsVfxEnabled &= additionalCameraData.AllowVfxPreparation;
			if (additionalCameraData.DepthTexture == null)
			{
				cameraData.DepthTexture = new RenderTargetIdentifier(BuiltinRenderTextureType.None);
			}
			else
			{
				cameraData.DepthTexture = new RenderTargetIdentifier(additionalCameraData.DepthTexture);
			}
			additionalCameraData.GetDisabledFeatures(s_DisabledRendererFeatures);
		}
		else
		{
			cameraData.VolumeLayerMask = 1;
			cameraData.VolumeTrigger = null;
			cameraData.IsStopNaNEnabled = false;
			cameraData.IsDitheringEnabled = false;
			cameraData.Antialiasing = AntialiasingMode.None;
			cameraData.AntialiasingQuality = AntialiasingQuality.High;
			cameraData.DepthTexture = new RenderTargetIdentifier(BuiltinRenderTextureType.None);
		}
		if (cameraData.IsPostProcessEnabled)
		{
			ScreenSpaceReflections component = VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>();
			cameraData.IsScreenSpaceReflectionsEnabled = component.IsActive();
			cameraData.IsNeedDepthPyramid = cameraData.IsScreenSpaceReflectionsEnabled && component.TracingMethod.value == TracingMethod.HiZ;
		}
		else
		{
			cameraData.IsScreenSpaceReflectionsEnabled = false;
			cameraData.IsNeedDepthPyramid = false;
		}
	}

	private static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera, float renderScale, bool isStereoEnabled, bool isHdrEnabled, int msaaSamples = 1)
	{
		RenderTextureFormat renderTextureFormat = RenderTextureFormat.Default;
		RenderTextureDescriptor result = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight);
		result.width = (int)((float)result.width * renderScale);
		result.height = (int)((float)result.height * renderScale);
		RenderTextureFormat renderTextureFormat2 = ((Application.isMobilePlatform && RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.RGB111110Float)) ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.DefaultHDR);
		if (Application.platform == RuntimePlatform.PS4)
		{
			renderTextureFormat2 = RenderTextureFormat.ARGBHalf;
		}
		result.colorFormat = (isHdrEnabled ? renderTextureFormat2 : renderTextureFormat);
		result.depthBufferBits = 24;
		result.enableRandomWrite = false;
		result.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
		result.msaaSamples = msaaSamples;
		result.bindMS = false;
		result.useDynamicScale = camera.allowDynamicResolution;
		return result;
	}

	private void SetupPerFrameShaderConstants()
	{
		SphericalHarmonicsL2 ambientProbe = UnityEngine.RenderSettings.ambientProbe;
		Color color = CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * UnityEngine.RenderSettings.reflectionIntensity);
		Shader.SetGlobalVector(PerFrameBuffer._GlossyEnvironmentColor, color);
		Shader.SetGlobalVector(PerFrameBuffer._HexRatio, new Vector4(1f, 1.7320508f));
		if (Asset.TerrainSettings.TriplanarEnabled)
		{
			Shader.EnableKeyword(ShaderKeywordStrings._TRIPLANAR);
		}
		else
		{
			Shader.DisableKeyword(ShaderKeywordStrings._TRIPLANAR);
		}
	}

	private void LightmappingDelegate(Light[] requests, NativeArray<LightDataGI> lightsOutput)
	{
		LightDataGI value = default(LightDataGI);
		for (int i = 0; i < requests.Length; i++)
		{
			Light light = requests[i];
			switch (light.type)
			{
			case UnityEngine.LightType.Directional:
			{
				DirectionalLight dir = default(DirectionalLight);
				LightmapperUtils.Extract(light, ref dir);
				value.Init(ref dir);
				break;
			}
			case UnityEngine.LightType.Point:
			{
				PointLight point = default(PointLight);
				LightmapperUtils.Extract(light, ref point);
				value.Init(ref point);
				break;
			}
			case UnityEngine.LightType.Spot:
			{
				SpotLight spot = default(SpotLight);
				LightmapperUtils.Extract(light, ref spot);
				value.Init(ref spot);
				break;
			}
			case UnityEngine.LightType.Area:
			{
				RectangleLight rect = default(RectangleLight);
				LightmapperUtils.Extract(light, ref rect);
				value.Init(ref rect);
				break;
			}
			default:
				value.InitNoBake(light.GetInstanceID());
				break;
			}
			value.falloff = FalloffType.InverseSquared;
			lightsOutput[i] = value;
		}
	}

	internal static void OnVolumeManagerUpdate(Camera camera)
	{
		VolumeManagerUpdated?.Invoke(camera);
	}

	private static ProfilingSampler GetProfilingSampler(Camera camera)
	{
		if (s_CameraSamplers.TryGetValue(camera, out var value))
		{
			return value;
		}
		List<Camera> list = new List<Camera>();
		foreach (Camera key in s_CameraSamplers.Keys)
		{
			if (key == null)
			{
				list.Add(key);
			}
		}
		foreach (Camera item in list)
		{
			s_CameraSamplers.Remove(item);
		}
		value = new ProfilingSampler(camera.name);
		s_CameraSamplers[camera] = value;
		return value;
	}
}
