using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public class PostProcessPass : ScriptableRenderPass
{
	internal class MaterialLibrary : IDisposable
	{
		public readonly Material stopNaN;

		public readonly Material subpixelMorphologicalAntialiasing;

		public readonly Material temporalAntialiasing;

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

		public readonly Material finalBlit;

		private List<Material> m_Materials = new List<Material>();

		public MaterialLibrary(PostProcessData data)
		{
			m_Materials.Clear();
			stopNaN = Load(data.Shaders.StopNanPS);
			subpixelMorphologicalAntialiasing = Load(data.Shaders.SubpixelMorphologicalAntialiasingPS);
			temporalAntialiasing = Load(data.Shaders.TemporalAntialiasingPS);
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
			finalBlit = Load(data.Shaders.FinalBlitPS);
		}

		private Material Load(Shader shader)
		{
			if (shader == null)
			{
				UnityEngine.Debug.LogErrorFormat("Missing shader. " + GetType().DeclaringType.Name + " render pass will not execute. Check for missing reference in the renderer resources.");
				return null;
			}
			Material material = CoreUtils.CreateEngineMaterial(shader);
			m_Materials.Add(material);
			return material;
		}

		public void Dispose()
		{
			foreach (Material material in m_Materials)
			{
				CoreUtils.Destroy(material);
			}
			m_Materials.Clear();
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

		public static readonly int _PrevHistoryFrame = Shader.PropertyToID("_PrevHistoryFrame");

		public static readonly int _VelocityBuffer = Shader.PropertyToID("_VelocityBuffer");

		public static readonly int _JitterUV = Shader.PropertyToID("_JitterUV");

		public static readonly int _FeedbackMin = Shader.PropertyToID("_FeedbackMin");

		public static readonly int _FeedbackMax = Shader.PropertyToID("_FeedbackMax");

		public static readonly int _MotionScale = Shader.PropertyToID("_MotionScale");

		public static readonly int _FinalBlendParameters = Shader.PropertyToID("_FinalBlendParameters");

		public static readonly int _Sharpness = Shader.PropertyToID("_Sharpness");

		public static readonly int _BlitTexture = Shader.PropertyToID("_BlitTexture");

		public static readonly int _TaaAccumulationTex = Shader.PropertyToID("_TaaAccumulationTex");

		public static readonly int _TaaMotionVectorTex = Shader.PropertyToID("_TaaMotionVectorTex");

		public static readonly int _TaaFrameInfluence = Shader.PropertyToID("_TaaFrameInfluence");

		public static readonly int _TaaPostParameters = Shader.PropertyToID("_TaaPostParameters");

		public static readonly int _TaaPostParameters1 = Shader.PropertyToID("_TaaPostParameters1");

		public static readonly int _TaaFilterWeights = Shader.PropertyToID("_TaaFilterWeights");

		public static readonly int _TaaFilterWeights1 = Shader.PropertyToID("_TaaFilterWeights1");

		public static readonly int _TaaFrameInfo = Shader.PropertyToID("_TaaFrameInfo");

		public static int[] _BloomMipUp;

		public static int[] _BloomMipDown;
	}

	internal class PostProcessPassDataBase : PassDataBase
	{
		public TextureHandle Source;

		public TextureHandle Destination;

		public MaterialLibrary Materials;
	}

	internal class FinalPostProcessPassData : PostProcessPassDataBase
	{
		public Matrix4x4 ViewMatrixToRestore;

		public Matrix4x4 projectionMatrixToRestore;

		public bool ApplyRcas;

		public float RcasSharpness;

		public Rect RcasViewport;

		public bool ApplyDithering;

		public bool ApplyFilmGrain;

		public FilmGrain FilmGrain;

		public PostProcessData PostProcessData;
	}

	internal class TaaPassData : PostProcessPassDataBase
	{
		public TextureHandle CameraDepthRT;

		public TextureHandle CameraDepthCopyRT;

		public TextureHandle CurrentFrameHistory;

		public TextureHandle VelocityBuffer;

		public float FrameInfluence;

		public int QualityPass;

		public int HistoryPass;
	}

	internal class SmaaPassData : PostProcessPassDataBase
	{
		public Matrix4x4 ViewMatrixToRestore;

		public Matrix4x4 ProjectionMatrixToRestore;

		public TextureHandle SmaaEdgeTexture;

		public TextureHandle SmaaBlendTexture;

		public TextureHandle SmaaStencilTexture;
	}

	internal class GaussianDofPassData : PostProcessPassDataBase
	{
		public Matrix4x4 ViewMatrixToRestore;

		public Matrix4x4 ProjectionMatrixToRestore;

		public TextureHandle CameraDepthTexture;

		public TextureHandle FullCoCTexture;

		public TextureHandle HalfCoCTexture;

		public TextureHandle PingTexture;

		public TextureHandle PongTexture;

		public RenderTargetIdentifier[] MRT2;
	}

	internal class BokehDofPassData : PostProcessPassDataBase
	{
		public Vector4 CoCParams;

		public Vector4[] BokehKernel;

		public TextureHandle CameraDepthRT;

		public TextureHandle FullCoCTexture;

		public TextureHandle PingTexture;

		public TextureHandle PongTexture;
	}

	internal class MotionBlurPassData : PostProcessPassDataBase
	{
		public int MaterialPass;
	}

	internal class BloomPassData : PostProcessPassDataBase
	{
		public TextureHandle[] BloomMipDown;

		public TextureHandle[] BloomMipUp;

		public int MipCount;
	}

	internal class BloomEnhancedPassData : PostProcessPassDataBase
	{
		public TextureHandle[] BloomMipDown;

		public TextureHandle[] BloomMipUp;

		public TextureHandle PrefilteredRT;

		public int MipCount;
	}

	internal class UberPassData : PostProcessPassDataBase
	{
		public Matrix4x4 ViewMatrixToRestore;

		public Matrix4x4 ProjectionMatrixToRestore;

		public TextureHandle InternalLut;

		public TextureHandle BloomTex;
	}

	internal class DaltonizationPassData : PostProcessPassDataBase
	{
		public Daltonization Daltonization;
	}

	private class RcasPassData : PostProcessPassDataBase
	{
		public float Sharpness;

		public Rect Viewport;

		public bool ApplyDithering;

		public bool ApplyFilmGrain;

		public FilmGrain FilmGrain;

		public PostProcessData PostProcessData;
	}

	private const int kMaxPyramidSize = 16;

	private PostProcessData m_Settings;

	private bool m_IsFinalPostProcessPass;

	private MaterialLibrary m_Materials;

	private Dictionary<Camera, MaterialLibrary> m_CameraMaterialsMap = new Dictionary<Camera, MaterialLibrary>();

	private readonly GraphicsFormat m_DefaultHDRFormat;

	private bool m_UseRGBM;

	private readonly GraphicsFormat m_GaussianCoCFormat;

	private RenderTargetIdentifier[] m_MRT2;

	private Vector4[] m_BokehKernel;

	private int m_BokehHash;

	private bool m_ResetHistory;

	private Matrix4x4 m_PrevViewProjM = Matrix4x4.identity;

	private TextureHandle[] m_BloomMipDown;

	private TextureHandle[] m_BloomMipUp;

	private string[] m_BloomMipDownNames;

	private string[] m_BloomMipUpNames;

	private bool m_HasFinalPass;

	private int m_DitheringTextureIndex;

	private BaseRenderFunc<RcasPassData, RenderGraphContext> m_RcasRenderFunc;

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

	private TaaSharpness m_TaaSharpness;

	private TextureDesc m_Desc;

	public bool ApplyTaaRcas { get; set; }

	public bool ApplyNoiseBasedEffects { get; set; }

	public override string Name => "PostProcessPass";

	public PostProcessPass(RenderPassEvent evt, PostProcessData data, bool isFinalPostProcessPass)
		: base(evt)
	{
		m_Settings = data;
		m_IsFinalPostProcessPass = isFinalPostProcessPass;
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
		m_MRT2 = new RenderTargetIdentifier[2];
		m_ResetHistory = true;
		m_BloomMipDown = new TextureHandle[16];
		m_BloomMipUp = new TextureHandle[16];
		m_BloomMipDownNames = new string[16];
		m_BloomMipUpNames = new string[16];
		for (int i = 0; i < 16; i++)
		{
			m_BloomMipDownNames[i] = $"BloomMipDown{i}";
			m_BloomMipUpNames[i] = $"BloomMipUp{i}";
		}
	}

	public void ResetHistory()
	{
		m_ResetHistory = true;
	}

	protected override void RecordRenderGraph(ref RenderingData renderingData)
	{
		m_Materials = GetMaterials(renderingData.CameraData.Camera);
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
		m_TaaSharpness = stack.GetComponent<TaaSharpness>();
		m_Desc = RenderingUtils.CreateTextureDesc("PostProcessRTDesc", renderingData.CameraData.CameraTargetDescriptor);
		if (m_IsFinalPostProcessPass)
		{
			RenderFinalPostProcess(ref renderingData);
		}
		else
		{
			RenderPostProcess(ref renderingData);
		}
		m_ResetHistory = false;
	}

	private MaterialLibrary GetMaterials(Camera camera)
	{
		if (!m_CameraMaterialsMap.TryGetValue(camera, out var value))
		{
			value = new MaterialLibrary(m_Settings);
			m_CameraMaterialsMap.Add(camera, value);
		}
		return value;
	}

	private void RenderFinalPostProcess(ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		Material finalPass = m_Materials.finalPass;
		finalPass.shaderKeywords = null;
		RenderGraph renderGraph = renderingData.RenderGraph;
		TextureDesc desc = m_Desc;
		desc.name = "CameraAfterPostProcessRT";
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		TextureHandle input = renderingData.CameraData.Renderer.RenderGraphResources.CameraColorBuffer;
		TextureHandle input2 = renderGraph.CreateTexture(in desc);
		if (cameraData.Antialiasing == AntialiasingMode.FastApproximateAntialiasing)
		{
			finalPass.EnableKeyword(ShaderKeywordStrings.Fxaa);
		}
		bool flag = ApplyTaaRcas && cameraData.Antialiasing == AntialiasingMode.TemporalAntialiasing && m_TaaSharpness.IsActive();
		if (ApplyNoiseBasedEffects && !flag)
		{
			SetupGrain(in cameraData, finalPass);
			SetupDithering(in cameraData, finalPass);
		}
		FinalPostProcessPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<FinalPostProcessPassData>("Final Post Process Pass", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 361);
		passData2.Source = renderGraphBuilder.ReadWriteTexture(in input);
		passData2.Destination = renderGraphBuilder.ReadWriteTexture(in input2);
		passData2.Materials = m_Materials;
		passData2.ViewMatrixToRestore = cameraData.GetViewMatrix();
		passData2.projectionMatrixToRestore = cameraData.GetProjectionMatrix();
		passData2.ApplyRcas = flag;
		passData2.RcasSharpness = m_TaaSharpness.GetSharpness();
		passData2.RcasViewport = new Rect(default(Vector2), renderingData.CameraData.FinalTargetViewport.size);
		passData2.ApplyDithering = ApplyNoiseBasedEffects && renderingData.CameraData.IsDitheringEnabled;
		passData2.ApplyFilmGrain = ApplyNoiseBasedEffects && m_FilmGrain.IsActive();
		passData2.FilmGrain = m_FilmGrain;
		passData2.PostProcessData = m_Settings;
		renderGraphBuilder.SetRenderFunc(delegate(FinalPostProcessPassData passData, RenderGraphContext context)
		{
			context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTex, passData.Source);
			context.cmd.SetRenderTarget(passData.Destination);
			context.cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
			context.cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, passData.Materials.finalPass);
			if (passData.ApplyRcas)
			{
				FinalBlitter.Blit(context.cmd, inputTexture: passData.Source, outputTexture: passData.Destination, outputViewport: passData.RcasViewport, applyDithering: passData.ApplyDithering, applyFilmGrain: passData.ApplyFilmGrain, rcasSharpness: passData.RcasSharpness, filmGrain: passData.FilmGrain, material: passData.Materials.finalBlit, inputColorSpace: ColorSpace.Gamma, outputColorSpace: ColorSpace.Gamma, applyRcas: true, postProcessData: passData.PostProcessData, samplerType: FinalBlitter.SamplerType.Bilinear, ditheringTextureIndex: ref m_DitheringTextureIndex);
			}
			else
			{
				context.cmd.Blit(passData.Destination, passData.Source);
			}
			context.cmd.SetViewProjectionMatrices(passData.ViewMatrixToRestore, passData.projectionMatrixToRestore);
		});
	}

	private void RenderPostProcess(ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		RenderGraph renderGraph = renderingData.RenderGraph;
		ProfilingSampler sampler = ProfilingSampler.Get(WaaaghProfileId.RenderPostProcess);
		renderGraph.BeginProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 419);
		TextureDesc desc = m_Desc;
		desc.name = "CameraAfterPostProcessRT";
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		TextureHandle source = renderingData.CameraData.Renderer.RenderGraphResources.CameraColorBuffer;
		TextureHandle destination = TextureHandle.nullHandle;
		PostProcessPassDataBase passData2;
		RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PostProcessPassDataBase>("StopNaN and SRGBConversion", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 446);
		try
		{
			PostProcessPassDataBase postProcessPassDataBase = passData2;
			TextureHandle input = GetSource();
			postProcessPassDataBase.Source = renderGraphBuilder.ReadTexture(in input);
			PostProcessPassDataBase postProcessPassDataBase2 = passData2;
			input = GetDestination();
			postProcessPassDataBase2.Destination = renderGraphBuilder.WriteTexture(in input);
			passData2.Materials = m_Materials;
			renderGraphBuilder.SetRenderFunc(delegate(PostProcessPassDataBase passData, RenderGraphContext context)
			{
				context.cmd.Blit(passData.Source, passData.Destination, passData.Materials.stopNaN);
			});
		}
		finally
		{
			((IDisposable)renderGraphBuilder).Dispose();
		}
		Swap();
		if (cameraData.Antialiasing == AntialiasingMode.TemporalAntialiasing)
		{
			DoTemporalAntialiasing(ref renderingData, GetSource(), GetDestination());
			Swap();
		}
		else if (cameraData.IsSSREnabled)
		{
			DoCopyHistory(ref renderingData, GetSource());
		}
		if (cameraData.Antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
		{
			DoSubpixelMorphologicalAntialiasing(ref renderingData, GetSource(), GetDestination());
			Swap();
		}
		if (m_DepthOfField.IsActive() && !cameraData.IsSceneViewCamera)
		{
			DoDepthOfField(ref renderingData, GetSource(), GetDestination());
			Swap();
		}
		if (m_MotionBlur.IsActive() && !cameraData.IsSceneViewCamera)
		{
			DoMotionBlur(ref renderingData, GetSource(), GetDestination());
			Swap();
		}
		if (m_RadialBlur.IsActive() && !cameraData.IsSceneViewCamera)
		{
			DoRadialBlur(ref renderingData, GetSource(), GetDestination());
			Swap();
		}
		if (m_PaniniProjection.IsActive() && !cameraData.IsSceneViewCamera)
		{
			DoPaniniProjection(ref renderingData, GetSource(), GetDestination());
			Swap();
		}
		m_Materials.uber.shaderKeywords = null;
		if (m_Bloom.IsActive())
		{
			SetupBloom(ref renderingData, GetSource(), m_Materials.uber);
		}
		if (m_BloomEnhanced.IsActive())
		{
			SetupBloomEnhanced(ref renderingData, GetSource(), m_Materials.uber);
		}
		SetupLensDistortion(m_Materials.uber, cameraData.IsSceneViewCamera);
		SetupChromaticAberration(m_Materials.uber);
		SetupVignette(m_Materials.uber);
		SetupColorGrading(ref renderingData, m_Materials.uber);
		bool flag = ApplyTaaRcas && cameraData.Antialiasing == AntialiasingMode.TemporalAntialiasing && m_TaaSharpness.IsActive();
		if (ApplyNoiseBasedEffects && !flag)
		{
			SetupGrain(in cameraData, m_Materials.uber);
			SetupDithering(in cameraData, m_Materials.uber);
		}
		DoUberPass(ref renderingData, GetSource(), GetDestination());
		Swap();
		if (m_Daltonization.IsActive())
		{
			DoDaltonization(ref renderingData, GetSource(), GetDestination());
			Swap();
		}
		if (flag)
		{
			DoRcas(ref renderingData, GetSource(), GetDestination(), m_TaaSharpness.GetSharpness());
			Swap();
		}
		PostProcessPassDataBase passData3;
		RenderGraphBuilder renderGraphBuilder2 = renderGraph.AddRenderPass<PostProcessPassDataBase>("Final Blit", out passData3, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 562);
		try
		{
			PostProcessPassDataBase postProcessPassDataBase3 = passData3;
			TextureHandle input = GetSource();
			postProcessPassDataBase3.Source = renderGraphBuilder2.ReadTexture(in input);
			passData3.Destination = renderGraphBuilder2.WriteTexture(in renderingData.CameraData.Renderer.RenderGraphResources.CameraColorBuffer);
			renderGraphBuilder2.SetRenderFunc(delegate(PostProcessPassDataBase passData, RenderGraphContext context)
			{
				RenderTexture renderTexture = passData.Source;
				RenderTexture renderTexture2 = passData.Destination;
				if (renderTexture != renderTexture2)
				{
					context.cmd.Blit(passData.Source, passData.Destination);
				}
			});
		}
		finally
		{
			((IDisposable)renderGraphBuilder2).Dispose();
		}
		renderGraph.EndProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 579);
		TextureHandle GetDestination()
		{
			if (!destination.IsValid())
			{
				destination = renderGraph.CreateTexture(in desc);
			}
			return destination;
		}
		TextureHandle GetSource()
		{
			return source;
		}
		void Swap()
		{
			CoreUtils.Swap(ref source, ref destination);
		}
	}

	private void DoCopyHistory(ref RenderingData renderingData, TextureHandle source)
	{
		PostProcessPassDataBase passData2;
		RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<PostProcessPassDataBase>("Copy History", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 587);
		try
		{
			passData2.Materials = m_Materials;
			passData2.Source = renderGraphBuilder.ReadTexture(in source);
			PostProcessPassDataBase postProcessPassDataBase = passData2;
			TextureHandle input = renderingData.CameraData.Renderer.RenderGraphResources.CameraHistoryColorBuffer;
			postProcessPassDataBase.Destination = renderGraphBuilder.WriteTexture(in input);
			renderGraphBuilder.SetRenderFunc(delegate(PostProcessPassDataBase passData, RenderGraphContext context)
			{
				context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, passData.Source);
				context.cmd.Blit(passData.Source, passData.Destination, passData.Materials.temporalAntialiasing, 4);
			});
		}
		finally
		{
			((IDisposable)renderGraphBuilder).Dispose();
		}
	}

	private bool HasEffectsAfterUber()
	{
		return m_Daltonization.IsActive();
	}

	private void DoTemporalAntialiasing(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		RenderGraph renderGraph = renderingData.RenderGraph;
		TemporalAntialiasingSettings temporalAntialiasingSettings = WaaaghPipeline.Asset.PostProcessSettings.TemporalAntialiasingSettings;
		TaaPassData passData2;
		RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<TaaPassData>("TAA", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 627);
		try
		{
			passData2.Materials = m_Materials;
			passData2.Source = renderGraphBuilder.ReadTexture(in source);
			passData2.FrameInfluence = temporalAntialiasingSettings.FrameInfluence;
			passData2.VelocityBuffer = renderGraphBuilder.ReadTexture(in renderingData.CameraData.Renderer.RenderGraphResources.CameraMotionVectorsRT);
			passData2.QualityPass = (int)(renderingData.CameraData.AntialiasingQuality + 1);
			passData2.HistoryPass = 4;
			passData2.CameraDepthCopyRT = renderGraphBuilder.ReadTexture(in renderingData.CameraData.Renderer.RenderGraphResources.CameraDepthCopyRT);
			TaaPassData taaPassData = passData2;
			TextureHandle input = renderingData.CameraData.Renderer.RenderGraphResources.CameraHistoryColorBuffer;
			taaPassData.CurrentFrameHistory = renderGraphBuilder.ReadWriteTexture(in input);
			passData2.Destination = renderGraphBuilder.UseColorBuffer(in destination, 0);
			passData2.CameraDepthRT = renderGraphBuilder.UseDepthBuffer(in renderingData.CameraData.Renderer.RenderGraphResources.CameraDepthBuffer, DepthAccess.Read);
			renderGraphBuilder.SetRenderFunc(delegate(TaaPassData passData, RenderGraphContext context)
			{
				context.cmd.SetGlobalFloat(ShaderConstants._TaaFrameInfluence, passData.FrameInfluence);
				context.cmd.SetGlobalTexture(ShaderConstants._BlitTexture, passData.Source);
				context.cmd.SetGlobalTexture(ShaderConstants._TaaAccumulationTex, passData.CurrentFrameHistory);
				context.cmd.SetGlobalTexture(ShaderConstants._TaaMotionVectorTex, passData.VelocityBuffer);
				context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthTexture, passData.CameraDepthCopyRT);
				context.cmd.DrawProcedural(Matrix4x4.identity, passData.Materials.temporalAntialiasing, passData.QualityPass, MeshTopology.Triangles, 3);
				context.cmd.SetRenderTarget(passData.CurrentFrameHistory);
				context.cmd.SetGlobalTexture(ShaderConstants._BlitTexture, passData.Destination);
				context.cmd.DrawProcedural(Matrix4x4.identity, passData.Materials.temporalAntialiasing, passData.HistoryPass, MeshTopology.Triangles, 3);
			});
		}
		finally
		{
			((IDisposable)renderGraphBuilder).Dispose();
		}
	}

	private void DoSubpixelMorphologicalAntialiasing(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		RenderGraph renderGraph = renderingData.RenderGraph;
		Material subpixelMorphologicalAntialiasing = m_Materials.subpixelMorphologicalAntialiasing;
		RenderTextureDescriptor cameraTargetDescriptor = cameraData.CameraTargetDescriptor;
		subpixelMorphologicalAntialiasing.SetVector(ShaderConstants._Metrics, new Vector4(1f / (float)cameraTargetDescriptor.width, 1f / (float)cameraTargetDescriptor.height, cameraTargetDescriptor.width, cameraTargetDescriptor.height));
		subpixelMorphologicalAntialiasing.SetTexture(ShaderConstants._AreaTexture, m_Settings.Textures.SmaaAreaTex);
		subpixelMorphologicalAntialiasing.SetTexture(ShaderConstants._SearchTexture, m_Settings.Textures.SmaaSearchTex);
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
		SmaaPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<SmaaPassData>("SMAA", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 705);
		passData2.Source = renderGraphBuilder.ReadTexture(in source);
		passData2.Destination = renderGraphBuilder.WriteTexture(in destination);
		passData2.Materials = m_Materials;
		TextureDesc desc = m_Desc;
		desc.name = "SmaaEdgeTexture";
		desc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Point;
		passData2.SmaaEdgeTexture = renderGraphBuilder.CreateTransientTexture(in desc);
		TextureDesc desc2 = desc;
		desc2.name = "SmaaBlendTexture";
		desc2.depthBufferBits = DepthBits.None;
		passData2.SmaaBlendTexture = renderGraphBuilder.CreateTransientTexture(in desc2);
		TextureDesc desc3 = m_Desc;
		desc3.name = "SmaaStencilTexture";
		desc3.colorFormat = GraphicsFormat.D24_UNorm_S8_UInt;
		passData2.SmaaStencilTexture = renderGraphBuilder.CreateTransientTexture(in desc3);
		passData2.ViewMatrixToRestore = cameraData.GetViewMatrix();
		passData2.ProjectionMatrixToRestore = cameraData.GetProjectionMatrix();
		renderGraphBuilder.SetRenderFunc(delegate(SmaaPassData passData, RenderGraphContext context)
		{
			context.cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
			context.cmd.SetRenderTarget(passData.SmaaEdgeTexture, passData.SmaaStencilTexture);
			context.cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
			context.cmd.SetGlobalTexture(ShaderConstants._ColorTexture, passData.Source);
			context.cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, passData.Materials.subpixelMorphologicalAntialiasing, 0, 0);
			context.cmd.SetRenderTarget(passData.SmaaBlendTexture, passData.SmaaStencilTexture);
			context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
			context.cmd.SetGlobalTexture(ShaderConstants._ColorTexture, passData.SmaaEdgeTexture);
			context.cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, passData.Materials.subpixelMorphologicalAntialiasing, 0, 1);
			context.cmd.SetRenderTarget(passData.Destination);
			context.cmd.SetGlobalTexture(ShaderConstants._ColorTexture, passData.Source);
			context.cmd.SetGlobalTexture(ShaderConstants._BlendTexture, passData.SmaaBlendTexture);
			context.cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, passData.Materials.subpixelMorphologicalAntialiasing, 0, 2);
			context.cmd.SetViewProjectionMatrices(passData.ViewMatrixToRestore, passData.ProjectionMatrixToRestore);
		});
	}

	private void DoDepthOfField(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		if (m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian)
		{
			DoGaussianDepthOfField(ref renderingData, source, destination);
		}
		else if (m_DepthOfField.mode.value == DepthOfFieldMode.Bokeh)
		{
			DoBokehDepthOfField(ref renderingData, source, destination);
		}
	}

	private void DoGaussianDepthOfField(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		RenderGraph renderGraph = renderingData.RenderGraph;
		Material gaussianDepthOfField = m_Materials.gaussianDepthOfField;
		int num = m_Desc.width / 2;
		int height = m_Desc.height / 2;
		float value = m_DepthOfField.gaussianStart.value;
		float y = Mathf.Max(value, m_DepthOfField.gaussianEnd.value);
		float a = m_DepthOfField.gaussianMaxRadius.value * ((float)num / 1080f);
		a = Mathf.Min(a, 2f);
		CoreUtils.SetKeyword(gaussianDepthOfField, ShaderKeywordStrings.HighQualitySampling, m_DepthOfField.highQualitySampling.value);
		gaussianDepthOfField.SetVector(ShaderConstants._CoCParams, new Vector3(value, y, a));
		GaussianDofPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<GaussianDofPassData>("Depth Of Field (Gaussian)", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 807);
		passData2.CameraDepthTexture = renderGraphBuilder.ReadTexture(in renderingData.CameraData.Renderer.RenderGraphResources.CameraDepthBuffer);
		passData2.ViewMatrixToRestore = renderingData.CameraData.GetViewMatrix();
		passData2.ProjectionMatrixToRestore = renderingData.CameraData.GetProjectionMatrix();
		passData2.Source = renderGraphBuilder.ReadTexture(in source);
		passData2.Destination = renderGraphBuilder.WriteTexture(in destination);
		passData2.Materials = m_Materials;
		TextureDesc desc = m_Desc;
		desc.name = "FullCoCTexture";
		desc.colorFormat = m_GaussianCoCFormat;
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Bilinear;
		passData2.FullCoCTexture = renderGraphBuilder.CreateTransientTexture(in desc);
		TextureDesc desc2 = desc;
		desc2.name = "HalfCoCTexture";
		desc2.width = num;
		desc2.height = height;
		passData2.HalfCoCTexture = renderGraphBuilder.CreateTransientTexture(in desc2);
		TextureDesc desc3 = desc2;
		desc3.name = "PingTexture";
		desc3.colorFormat = m_DefaultHDRFormat;
		passData2.PingTexture = renderGraphBuilder.CreateTransientTexture(in desc3);
		TextureDesc desc4 = desc3;
		desc4.name = "PongTexture";
		passData2.PongTexture = renderGraphBuilder.CreateTransientTexture(in desc4);
		passData2.MRT2 = m_MRT2;
		renderGraphBuilder.SetRenderFunc(delegate(GaussianDofPassData passData, RenderGraphContext context)
		{
			context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthTexture, passData.CameraDepthTexture);
			context.cmd.Blit(passData.Source, passData.FullCoCTexture, passData.Materials.gaussianDepthOfField, 0);
			passData.MRT2[0] = passData.HalfCoCTexture;
			passData.MRT2[1] = passData.PingTexture;
			context.cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
			context.cmd.SetGlobalTexture(ShaderConstants._ColorTexture, passData.Source);
			context.cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, passData.FullCoCTexture);
			context.cmd.SetRenderTarget(m_MRT2, passData.HalfCoCTexture);
			context.cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, passData.Materials.gaussianDepthOfField, 0, 1);
			context.cmd.SetViewProjectionMatrices(passData.ViewMatrixToRestore, passData.ProjectionMatrixToRestore);
			context.cmd.SetGlobalTexture(ShaderConstants._HalfCoCTexture, passData.HalfCoCTexture);
			context.cmd.Blit(passData.PingTexture, passData.PongTexture, passData.Materials.gaussianDepthOfField, 2);
			context.cmd.Blit(passData.PongTexture, passData.PingTexture, passData.Materials.gaussianDepthOfField, 3);
			context.cmd.SetGlobalTexture(ShaderConstants._ColorTexture, passData.PingTexture);
			context.cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, passData.FullCoCTexture);
			context.cmd.Blit(passData.Source, passData.Destination, passData.Materials.gaussianDepthOfField, 4);
		});
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

	private void DoBokehDepthOfField(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		_ = m_Materials.bokehDepthOfField;
		int num = m_Desc.width / 2;
		int num2 = m_Desc.height / 2;
		float num3 = m_DepthOfField.focalLength.value / 1000f;
		float num4 = m_DepthOfField.focalLength.value / m_DepthOfField.aperture.value;
		float value = m_DepthOfField.focusDistance.value;
		float y = num4 * num3 / (value - num3);
		float maxBokehRadiusInPixels = GetMaxBokehRadiusInPixels(m_Desc.height);
		float w = 1f / ((float)num / (float)num2);
		Vector4 coCParams = new Vector4(value, y, maxBokehRadiusInPixels, w);
		int hashCode = m_DepthOfField.GetHashCode();
		if (hashCode != m_BokehHash)
		{
			m_BokehHash = hashCode;
			PrepareBokehKernel();
		}
		BokehDofPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<BokehDofPassData>("Depth Of Field (Bokeh)", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 959);
		passData2.CameraDepthRT = renderGraphBuilder.ReadTexture(in renderingData.CameraData.Renderer.RenderGraphResources.CameraDepthBuffer);
		passData2.Source = renderGraphBuilder.ReadTexture(in source);
		passData2.Destination = renderGraphBuilder.WriteTexture(in destination);
		passData2.Materials = m_Materials;
		passData2.CoCParams = coCParams;
		passData2.BokehKernel = m_BokehKernel;
		TextureDesc desc = m_Desc;
		desc.name = "FullCoCTexture";
		desc.colorFormat = GraphicsFormat.R8_UNorm;
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Bilinear;
		passData2.FullCoCTexture = renderGraphBuilder.CreateTransientTexture(in desc);
		TextureDesc desc2 = m_Desc;
		desc2.name = "PingTexture";
		desc2.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
		desc2.depthBufferBits = DepthBits.None;
		desc2.filterMode = FilterMode.Bilinear;
		desc2.width = num;
		desc2.height = num2;
		passData2.PingTexture = renderGraphBuilder.CreateTransientTexture(in desc2);
		TextureDesc desc3 = desc2;
		desc3.name = "PongTexture";
		passData2.PongTexture = renderGraphBuilder.CreateTransientTexture(in desc3);
		renderGraphBuilder.SetRenderFunc(delegate(BokehDofPassData passData, RenderGraphContext context)
		{
			context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthTexture, passData.CameraDepthRT);
			context.cmd.SetGlobalVector(ShaderConstants._CoCParams, passData.CoCParams);
			context.cmd.SetGlobalVectorArray(ShaderConstants._BokehKernel, passData.BokehKernel);
			context.cmd.Blit(passData.Source, passData.FullCoCTexture, passData.Materials.bokehDepthOfField, 0);
			context.cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, passData.FullCoCTexture);
			context.cmd.Blit(passData.Source, passData.PingTexture, passData.Materials.bokehDepthOfField, 1);
			context.cmd.Blit(passData.PingTexture, passData.PongTexture, passData.Materials.bokehDepthOfField, 2);
			context.cmd.Blit(passData.PongTexture, passData.PingTexture, passData.Materials.bokehDepthOfField, 3);
			context.cmd.SetGlobalTexture(ShaderConstants._DofTexture, passData.PingTexture);
			context.cmd.Blit(passData.Source, passData.Destination, passData.Materials.bokehDepthOfField, 4);
		});
	}

	private void DoMotionBlur(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		Camera camera = renderingData.CameraData.Camera;
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
		MotionBlurPassData passData2;
		using (RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<MotionBlurPassData>("Motion Blur", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1047))
		{
			passData2.Source = renderGraphBuilder.ReadTexture(in source);
			passData2.Destination = renderGraphBuilder.WriteTexture(in destination);
			passData2.Materials = m_Materials;
			passData2.MaterialPass = (int)m_MotionBlur.quality.value;
			renderGraphBuilder.SetRenderFunc(delegate(MotionBlurPassData passData, RenderGraphContext context)
			{
				context.cmd.Blit(passData.Source, passData.Destination, passData.Materials.cameraMotionBlur, passData.MaterialPass);
			});
		}
		m_PrevViewProjM = matrix4x;
	}

	private void DoRadialBlur(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		Material radialBlur = m_Materials.radialBlur;
		radialBlur.SetVector("_RadialBlurCenter", m_RadialBlur.Center.value);
		radialBlur.SetFloat("_RadialBlurStrength", m_RadialBlur.Strength.value);
		radialBlur.SetFloat("_RadialBlurWidth", m_RadialBlur.Width.value);
		PostProcessPassDataBase passData2;
		using RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<PostProcessPassDataBase>("Radial Blur", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1076);
		passData2.Source = renderGraphBuilder.ReadTexture(in source);
		passData2.Destination = renderGraphBuilder.WriteTexture(in destination);
		passData2.Materials = m_Materials;
		renderGraphBuilder.SetRenderFunc(delegate(PostProcessPassDataBase passData, RenderGraphContext context)
		{
			context.cmd.Blit(passData.Source, passData.Destination, passData.Materials.radialBlur, 0);
		});
	}

	private Vector2 CalcViewExtents(Camera camera)
	{
		float num = camera.fieldOfView * (MathF.PI / 180f);
		float num2 = (float)m_Desc.width / (float)m_Desc.height;
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

	private void DoPaniniProjection(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		Camera camera = renderingData.CameraData.Camera;
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
		PostProcessPassDataBase passData2;
		using RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<PostProcessPassDataBase>("Panini Projection", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1162);
		passData2.Source = renderGraphBuilder.ReadTexture(in source);
		passData2.Destination = renderGraphBuilder.WriteTexture(in destination);
		passData2.Materials = m_Materials;
		renderGraphBuilder.SetRenderFunc(delegate(PostProcessPassDataBase passData, RenderGraphContext context)
		{
			context.cmd.Blit(passData.Source, passData.Destination, passData.Materials.paniniProjection);
		});
	}

	private void SetupBloom(ref RenderingData renderingData, TextureHandle source, Material uberMaterial)
	{
		int num = m_Desc.width >> 1;
		int num2 = m_Desc.height >> 1;
		int num3 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log(Mathf.Max(num, num2), 2f) - 1f), 1, 16);
		float value = m_Bloom.clamp.value;
		float num4 = Mathf.GammaToLinearSpace(m_Bloom.threshold.value);
		float w = num4 * 0.5f;
		float x = Mathf.Lerp(0.05f, 0.95f, m_Bloom.scatter.value);
		Material bloom = m_Materials.bloom;
		bloom.SetVector(ShaderConstants._Params, new Vector4(x, value, num4, w));
		CoreUtils.SetKeyword(bloom, ShaderKeywordStrings.BloomHQ, m_Bloom.highQualityFiltering.value);
		CoreUtils.SetKeyword(bloom, ShaderKeywordStrings.UseRGBM, m_UseRGBM);
		RenderGraph renderGraph = renderingData.RenderGraph;
		BloomPassData passData2;
		using (RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<BloomPassData>("Bloom", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1209))
		{
			passData2.Source = renderGraphBuilder.ReadTexture(in source);
			passData2.Materials = m_Materials;
			passData2.BloomMipDown = m_BloomMipDown;
			passData2.BloomMipUp = m_BloomMipUp;
			passData2.MipCount = num3;
			TextureDesc desc = m_Desc;
			desc.colorFormat = m_DefaultHDRFormat;
			desc.depthBufferBits = DepthBits.None;
			desc.width = num;
			desc.height = num2;
			desc.filterMode = FilterMode.Bilinear;
			desc.name = m_BloomMipDownNames[0];
			passData2.BloomMipDown[0] = renderGraphBuilder.CreateTransientTexture(in desc);
			desc.name = m_BloomMipUpNames[0];
			renderingData.CameraData.Renderer.RenderGraphResources.BloomTexture = renderGraph.CreateTexture(in desc);
			passData2.BloomMipUp[0] = renderGraphBuilder.ReadWriteTexture(in renderingData.CameraData.Renderer.RenderGraphResources.BloomTexture);
			for (int i = 1; i < num3; i++)
			{
				num = Mathf.Max(1, num >> 1);
				num2 = Mathf.Max(1, num2 >> 1);
				desc.width = num;
				desc.height = num2;
				desc.name = m_BloomMipDownNames[i];
				passData2.BloomMipDown[i] = renderGraphBuilder.CreateTransientTexture(in desc);
				desc.name = m_BloomMipUpNames[i];
				passData2.BloomMipUp[i] = renderGraphBuilder.CreateTransientTexture(in desc);
			}
			renderGraphBuilder.SetRenderFunc(delegate(BloomPassData passData, RenderGraphContext context)
			{
				context.cmd.Blit(passData.Source, passData.BloomMipDown[0], passData.Materials.bloom, 0);
				TextureHandle textureHandle = passData.BloomMipDown[0];
				for (int j = 1; j < passData.MipCount; j++)
				{
					TextureHandle textureHandle2 = passData.BloomMipDown[j];
					TextureHandle textureHandle3 = passData.BloomMipUp[j];
					context.cmd.Blit(textureHandle, textureHandle3, passData.Materials.bloom, 1);
					context.cmd.Blit(textureHandle3, textureHandle2, passData.Materials.bloom, 2);
					textureHandle = textureHandle2;
				}
				for (int num8 = passData.MipCount - 2; num8 >= 0; num8--)
				{
					TextureHandle textureHandle4 = ((num8 == passData.MipCount - 2) ? passData.BloomMipDown[num8 + 1] : passData.BloomMipUp[num8 + 1]);
					TextureHandle textureHandle5 = passData.BloomMipDown[num8];
					TextureHandle textureHandle6 = passData.BloomMipUp[num8];
					context.cmd.SetGlobalTexture(ShaderConstants._MainTexLowMip, textureHandle4);
					context.cmd.Blit(textureHandle5, textureHandle6, passData.Materials.bloom, 3);
				}
				context.cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, passData.BloomMipUp[0]);
			});
		}
		Color color = m_Bloom.tint.value.linear;
		float num5 = ColorUtils.Luminance(in color);
		color = ((num5 > 0f) ? (color * (1f / num5)) : Color.white);
		uberMaterial.SetVector(value: new Vector4(m_Bloom.intensity.value, color.r, color.g, color.b), nameID: ShaderConstants._Bloom_Params);
		uberMaterial.SetFloat(ShaderConstants._Bloom_RGBM, m_UseRGBM ? 1f : 0f);
		Texture texture = ((m_Bloom.dirtTexture.value == null) ? Texture2D.blackTexture : m_Bloom.dirtTexture.value);
		float num6 = (float)texture.width / (float)texture.height;
		float num7 = (float)m_Desc.width / (float)m_Desc.height;
		Vector4 value3 = new Vector4(1f, 1f, 0f, 0f);
		float value4 = m_Bloom.dirtIntensity.value;
		if (num6 > num7)
		{
			value3.x = num7 / num6;
			value3.z = (1f - value3.x) * 0.5f;
		}
		else if (num7 > num6)
		{
			value3.y = num6 / num7;
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

	private void SetupBloomEnhanced(ref RenderingData renderingData, TextureHandle source, Material uberMaterial)
	{
		int num = m_Desc.width / 2;
		int num2 = m_Desc.height / 2;
		float num3 = Mathf.Log(num2, 2f) + m_BloomEnhanced.radius.value - 8f;
		int num4 = (int)num3;
		int mipCount = Mathf.Clamp(num4, 1, 16);
		float thresholdLinear = m_BloomEnhanced.thresholdLinear;
		float y = (m_BloomEnhanced.antiFlicker.value ? (-0.5f) : 0f);
		float z = 0.5f + num3 - (float)num4;
		Material bloomEnhanced = m_Materials.bloomEnhanced;
		bloomEnhanced.SetVector(ShaderConstants._Params, new Vector4(thresholdLinear, y, z, m_BloomEnhanced.dirtIntensity.value));
		bloomEnhanced.SetVector(ShaderConstants._Params1, new Vector4(m_BloomEnhanced.clamp.value, 0f, 0f, 0f));
		float num5 = thresholdLinear * m_BloomEnhanced.softKnee.value + 1E-05f;
		bloomEnhanced.SetVector(value: new Vector3(thresholdLinear - num5, num5 * 2f, 0.25f / num5), nameID: ShaderConstants._Curve);
		CoreUtils.SetKeyword(bloomEnhanced, ShaderKeywordStrings.ANTI_FLICKER, m_BloomEnhanced.antiFlicker.value);
		RenderGraph renderGraph = renderingData.RenderGraph;
		BloomEnhancedPassData passData2;
		using (RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<BloomEnhancedPassData>("Bloom Enhanced", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1359))
		{
			passData2.Source = renderGraphBuilder.ReadTexture(in source);
			passData2.Materials = m_Materials;
			passData2.BloomMipDown = m_BloomMipDown;
			passData2.BloomMipUp = m_BloomMipUp;
			passData2.MipCount = mipCount;
			TextureDesc desc = m_Desc;
			desc.colorFormat = m_DefaultHDRFormat;
			desc.depthBufferBits = DepthBits.None;
			desc.width = num;
			desc.height = num2;
			desc.filterMode = FilterMode.Bilinear;
			passData2.PrefilteredRT = renderGraphBuilder.CreateTransientTexture(in desc);
			for (int i = 0; i < passData2.MipCount; i++)
			{
				num = Mathf.Max(1, num / 2);
				num2 = Mathf.Max(1, num2 / 2);
				_ = ref passData2.BloomMipDown[i];
				_ = ref passData2.BloomMipUp[i];
				desc.width = num;
				desc.height = num2;
				desc.name = m_BloomMipDownNames[i];
				passData2.BloomMipDown[i] = renderGraphBuilder.CreateTransientTexture(in desc);
				desc.name = m_BloomMipUpNames[i];
				if (i == 0)
				{
					renderingData.CameraData.Renderer.RenderGraphResources.BloomTexture = renderGraph.CreateTexture(in desc);
					passData2.BloomMipUp[i] = renderGraphBuilder.ReadWriteTexture(in renderingData.CameraData.Renderer.RenderGraphResources.BloomTexture);
				}
				else
				{
					passData2.BloomMipUp[i] = renderGraphBuilder.CreateTransientTexture(in desc);
				}
			}
			renderGraphBuilder.SetRenderFunc(delegate(BloomEnhancedPassData passData, RenderGraphContext context)
			{
				context.cmd.Blit(passData.Source, passData.PrefilteredRT, passData.Materials.bloomEnhanced, 0);
				TextureHandle textureHandle = passData.PrefilteredRT;
				for (int j = 0; j < passData.MipCount; j++)
				{
					TextureHandle textureHandle2 = passData.BloomMipDown[j];
					_ = ref passData.BloomMipUp[j];
					int pass = ((j == 0) ? 1 : 2);
					context.cmd.Blit(textureHandle, textureHandle2, passData.Materials.bloomEnhanced, pass);
					textureHandle = textureHandle2;
				}
				for (int num9 = passData.MipCount - 2; num9 >= 0; num9--)
				{
					TextureHandle textureHandle3 = passData.BloomMipDown[num9];
					context.cmd.SetGlobalTexture(ShaderConstants._BaseTex, textureHandle3);
					context.cmd.Blit(textureHandle, passData.BloomMipUp[num9], passData.Materials.bloomEnhanced, 3);
					textureHandle = passData.BloomMipUp[num9];
				}
				context.cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, passData.BloomMipUp[0]);
			});
		}
		Color color = m_BloomEnhanced.tint.value.linear;
		float num6 = ColorUtils.Luminance(in color);
		color = ((num6 > 0f) ? (color * (1f / num6)) : Color.white);
		uberMaterial.SetVector(value: new Vector4(m_BloomEnhanced.intensity.value, color.r, color.g, color.b), nameID: ShaderConstants._Bloom_Params);
		uberMaterial.SetFloat(ShaderConstants._Bloom_RGBM, m_UseRGBM ? 1f : 0f);
		Texture texture = ((m_BloomEnhanced.dirtTexture.value == null) ? Texture2D.blackTexture : m_BloomEnhanced.dirtTexture.value);
		float num7 = (float)texture.width / (float)texture.height;
		float num8 = (float)m_Desc.width / (float)m_Desc.height;
		Vector4 value3 = new Vector4(1f, 1f, 0f, 0f);
		float value4 = m_BloomEnhanced.dirtIntensity.value;
		if (num7 > num8)
		{
			value3.x = num8 / num7;
			value3.z = (1f - value3.x) * 0.5f;
		}
		else if (num8 > num7)
		{
			value3.y = num7 / num8;
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
		Vector4 value3 = new Vector4(value.r, value.g, value.b, m_Vignette.rounded.value ? ((float)m_Desc.width / (float)m_Desc.height) : 1f);
		Vector4 value4 = new Vector4(value2.x, value2.y, m_Vignette.intensity.value * 3f, m_Vignette.smoothness.value * 5f);
		material.SetVector(ShaderConstants._Vignette_Params1, value3);
		material.SetVector(ShaderConstants._Vignette_Params2, value4);
	}

	private void SetupColorGrading(ref RenderingData renderingData, Material material)
	{
		ref PostProcessingData postProcessingData = ref renderingData.PostProcessingData;
		bool flag = postProcessingData.GradingMode == ColorGradingMode.HighDynamicRange;
		int lutSize = postProcessingData.LutSize;
		int num = lutSize * lutSize;
		float w = Mathf.Pow(2f, m_ColorAdjustments.postExposure.value);
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

	private void SetupGrain(in CameraData cameraData, Material material)
	{
		if (m_FilmGrain.IsActive())
		{
			material.EnableKeyword(ShaderKeywordStrings.FilmGrain);
			PostProcessUtils.ConfigureFilmGrain(m_Settings, m_FilmGrain, (int)cameraData.FinalTargetViewport.width, (int)cameraData.FinalTargetViewport.height, material);
		}
	}

	private void SetupDithering(in CameraData cameraData, Material material)
	{
		if (cameraData.IsDitheringEnabled)
		{
			material.EnableKeyword(ShaderKeywordStrings.Dithering);
			m_DitheringTextureIndex = PostProcessUtils.ConfigureDithering(m_Settings, m_DitheringTextureIndex, (int)cameraData.FinalTargetViewport.width, (int)cameraData.FinalTargetViewport.height, material);
		}
	}

	private void DoUberPass(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		UberPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<UberPassData>("Uber Pass", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1619);
		passData2.Source = renderGraphBuilder.ReadTexture(in source);
		passData2.Destination = renderGraphBuilder.WriteTexture(in destination);
		passData2.Materials = m_Materials;
		passData2.ViewMatrixToRestore = renderingData.CameraData.GetViewMatrix();
		passData2.ProjectionMatrixToRestore = renderingData.CameraData.GetProjectionMatrix();
		passData2.InternalLut = renderGraphBuilder.ReadTexture(in renderingData.CameraData.Renderer.RenderGraphResources.ColorGradingLut);
		if (m_Bloom.IsActive())
		{
			passData2.BloomTex = renderGraphBuilder.ReadTexture(in renderingData.CameraData.Renderer.RenderGraphResources.BloomTexture);
		}
		if (m_BloomEnhanced.IsActive())
		{
			passData2.BloomTex = renderGraphBuilder.ReadTexture(in renderingData.CameraData.Renderer.RenderGraphResources.BloomTexture);
		}
		renderGraphBuilder.SetRenderFunc(delegate(UberPassData passData, RenderGraphContext context)
		{
			context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTex, passData.Source);
			context.cmd.SetGlobalTexture(ShaderConstants._InternalLut, passData.InternalLut);
			context.cmd.SetRenderTarget(passData.Destination);
			context.cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
			context.cmd.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, passData.Materials.uber);
			context.cmd.SetViewProjectionMatrices(passData.ViewMatrixToRestore, passData.ProjectionMatrixToRestore);
		});
	}

	private void DoDaltonization(ref RenderingData renderingData, TextureHandle source, TextureHandle destination)
	{
		DaltonizationPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<DaltonizationPassData>("Daltonization", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1663);
		passData2.Source = renderGraphBuilder.ReadTexture(in source);
		passData2.Destination = renderGraphBuilder.WriteTexture(in destination);
		passData2.Materials = m_Materials;
		passData2.Daltonization = m_Daltonization;
		renderGraphBuilder.SetRenderFunc(delegate(DaltonizationPassData passData, RenderGraphContext context)
		{
			float num = Mathf.Clamp01(passData.Daltonization.BrightnessFactor.value);
			float num2 = Mathf.Clamp01(passData.Daltonization.ContrastFactor.value);
			float x = 1f - Mathf.Abs(num - 0.5f);
			int num3 = (((double)num >= 0.5) ? 1 : 0);
			float z = num2 + 0.5f;
			context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTex, passData.Source);
			context.cmd.SetGlobalVector(ShaderConstants._Params, new Vector4(passData.Daltonization.ProtanopiaFactor.value, passData.Daltonization.DeuteranopiaFactor.value, passData.Daltonization.TritanopiaFactor.value, passData.Daltonization.Intensity.value));
			context.cmd.SetGlobalVector(ShaderConstants._Params1, new Vector4(x, num3, z, 0f));
			context.cmd.Blit(passData.Source, passData.Destination, passData.Materials.daltonization, 0);
		});
	}

	private void DoRcas(ref RenderingData renderingData, TextureHandle source, TextureHandle destination, float sharpness)
	{
		RcasPassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderingData.RenderGraph.AddRenderPass<RcasPassData>("Rcas", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@0f6cf20663a3\\Runtime\\Waaagh\\Passes\\PostProcess\\PostProcessPass.cs", 1706);
		passData.Sharpness = sharpness;
		passData.Viewport = new Rect(default(Vector2), renderingData.CameraData.FinalTargetViewport.size);
		passData.Source = renderGraphBuilder.ReadTexture(in source);
		passData.Destination = renderGraphBuilder.WriteTexture(in destination);
		passData.Materials = m_Materials;
		passData.ApplyDithering = ApplyNoiseBasedEffects && renderingData.CameraData.IsDitheringEnabled;
		passData.ApplyFilmGrain = ApplyNoiseBasedEffects && m_FilmGrain.IsActive();
		passData.FilmGrain = m_FilmGrain;
		passData.PostProcessData = m_Settings;
		if (m_RcasRenderFunc == null)
		{
			m_RcasRenderFunc = delegate(RcasPassData data, RenderGraphContext context)
			{
				FinalBlitter.Blit(context.cmd, inputTexture: data.Source, outputTexture: data.Destination, outputViewport: data.Viewport, applyDithering: data.ApplyDithering, applyFilmGrain: data.ApplyFilmGrain, rcasSharpness: data.Sharpness, filmGrain: data.FilmGrain, material: data.Materials.finalBlit, inputColorSpace: ColorSpace.Gamma, outputColorSpace: ColorSpace.Gamma, applyRcas: true, postProcessData: data.PostProcessData, samplerType: FinalBlitter.SamplerType.Bilinear, ditheringTextureIndex: ref m_DitheringTextureIndex);
			};
		}
		renderGraphBuilder.SetRenderFunc(m_RcasRenderFunc);
	}

	public void Cleanup()
	{
		foreach (KeyValuePair<Camera, MaterialLibrary> item in m_CameraMaterialsMap)
		{
			item.Value.Dispose();
		}
		m_CameraMaterialsMap.Clear();
	}
}
