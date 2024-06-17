using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using Owlcat.Runtime.Visual.RenderPipeline.PostProcess;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

internal class PostProcessPass : ScriptableRenderPass
{
	private class MaterialLibrary
	{
		public readonly Material stopNaN;

		public readonly Material subpixelMorphologicalAntialiasing;

		public readonly Material gaussianDepthOfField;

		public readonly Material bokehDepthOfField;

		public readonly Material cameraMotionBlur;

		public readonly Material paniniProjection;

		public readonly Material bloom;

		public readonly Material bloomEnhanced;

		public readonly Material radialBlur;

		public readonly Material colorOverlay;

		public readonly Material uber;

		public readonly Material finalPass;

		public readonly Material daltonization;

		public MaterialLibrary(PostProcessData data)
		{
			stopNaN = Load(data.Shaders.StopNanPS);
			subpixelMorphologicalAntialiasing = Load(data.Shaders.SubpixelMorphologicalAntialiasingPS);
			gaussianDepthOfField = Load(data.Shaders.GaussianDepthOfFieldPS);
			bokehDepthOfField = Load(data.Shaders.BokehDepthOfFieldPS);
			cameraMotionBlur = Load(data.Shaders.CameraMotionBlurPS);
			paniniProjection = Load(data.Shaders.PaniniProjectionPS);
			bloom = Load(data.Shaders.BloomPS);
			bloomEnhanced = Load(data.Shaders.BloomEnhancedPS);
			radialBlur = Load(data.Shaders.RadialBlurPS);
			colorOverlay = Load(data.Shaders.MaskedColorTransformPS);
			uber = Load(data.Shaders.UberPostPS);
			finalPass = Load(data.Shaders.FinalPostPassPS);
			daltonization = Load(data.Shaders.DaltonizationPS);
		}

		private Material Load(Shader shader)
		{
			if (shader == null)
			{
				Debug.LogErrorFormat("Missing shader. " + GetType().DeclaringType.Name + " render pass will not execute. Check for missing reference in the renderer resources.");
				return null;
			}
			return CoreUtils.CreateEngineMaterial(shader);
		}
	}

	private static class ShaderConstants
	{
		public static readonly int _TempTarget = Shader.PropertyToID("_TempTarget");

		public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");

		public static readonly int _StencilMask = Shader.PropertyToID("_StencilMask");

		public static readonly int _FullCoCTexture = Shader.PropertyToID("_FullCoCTexture");

		public static readonly int _HalfCoCTexture = Shader.PropertyToID("_HalfCoCTexture");

		public static readonly int _DofTexture = Shader.PropertyToID("_DofTexture");

		public static readonly int _CoCParams = Shader.PropertyToID("_CoCParams");

		public static readonly int _BokehKernel = Shader.PropertyToID("_BokehKernel");

		public static readonly int _PongTexture = Shader.PropertyToID("_PongTexture");

		public static readonly int _PingTexture = Shader.PropertyToID("_PingTexture");

		public static readonly int _Metrics = Shader.PropertyToID("_Metrics");

		public static readonly int _AreaTexture = Shader.PropertyToID("_AreaTexture");

		public static readonly int _SearchTexture = Shader.PropertyToID("_SearchTexture");

		public static readonly int _EdgeTexture = Shader.PropertyToID("_EdgeTexture");

		public static readonly int _BlendTexture = Shader.PropertyToID("_BlendTexture");

		public static readonly int _ColorTexture = Shader.PropertyToID("_ColorTexture");

		public static readonly int _Params = Shader.PropertyToID("_Params");

		public static readonly int _Params1 = Shader.PropertyToID("_Params1");

		public static readonly int _MainTexLowMip = Shader.PropertyToID("_MainTexLowMip");

		public static readonly int _Bloom_Params = Shader.PropertyToID("_Bloom_Params");

		public static readonly int _Bloom_RGBM = Shader.PropertyToID("_Bloom_RGBM");

		public static readonly int _Bloom_Texture = Shader.PropertyToID("_Bloom_Texture");

		public static readonly int _LensDirt_Texture = Shader.PropertyToID("_LensDirt_Texture");

		public static readonly int _LensDirt_Params = Shader.PropertyToID("_LensDirt_Params");

		public static readonly int _LensDirt_Intensity = Shader.PropertyToID("_LensDirt_Intensity");

		public static readonly int _Distortion_Params1 = Shader.PropertyToID("_Distortion_Params1");

		public static readonly int _Distortion_Params2 = Shader.PropertyToID("_Distortion_Params2");

		public static readonly int _Chroma_Params = Shader.PropertyToID("_Chroma_Params");

		public static readonly int _Vignette_Params1 = Shader.PropertyToID("_Vignette_Params1");

		public static readonly int _Vignette_Params2 = Shader.PropertyToID("_Vignette_Params2");

		public static readonly int _Lut_Params = Shader.PropertyToID("_Lut_Params");

		public static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");

		public static readonly int _InternalLut = Shader.PropertyToID("_InternalLut");

		public static readonly int _UserLut = Shader.PropertyToID("_UserLut");

		public static readonly int _Curve = Shader.PropertyToID("_Curve");

		public static readonly int _BaseTex = Shader.PropertyToID("_BaseTex");

		public static readonly int _MainTex = Shader.PropertyToID("_MainTex");

		public static readonly int _Color = Shader.PropertyToID("_Color");

		public static int[] _BloomMipUp;

		public static int[] _BloomMipDown;
	}

	private const string kRenderPostProcessingTag = "Render PostProcessing Effects";

	private const string kRenderFinalPostProcessingTag = "Render Final PostProcessing Pass";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Render PostProcessing Effects");

	private ProfilingSampler m_FinalProfilingSampler = new ProfilingSampler("Render Final PostProcessing Pass");

	private const int kMaxPyramidSize = 16;

	private RenderTextureDescriptor m_Descriptor;

	private RenderTargetHandle m_Source;

	private RenderTargetHandle m_Destination;

	private RenderTargetHandle m_Depth;

	private RenderTargetHandle m_InternalLut;

	private MaterialLibrary m_Materials;

	private PostProcessData m_Data;

	private DepthOfField m_DepthOfField;

	private MotionBlur m_MotionBlur;

	private PaniniProjection m_PaniniProjection;

	private Bloom m_Bloom;

	private BloomEnhanced m_BloomEnhanced;

	private RadialBlur m_RadialBlur;

	private LensDistortion m_LensDistortion;

	private ChromaticAberration m_ChromaticAberration;

	private Vignette m_Vignette;

	private ColorLookup m_ColorLookup;

	private ColorAdjustments m_ColorAdjustments;

	private Tonemapping m_Tonemapping;

	private FilmGrain m_FilmGrain;

	private Daltonization m_Daltonization;

	private readonly GraphicsFormat m_DefaultHDRFormat;

	private bool m_UseRGBM;

	private readonly GraphicsFormat m_GaussianCoCFormat;

	private Matrix4x4 m_PrevViewProjM = Matrix4x4.identity;

	private bool m_ResetHistory;

	private int m_DitheringTextureIndex;

	private RenderTargetIdentifier[] m_MRT2;

	private Vector4[] m_BokehKernel;

	private int m_BokehHash;

	private bool m_IsStereo;

	private bool m_IsFinalPass;

	private bool m_HasFinalPass;

	public PostProcessPass(RenderPassEvent evt, PostProcessData data)
	{
		base.RenderPassEvent = evt;
		m_Data = data;
		m_Materials = new MaterialLibrary(data);
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Blend))
		{
			m_DefaultHDRFormat = GraphicsFormat.R16G16B16A16_SFloat;
			m_UseRGBM = false;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Blend))
		{
			m_DefaultHDRFormat = GraphicsFormat.B10G11R11_UFloatPack32;
			m_UseRGBM = false;
		}
		else
		{
			m_DefaultHDRFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
			m_UseRGBM = true;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_UNorm, FormatUsage.Blend))
		{
			m_GaussianCoCFormat = GraphicsFormat.R16_UNorm;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, FormatUsage.Blend))
		{
			m_GaussianCoCFormat = GraphicsFormat.R16_SFloat;
		}
		else
		{
			m_GaussianCoCFormat = GraphicsFormat.R8_UNorm;
		}
		ShaderConstants._BloomMipUp = new int[16];
		ShaderConstants._BloomMipDown = new int[16];
		for (int i = 0; i < 16; i++)
		{
			ShaderConstants._BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
			ShaderConstants._BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
		}
		m_MRT2 = new RenderTargetIdentifier[2];
		m_ResetHistory = true;
	}

	public void Setup(in RenderTextureDescriptor baseDescriptor, in RenderTargetHandle source, in RenderTargetHandle destination, in RenderTargetHandle depth, in RenderTargetHandle internalLut, bool hasFinalPass)
	{
		m_Descriptor = baseDescriptor;
		m_Source = source;
		m_Destination = destination;
		m_Depth = depth;
		m_InternalLut = internalLut;
		m_IsFinalPass = false;
		m_HasFinalPass = hasFinalPass;
	}

	public void SetupFinalPass(in RenderTargetHandle source, in RenderTargetHandle destination)
	{
		m_Source = source;
		m_Destination = destination;
		m_IsFinalPass = true;
		m_HasFinalPass = false;
	}

	private void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		if (!(m_Destination == RenderTargetHandle.CameraTarget) && !m_IsFinalPass)
		{
			RenderTextureDescriptor desc = cameraTextureDescriptor;
			desc.msaaSamples = 1;
			desc.depthBufferBits = 0;
			cmd.GetTemporaryRT(m_Destination.Id, desc, FilterMode.Point);
		}
	}

	public void ResetHistory()
	{
		m_ResetHistory = true;
	}

	public bool CanRunOnTile()
	{
		return false;
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		if (!m_IsFinalPass)
		{
			cmd.ReleaseTemporaryRT(m_Destination.Id);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_DepthOfField = stack.GetComponent<DepthOfField>();
		m_MotionBlur = stack.GetComponent<MotionBlur>();
		m_PaniniProjection = stack.GetComponent<PaniniProjection>();
		m_Bloom = stack.GetComponent<Bloom>();
		m_BloomEnhanced = stack.GetComponent<BloomEnhanced>();
		m_RadialBlur = stack.GetComponent<RadialBlur>();
		m_LensDistortion = stack.GetComponent<LensDistortion>();
		m_ChromaticAberration = stack.GetComponent<ChromaticAberration>();
		m_Vignette = stack.GetComponent<Vignette>();
		m_ColorLookup = stack.GetComponent<ColorLookup>();
		m_ColorAdjustments = stack.GetComponent<ColorAdjustments>();
		m_Tonemapping = stack.GetComponent<Tonemapping>();
		m_FilmGrain = stack.GetComponent<FilmGrain>();
		m_Daltonization = stack.GetComponent<Daltonization>();
		if (m_IsFinalPass)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			Configure(commandBuffer, m_Descriptor);
			RenderFinalPass(commandBuffer, ref renderingData);
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
		else if (!CanRunOnTile())
		{
			CommandBuffer commandBuffer2 = CommandBufferPool.Get();
			Configure(commandBuffer2, m_Descriptor);
			Render(commandBuffer2, ref renderingData);
			context.ExecuteCommandBuffer(commandBuffer2);
			CommandBufferPool.Release(commandBuffer2);
		}
		m_ResetHistory = false;
	}

	private RenderTextureDescriptor GetCompatibleDescriptor()
	{
		return GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
	}

	private RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format)
	{
		RenderTextureDescriptor descriptor = m_Descriptor;
		descriptor.depthBufferBits = 0;
		descriptor.msaaSamples = 1;
		descriptor.width = width;
		descriptor.height = height;
		descriptor.graphicsFormat = format;
		return descriptor;
	}

	private void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		m_IsStereo = renderingData.CameraData.IsStereoEnabled;
		int source = m_Source.Id;
		int destination = -1;
		using (new ProfilingScope(cmd, m_ProfilingSampler))
		{
			cmd.Blit(GetSource(), GetDestination(), m_Materials.stopNaN);
			Swap();
		}
		if (cameraData.Antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
		{
			using (new ProfilingScope(cmd, m_ProfilingSampler))
			{
				DoSubpixelMorphologicalAntialiasing(ref cameraData, cmd, GetSource(), GetDestination());
				Swap();
			}
		}
		if (m_DepthOfField.IsActive() && !cameraData.IsSceneViewCamera)
		{
			_ = m_DepthOfField.mode.value;
			_ = 1;
			using (new ProfilingScope(cmd, m_ProfilingSampler))
			{
				DoDepthOfField(cameraData.Camera, cmd, GetSource(), GetDestination());
				Swap();
			}
		}
		if (m_MotionBlur.IsActive() && !cameraData.IsSceneViewCamera)
		{
			using (new ProfilingScope(cmd, m_ProfilingSampler))
			{
				DoMotionBlur(cameraData.Camera, cmd, GetSource(), GetDestination());
				Swap();
			}
		}
		if (m_RadialBlur.IsActive() && !cameraData.IsSceneViewCamera)
		{
			using (new ProfilingScope(cmd, m_ProfilingSampler))
			{
				DoRadialBlur(cameraData.Camera, cmd, GetSource(), GetDestination());
				Swap();
			}
		}
		if (m_PaniniProjection.IsActive() && !cameraData.IsSceneViewCamera)
		{
			using (new ProfilingScope(cmd, m_ProfilingSampler))
			{
				DoPaniniProjection(cameraData.Camera, cmd, GetSource(), GetDestination());
				Swap();
			}
		}
		using (new ProfilingScope(cmd, m_ProfilingSampler))
		{
			m_Materials.uber.shaderKeywords = null;
			bool flag = m_Bloom.IsActive();
			if (flag)
			{
				using (new ProfilingScope(cmd, m_ProfilingSampler))
				{
					SetupBloom(cmd, GetSource(), m_Materials.uber);
				}
			}
			bool flag2 = m_BloomEnhanced.IsActive();
			if (flag2)
			{
				using (new ProfilingScope(cmd, m_ProfilingSampler))
				{
					SetupBloomEnhanced(cmd, GetSource(), m_Materials.uber);
				}
			}
			SetupLensDistortion(m_Materials.uber, cameraData.IsSceneViewCamera);
			SetupChromaticAberration(m_Materials.uber);
			SetupVignette(m_Materials.uber);
			SetupColorGrading(cmd, ref renderingData, m_Materials.uber);
			SetupGrain(cameraData.Camera, m_Materials.uber);
			SetupDithering(ref cameraData, m_Materials.uber);
			cmd.SetGlobalTexture("_BlitTex", GetSource());
			RenderTargetIdentifier renderTargetIdentifier = (HasEffectsAfterUber() ? ((RenderTargetIdentifier)GetDestination()) : m_Destination.Identifier());
			if (m_IsStereo)
			{
				Blit(cmd, GetSource(), renderTargetIdentifier, m_Materials.uber);
			}
			else
			{
				cmd.SetRenderTarget(renderTargetIdentifier);
				cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
				cmd.SetViewport(cameraData.Camera.pixelRect);
				cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, m_Materials.uber);
				cmd.SetViewProjectionMatrices(cameraData.Camera.worldToCameraMatrix, cameraData.Camera.projectionMatrix);
			}
			Swap();
			if (m_Daltonization.IsActive())
			{
				DoDaltonization(cmd, GetSource(), m_Destination.Identifier());
			}
			if (flag || flag2)
			{
				cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipUp[0]);
			}
			if (destination != -1)
			{
				cmd.ReleaseTemporaryRT(ShaderConstants._TempTarget);
			}
		}
		int GetDestination()
		{
			if (destination == -1)
			{
				cmd.GetTemporaryRT(ShaderConstants._TempTarget, GetCompatibleDescriptor(), FilterMode.Bilinear);
				destination = ShaderConstants._TempTarget;
			}
			return destination;
		}
		int GetSource()
		{
			return source;
		}
		void Swap()
		{
			CoreUtils.Swap(ref source, ref destination);
		}
	}

	private void DoDaltonization(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination)
	{
		cmd.SetGlobalTexture("_BlitTex", source);
		cmd.SetGlobalVector(ShaderConstants._Params, new Vector4(m_Daltonization.ProtanopiaFactor.value, m_Daltonization.DeuteranopiaFactor.value, m_Daltonization.TritanopiaFactor.value, m_Daltonization.Intensity.value));
		cmd.SetGlobalVector(ShaderConstants._Params1, new Vector4(m_Daltonization.BrightnessFactor.value, m_Daltonization.ContrastFactor.value));
		cmd.Blit(source, destination, m_Materials.daltonization, 0);
	}

	private bool HasEffectsAfterUber()
	{
		return m_Daltonization.IsActive();
	}

	private void DoRadialBlur(Camera camera, CommandBuffer cmd, int source, int destination)
	{
		Material radialBlur = m_Materials.radialBlur;
		radialBlur.SetVector("_RadialBlurCenter", m_RadialBlur.Center.value);
		radialBlur.SetFloat("_RadialBlurStrength", m_RadialBlur.Strength.value);
		radialBlur.SetFloat("_RadialBlurWidth", m_RadialBlur.Width.value);
		cmd.Blit(source, destination, radialBlur, 0);
	}

	private void DoSubpixelMorphologicalAntialiasing(ref CameraData cameraData, CommandBuffer cmd, int source, int destination)
	{
		Camera camera = cameraData.Camera;
		Material subpixelMorphologicalAntialiasing = m_Materials.subpixelMorphologicalAntialiasing;
		subpixelMorphologicalAntialiasing.SetVector(ShaderConstants._Metrics, new Vector4(1f / (float)m_Descriptor.width, 1f / (float)m_Descriptor.height, m_Descriptor.width, m_Descriptor.height));
		subpixelMorphologicalAntialiasing.SetTexture(ShaderConstants._AreaTexture, m_Data.Textures.SmaaAreaTex);
		subpixelMorphologicalAntialiasing.SetTexture(ShaderConstants._SearchTexture, m_Data.Textures.SmaaSearchTex);
		subpixelMorphologicalAntialiasing.SetInt(ShaderConstants._StencilRef, 64);
		subpixelMorphologicalAntialiasing.SetInt(ShaderConstants._StencilMask, 64);
		subpixelMorphologicalAntialiasing.shaderKeywords = null;
		switch (cameraData.AntialiasingQuality)
		{
		case AntialiasingQuality.Low:
			subpixelMorphologicalAntialiasing.EnableKeyword(ShaderKeywordStrings.SmaaLow);
			break;
		case AntialiasingQuality.Medium:
			subpixelMorphologicalAntialiasing.EnableKeyword(ShaderKeywordStrings.SmaaMedium);
			break;
		case AntialiasingQuality.High:
			subpixelMorphologicalAntialiasing.EnableKeyword(ShaderKeywordStrings.SmaaHigh);
			break;
		}
		cmd.GetTemporaryRT(ShaderConstants._EdgeTexture, m_Descriptor.width, m_Descriptor.height, 32, FilterMode.Point, GraphicsFormat.R8G8B8A8_UNorm);
		cmd.GetTemporaryRT(ShaderConstants._BlendTexture, m_Descriptor.width, m_Descriptor.height, 0, FilterMode.Point, GraphicsFormat.R8G8B8A8_UNorm);
		cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
		cmd.SetViewport(camera.pixelRect);
		cmd.SetRenderTarget(ShaderConstants._EdgeTexture);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
		cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, subpixelMorphologicalAntialiasing, 0, 0);
		cmd.SetRenderTarget(ShaderConstants._BlendTexture, ShaderConstants._EdgeTexture);
		cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, ShaderConstants._EdgeTexture);
		cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, subpixelMorphologicalAntialiasing, 0, 1);
		cmd.SetRenderTarget(destination);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
		cmd.SetGlobalTexture(ShaderConstants._BlendTexture, ShaderConstants._BlendTexture);
		cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, subpixelMorphologicalAntialiasing, 0, 2);
		cmd.ReleaseTemporaryRT(ShaderConstants._EdgeTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._BlendTexture);
		cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
	}

	private void DoDepthOfField(Camera camera, CommandBuffer cmd, int source, int destination)
	{
		if (m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian)
		{
			DoGaussianDepthOfField(camera, cmd, source, destination);
		}
		else if (m_DepthOfField.mode.value == DepthOfFieldMode.Bokeh)
		{
			DoBokehDepthOfField(cmd, source, destination);
		}
	}

	private void DoGaussianDepthOfField(Camera camera, CommandBuffer cmd, int source, int destination)
	{
		Material gaussianDepthOfField = m_Materials.gaussianDepthOfField;
		int num = m_Descriptor.width / 2;
		int height = m_Descriptor.height / 2;
		float value = m_DepthOfField.gaussianStart.value;
		float y = Mathf.Max(value, m_DepthOfField.gaussianEnd.value);
		float a = m_DepthOfField.gaussianMaxRadius.value * ((float)num / 1080f);
		a = Mathf.Min(a, 2f);
		CoreUtils.SetKeyword(gaussianDepthOfField, ShaderKeywordStrings.HighQualitySampling, m_DepthOfField.highQualitySampling.value);
		gaussianDepthOfField.SetVector(ShaderConstants._CoCParams, new Vector3(value, y, a));
		cmd.GetTemporaryRT(ShaderConstants._FullCoCTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_GaussianCoCFormat), FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._HalfCoCTexture, GetCompatibleDescriptor(num, height, m_GaussianCoCFormat), FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._PingTexture, GetCompatibleDescriptor(num, height, m_DefaultHDRFormat), FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._PongTexture, GetCompatibleDescriptor(num, height, m_DefaultHDRFormat), FilterMode.Bilinear);
		cmd.Blit(source, ShaderConstants._FullCoCTexture, gaussianDepthOfField, 0);
		m_MRT2[0] = ShaderConstants._HalfCoCTexture;
		m_MRT2[1] = ShaderConstants._PingTexture;
		cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
		cmd.SetViewport(camera.pixelRect);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
		cmd.SetRenderTarget(m_MRT2, ShaderConstants._HalfCoCTexture);
		cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, gaussianDepthOfField, 0, 1);
		cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
		cmd.SetGlobalTexture(ShaderConstants._HalfCoCTexture, ShaderConstants._HalfCoCTexture);
		cmd.Blit(ShaderConstants._PingTexture, ShaderConstants._PongTexture, gaussianDepthOfField, 2);
		cmd.Blit(ShaderConstants._PongTexture, ShaderConstants._PingTexture, gaussianDepthOfField, 3);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, ShaderConstants._PingTexture);
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
		cmd.Blit(source, destination, gaussianDepthOfField, 4);
		cmd.ReleaseTemporaryRT(ShaderConstants._FullCoCTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._HalfCoCTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._PingTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._PongTexture);
	}

	private void PrepareBokehKernel()
	{
		if (m_BokehKernel == null)
		{
			m_BokehKernel = new Vector4[42];
		}
		int num = 0;
		float num2 = m_DepthOfField.bladeCount.value;
		float p = 1f - m_DepthOfField.bladeCurvature.value;
		float num3 = m_DepthOfField.bladeRotation.value * (MathF.PI / 180f);
		for (int i = 1; i < 4; i++)
		{
			float num4 = 1f / 7f;
			float num5 = ((float)i + num4) / (3f + num4);
			int num6 = i * 7;
			for (int j = 0; j < num6; j++)
			{
				float num7 = MathF.PI * 2f * (float)j / (float)num6;
				float num8 = Mathf.Cos(MathF.PI / num2);
				float num9 = Mathf.Cos(num7 - MathF.PI * 2f / num2 * Mathf.Floor((num2 * num7 + MathF.PI) / (MathF.PI * 2f)));
				float num10 = num5 * Mathf.Pow(num8 / num9, p);
				float x = num10 * Mathf.Cos(num7 - num3);
				float y = num10 * Mathf.Sin(num7 - num3);
				m_BokehKernel[num] = new Vector4(x, y);
				num++;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GetMaxBokehRadiusInPixels(float viewportHeight)
	{
		return Mathf.Min(0.05f, 14f / viewportHeight);
	}

	private void DoBokehDepthOfField(CommandBuffer cmd, int source, int destination)
	{
		Material bokehDepthOfField = m_Materials.bokehDepthOfField;
		int num = m_Descriptor.width / 2;
		int num2 = m_Descriptor.height / 2;
		float num3 = m_DepthOfField.focalLength.value / 1000f;
		float num4 = m_DepthOfField.focalLength.value / m_DepthOfField.aperture.value;
		float value = m_DepthOfField.focusDistance.value;
		float y = num4 * num3 / (value - num3);
		float maxBokehRadiusInPixels = GetMaxBokehRadiusInPixels(m_Descriptor.height);
		float w = 1f / ((float)num / (float)num2);
		cmd.SetGlobalVector(ShaderConstants._CoCParams, new Vector4(value, y, maxBokehRadiusInPixels, w));
		int hashCode = m_DepthOfField.GetHashCode();
		if (hashCode != m_BokehHash)
		{
			m_BokehHash = hashCode;
			PrepareBokehKernel();
		}
		cmd.SetGlobalVectorArray(ShaderConstants._BokehKernel, m_BokehKernel);
		cmd.GetTemporaryRT(ShaderConstants._FullCoCTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8_UNorm), FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._PingTexture, GetCompatibleDescriptor(num, num2, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._PongTexture, GetCompatibleDescriptor(num, num2, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear);
		cmd.Blit(source, ShaderConstants._FullCoCTexture, bokehDepthOfField, 0);
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
		cmd.Blit(source, ShaderConstants._PingTexture, bokehDepthOfField, 1);
		cmd.Blit(ShaderConstants._PingTexture, ShaderConstants._PongTexture, bokehDepthOfField, 2);
		cmd.Blit(ShaderConstants._PongTexture, ShaderConstants._PingTexture, bokehDepthOfField, 3);
		cmd.SetGlobalTexture(ShaderConstants._DofTexture, ShaderConstants._PingTexture);
		cmd.Blit(source, destination, bokehDepthOfField, 4);
		cmd.ReleaseTemporaryRT(ShaderConstants._FullCoCTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._PingTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._PongTexture);
	}

	private void DoMotionBlur(Camera camera, CommandBuffer cmd, int source, int destination)
	{
		Material cameraMotionBlur = m_Materials.cameraMotionBlur;
		Matrix4x4 nonJitteredProjectionMatrix = camera.nonJitteredProjectionMatrix;
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 matrix4x = nonJitteredProjectionMatrix * worldToCameraMatrix;
		cameraMotionBlur.SetMatrix("_ViewProjM", matrix4x);
		if (m_ResetHistory)
		{
			cameraMotionBlur.SetMatrix("_PrevViewProjM", matrix4x);
		}
		else
		{
			cameraMotionBlur.SetMatrix("_PrevViewProjM", m_PrevViewProjM);
		}
		cameraMotionBlur.SetFloat("_Intensity", m_MotionBlur.intensity.value);
		cameraMotionBlur.SetFloat("_Clamp", m_MotionBlur.clamp.value);
		cmd.Blit(source, destination, cameraMotionBlur, (int)m_MotionBlur.quality.value);
		m_PrevViewProjM = matrix4x;
	}

	private void DoPaniniProjection(Camera camera, CommandBuffer cmd, int source, int destination)
	{
		float value = m_PaniniProjection.distance.value;
		Vector2 vector = CalcViewExtents(camera);
		Vector2 vector2 = CalcCropExtents(camera, value);
		float a = vector2.x / vector.x;
		float b = vector2.y / vector.y;
		float value2 = Mathf.Min(a, b);
		float num = value;
		float w = Mathf.Lerp(1f, Mathf.Clamp01(value2), m_PaniniProjection.cropToFit.value);
		Material paniniProjection = m_Materials.paniniProjection;
		paniniProjection.SetVector(ShaderConstants._Params, new Vector4(vector.x, vector.y, num, w));
		paniniProjection.EnableKeyword((1f - Mathf.Abs(num) > float.Epsilon) ? ShaderKeywordStrings.PaniniGeneric : ShaderKeywordStrings.PaniniUnitDistance);
		cmd.Blit(source, destination, paniniProjection);
	}

	private Vector2 CalcViewExtents(Camera camera)
	{
		float num = camera.fieldOfView * (MathF.PI / 180f);
		float num2 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		float num3 = Mathf.Tan(0.5f * num);
		return new Vector2(num2 * num3, num3);
	}

	private Vector2 CalcCropExtents(Camera camera, float d)
	{
		float num = 1f + d;
		Vector2 vector = CalcViewExtents(camera);
		float num2 = Mathf.Sqrt(vector.x * vector.x + 1f);
		float num3 = 1f / num2;
		float num4 = num3 + d;
		return vector * num3 * (num / num4);
	}

	private void SetupBloom(CommandBuffer cmd, int source, Material uberMaterial)
	{
		int num = m_Descriptor.width >> 1;
		int num2 = m_Descriptor.height >> 1;
		int num3 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log(Mathf.Max(num, num2), 2f) - 1f), 1, 16);
		float value = m_Bloom.clamp.value;
		float num4 = Mathf.GammaToLinearSpace(m_Bloom.threshold.value);
		float w = num4 * 0.5f;
		float x = Mathf.Lerp(0.05f, 0.95f, m_Bloom.scatter.value);
		Material bloom = m_Materials.bloom;
		bloom.SetVector(ShaderConstants._Params, new Vector4(x, value, num4, w));
		CoreUtils.SetKeyword(bloom, ShaderKeywordStrings.BloomHQ, m_Bloom.highQualityFiltering.value);
		CoreUtils.SetKeyword(bloom, ShaderKeywordStrings.UseRGBM, m_UseRGBM);
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(num, num2, m_DefaultHDRFormat);
		cmd.GetTemporaryRT(ShaderConstants._BloomMipDown[0], compatibleDescriptor, FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._BloomMipUp[0], compatibleDescriptor, FilterMode.Bilinear);
		cmd.Blit(source, ShaderConstants._BloomMipDown[0], bloom, 0);
		int num5 = ShaderConstants._BloomMipDown[0];
		for (int i = 1; i < num3; i++)
		{
			num = Mathf.Max(1, num >> 1);
			num2 = Mathf.Max(1, num2 >> 1);
			int num6 = ShaderConstants._BloomMipDown[i];
			int num7 = ShaderConstants._BloomMipUp[i];
			compatibleDescriptor.width = num;
			compatibleDescriptor.height = num2;
			cmd.GetTemporaryRT(num6, compatibleDescriptor, FilterMode.Bilinear);
			cmd.GetTemporaryRT(num7, compatibleDescriptor, FilterMode.Bilinear);
			cmd.Blit(num5, num7, bloom, 1);
			cmd.Blit(num7, num6, bloom, 2);
			num5 = num6;
		}
		for (int num8 = num3 - 2; num8 >= 0; num8--)
		{
			int num9 = ((num8 == num3 - 2) ? ShaderConstants._BloomMipDown[num8 + 1] : ShaderConstants._BloomMipUp[num8 + 1]);
			int num10 = ShaderConstants._BloomMipDown[num8];
			int num11 = ShaderConstants._BloomMipUp[num8];
			cmd.SetGlobalTexture(ShaderConstants._MainTexLowMip, num9);
			cmd.Blit(num10, num11, bloom, 3);
		}
		for (int j = 0; j < num3; j++)
		{
			cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipDown[j]);
			if (j > 0)
			{
				cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipUp[j]);
			}
		}
		Color color = m_Bloom.tint.value.linear;
		float num12 = ColorUtils.Luminance(in color);
		color = ((num12 > 0f) ? (color * (1f / num12)) : Color.white);
		uberMaterial.SetVector(value: new Vector4(m_Bloom.intensity.value, color.r, color.g, color.b), nameID: ShaderConstants._Bloom_Params);
		uberMaterial.SetFloat(ShaderConstants._Bloom_RGBM, m_UseRGBM ? 1f : 0f);
		cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, ShaderConstants._BloomMipUp[0]);
		Texture texture = ((m_Bloom.dirtTexture.value == null) ? Texture2D.blackTexture : m_Bloom.dirtTexture.value);
		float num13 = (float)texture.width / (float)texture.height;
		float num14 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		Vector4 value3 = new Vector4(1f, 1f, 0f, 0f);
		float value4 = m_Bloom.dirtIntensity.value;
		if (num13 > num14)
		{
			value3.x = num14 / num13;
			value3.z = (1f - value3.x) * 0.5f;
		}
		else if (num14 > num13)
		{
			value3.y = num13 / num14;
			value3.w = (1f - value3.y) * 0.5f;
		}
		uberMaterial.SetVector(ShaderConstants._LensDirt_Params, value3);
		uberMaterial.SetFloat(ShaderConstants._LensDirt_Intensity, value4);
		uberMaterial.SetTexture(ShaderConstants._LensDirt_Texture, texture);
		if (m_Bloom.highQualityFiltering.value)
		{
			uberMaterial.EnableKeyword((value4 > 0f) ? ShaderKeywordStrings.BloomHQDirt : ShaderKeywordStrings.BloomHQ);
		}
		else
		{
			uberMaterial.EnableKeyword((value4 > 0f) ? ShaderKeywordStrings.BloomLQDirt : ShaderKeywordStrings.BloomLQ);
		}
	}

	private void SetupBloomEnhanced(CommandBuffer cmd, int source, Material uberMaterial)
	{
		int num = m_Descriptor.width / 2;
		int num2 = m_Descriptor.height / 2;
		float num3 = Mathf.Log(num2, 2f) + m_BloomEnhanced.radius.value - 8f;
		int num4 = (int)num3;
		int num5 = Mathf.Clamp(num4, 1, 16);
		float thresholdLinear = m_BloomEnhanced.thresholdLinear;
		float y = (m_BloomEnhanced.antiFlicker.value ? (-0.5f) : 0f);
		float z = 0.5f + num3 - (float)num4;
		Material bloomEnhanced = m_Materials.bloomEnhanced;
		bloomEnhanced.SetVector(ShaderConstants._Params, new Vector4(thresholdLinear, y, z, m_BloomEnhanced.dirtIntensity.value));
		bloomEnhanced.SetVector(ShaderConstants._Params1, new Vector4(m_BloomEnhanced.clamp.value, 0f, 0f, 0f));
		float num6 = thresholdLinear * m_BloomEnhanced.softKnee.value + 1E-05f;
		bloomEnhanced.SetVector(value: new Vector3(thresholdLinear - num6, num6 * 2f, 0.25f / num6), nameID: ShaderConstants._Curve);
		CoreUtils.SetKeyword(bloomEnhanced, ShaderKeywordStrings.ANTI_FLICKER, m_BloomEnhanced.antiFlicker.value);
		RenderTargetHandle renderTargetHandle = default(RenderTargetHandle);
		renderTargetHandle.Init("_BloomEnhancedPrefilteredRt");
		RenderTextureDescriptor compatibleDescriptor = GetCompatibleDescriptor(num, num2, m_DefaultHDRFormat);
		cmd.GetTemporaryRT(renderTargetHandle.Id, compatibleDescriptor, FilterMode.Bilinear);
		cmd.Blit(source, renderTargetHandle.Identifier(), bloomEnhanced, 0);
		int num7 = renderTargetHandle.Id;
		for (int i = 0; i < num5; i++)
		{
			num = Mathf.Max(1, num / 2);
			num2 = Mathf.Max(1, num2 / 2);
			int num8 = ShaderConstants._BloomMipDown[i];
			int nameID = ShaderConstants._BloomMipUp[i];
			compatibleDescriptor.width = num;
			compatibleDescriptor.height = num2;
			cmd.GetTemporaryRT(num8, compatibleDescriptor, FilterMode.Bilinear);
			cmd.GetTemporaryRT(nameID, compatibleDescriptor, FilterMode.Bilinear);
			int pass = ((i == 0) ? 1 : 2);
			cmd.Blit(num7, num8, bloomEnhanced, pass);
			num7 = num8;
		}
		for (int num9 = num5 - 2; num9 >= 0; num9--)
		{
			int num10 = ShaderConstants._BloomMipDown[num9];
			cmd.SetGlobalTexture(ShaderConstants._BaseTex, num10);
			cmd.Blit(num7, ShaderConstants._BloomMipUp[num9], bloomEnhanced, 3);
			num7 = ShaderConstants._BloomMipUp[num9];
		}
		cmd.ReleaseTemporaryRT(renderTargetHandle.Id);
		for (int j = 0; j < num5; j++)
		{
			cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipDown[j]);
			if (j > 0)
			{
				cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipUp[j]);
			}
		}
		Color color = m_BloomEnhanced.tint.value.linear;
		float num11 = ColorUtils.Luminance(in color);
		color = ((num11 > 0f) ? (color * (1f / num11)) : Color.white);
		uberMaterial.SetVector(value: new Vector4(m_BloomEnhanced.intensity.value, color.r, color.g, color.b), nameID: ShaderConstants._Bloom_Params);
		uberMaterial.SetFloat(ShaderConstants._Bloom_RGBM, m_UseRGBM ? 1f : 0f);
		cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, ShaderConstants._BloomMipUp[0]);
		Texture texture = ((m_BloomEnhanced.dirtTexture.value == null) ? Texture2D.blackTexture : m_BloomEnhanced.dirtTexture.value);
		float num12 = (float)texture.width / (float)texture.height;
		float num13 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		Vector4 value3 = new Vector4(1f, 1f, 0f, 0f);
		float value4 = m_BloomEnhanced.dirtIntensity.value;
		if (num12 > num13)
		{
			value3.x = num13 / num12;
			value3.z = (1f - value3.x) * 0.5f;
		}
		else if (num13 > num12)
		{
			value3.y = num12 / num13;
			value3.w = (1f - value3.y) * 0.5f;
		}
		uberMaterial.SetVector(ShaderConstants._LensDirt_Params, value3);
		uberMaterial.SetFloat(ShaderConstants._LensDirt_Intensity, value4);
		uberMaterial.SetTexture(ShaderConstants._LensDirt_Texture, texture);
		uberMaterial.EnableKeyword((value4 > 0f) ? ShaderKeywordStrings.BloomHQDirt : ShaderKeywordStrings.BloomHQ);
	}

	private void SetupLensDistortion(Material material, bool isSceneView)
	{
		float b = 1.6f * Mathf.Max(Mathf.Abs(m_LensDistortion.intensity.value * 100f), 1f);
		float num = MathF.PI / 180f * Mathf.Min(160f, b);
		float y = 2f * Mathf.Tan(num * 0.5f);
		Vector2 vector = m_LensDistortion.center.value * 2f - Vector2.one;
		Vector4 value = new Vector4(vector.x, vector.y, Mathf.Max(m_LensDistortion.xMultiplier.value, 0.0001f), Mathf.Max(m_LensDistortion.yMultiplier.value, 0.0001f));
		Vector4 value2 = new Vector4((m_LensDistortion.intensity.value >= 0f) ? num : (1f / num), y, 1f / m_LensDistortion.scale.value, m_LensDistortion.intensity.value * 100f);
		material.SetVector(ShaderConstants._Distortion_Params1, value);
		material.SetVector(ShaderConstants._Distortion_Params2, value2);
		if (m_LensDistortion.IsActive() && !isSceneView)
		{
			material.EnableKeyword(ShaderKeywordStrings.Distortion);
		}
	}

	private void SetupChromaticAberration(Material material)
	{
		material.SetFloat(ShaderConstants._Chroma_Params, m_ChromaticAberration.intensity.value * 0.05f);
		if (m_ChromaticAberration.IsActive())
		{
			material.EnableKeyword(ShaderKeywordStrings.ChromaticAberration);
		}
	}

	private void SetupVignette(Material material)
	{
		Color value = m_Vignette.color.value;
		Vector2 value2 = m_Vignette.center.value;
		Vector4 value3 = new Vector4(value.r, value.g, value.b, m_Vignette.rounded.value ? ((float)m_Descriptor.width / (float)m_Descriptor.height) : 1f);
		Vector4 value4 = new Vector4(value2.x, value2.y, m_Vignette.intensity.value * 3f, m_Vignette.smoothness.value * 5f);
		material.SetVector(ShaderConstants._Vignette_Params1, value3);
		material.SetVector(ShaderConstants._Vignette_Params2, value4);
	}

	private void SetupColorGrading(CommandBuffer cmd, ref RenderingData renderingData, Material material)
	{
		ref PostProcessingData postProcessingData = ref renderingData.PostProcessingData;
		bool flag = postProcessingData.GradingMode == ColorGradingMode.HighDynamicRange;
		int lutSize = postProcessingData.LutSize;
		int num = lutSize * lutSize;
		float w = Mathf.Pow(2f, m_ColorAdjustments.postExposure.value);
		cmd.SetGlobalTexture(ShaderConstants._InternalLut, m_InternalLut.Identifier());
		material.SetVector(ShaderConstants._Lut_Params, new Vector4(1f / (float)num, 1f / (float)lutSize, (float)lutSize - 1f, w));
		material.SetTexture(ShaderConstants._UserLut, m_ColorLookup.texture.value);
		material.SetVector(ShaderConstants._UserLut_Params, (!m_ColorLookup.IsActive()) ? Vector4.zero : new Vector4(1f / (float)m_ColorLookup.texture.value.width, 1f / (float)m_ColorLookup.texture.value.height, (float)m_ColorLookup.texture.value.height - 1f, m_ColorLookup.contribution.value));
		if (flag)
		{
			material.EnableKeyword(ShaderKeywordStrings.HDRGrading);
			return;
		}
		switch (m_Tonemapping.mode.value)
		{
		case TonemappingMode.Neutral:
			material.EnableKeyword(ShaderKeywordStrings.TonemapNeutral);
			break;
		case TonemappingMode.ACES:
			material.EnableKeyword(ShaderKeywordStrings.TonemapACES);
			break;
		}
	}

	private void SetupGrain(Camera camera, Material material)
	{
		if (!m_HasFinalPass && m_FilmGrain.IsActive())
		{
			material.EnableKeyword(ShaderKeywordStrings.FilmGrain);
			PostProcessUtils.ConfigureFilmGrain(m_Data, m_FilmGrain, camera, material);
		}
	}

	private void SetupDithering(ref CameraData cameraData, Material material)
	{
		if (!m_HasFinalPass && cameraData.IsDitheringEnabled)
		{
			material.EnableKeyword(ShaderKeywordStrings.Dithering);
			m_DitheringTextureIndex = PostProcessUtils.ConfigureDithering(m_Data, m_DitheringTextureIndex, cameraData.Camera, material);
		}
	}

	private void RenderFinalPass(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		Material finalPass = m_Materials.finalPass;
		finalPass.shaderKeywords = null;
		if (cameraData.Antialiasing == AntialiasingMode.FastApproximateAntialiasing)
		{
			finalPass.EnableKeyword(ShaderKeywordStrings.Fxaa);
		}
		SetupGrain(cameraData.Camera, finalPass);
		SetupDithering(ref cameraData, finalPass);
		cmd.SetGlobalTexture("_BlitTex", m_Source.Identifier());
		if (cameraData.IsStereoEnabled)
		{
			Blit(cmd, m_Source.Identifier(), m_Destination.Identifier(), finalPass);
			return;
		}
		cmd.SetRenderTarget(m_Destination.Identifier());
		cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
		cmd.SetViewport(cameraData.Camera.pixelRect);
		cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, finalPass);
		cmd.SetViewProjectionMatrices(cameraData.Camera.worldToCameraMatrix, cameraData.Camera.projectionMatrix);
	}
}
