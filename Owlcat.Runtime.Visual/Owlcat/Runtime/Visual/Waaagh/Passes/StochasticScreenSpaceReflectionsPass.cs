using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class StochasticScreenSpaceReflectionsPass : ScriptableRenderPass<StochasticScreenSpaceReflectionsPassData>
{
	private static class ShaderConstants
	{
		public static readonly int _SsrHitPointTextureSize = Shader.PropertyToID("_SsrHitPointTextureSize");

		public static readonly int _SsrThicknessBias = Shader.PropertyToID("_SsrThicknessBias");

		public static readonly int _SsrThicknessScale = Shader.PropertyToID("_SsrThicknessScale");

		public static readonly int _SsrIterLimit = Shader.PropertyToID("_SsrIterLimit");

		public static readonly int _SsrRoughnessFadeEnd = Shader.PropertyToID("_SsrRoughnessFadeEnd");

		public static readonly int _SsrMinDepthMipLevel = Shader.PropertyToID("_SsrMinDepthMipLevel");

		public static readonly int _SsrBRDFBias = Shader.PropertyToID("_SsrBRDFBias");

		public static readonly int _SsrFrameCount = Shader.PropertyToID("_SsrFrameCount");

		public static readonly int _SsrEdgeFadeRcpLength = Shader.PropertyToID("_SsrEdgeFadeRcpLength");

		public static readonly int _SsrRoughnessFadeEndTimesRcpLength = Shader.PropertyToID("_SsrRoughnessFadeEndTimesRcpLength");

		public static readonly int _SsrRoughnessFadeRcpLength = Shader.PropertyToID("_SsrRoughnessFadeRcpLength");

		public static readonly int _SsrAccumulationAmount = Shader.PropertyToID("_SsrAccumulationAmount");

		public static readonly int _SsrPRBSpeedRejectionScalerFactor = Shader.PropertyToID("_SsrPRBSpeedRejectionScalerFactor");

		public static readonly int _SsrPBRSpeedRejection = Shader.PropertyToID("_SsrPBRSpeedRejection");

		public static readonly int _SsrFresnelPower = Shader.PropertyToID("_SsrFresnelPower");

		public static readonly int _SsrUseReprojectedHistory = Shader.PropertyToID("_SsrUseReprojectedHistory");

		public static readonly int _SsrRoughnessRemap = Shader.PropertyToID("_SsrRoughnessRemap");

		public static readonly int _HighlightSuppression = Shader.PropertyToID("_HighlightSuppression");

		public static readonly int _SsrPyramidLodCount = Shader.PropertyToID("_SsrPyramidLodCount");

		public static readonly int _SsrBlruEnabled = Shader.PropertyToID("_SsrBlruEnabled");

		public static readonly int _SsrScreenSize = Shader.PropertyToID("_SsrScreenSize");

		public static readonly int _SsrHitPointTexture = Shader.PropertyToID("_SsrHitPointTexture");

		public static readonly int _RankingTileXSPP = Shader.PropertyToID("_RankingTileXSPP");

		public static readonly int _OwenScrambledTexture = Shader.PropertyToID("_OwenScrambledTexture");

		public static readonly int _ScramblingTileXSPP = Shader.PropertyToID("_ScramblingTileXSPP");

		public static readonly int _SSRAccumTexture = Shader.PropertyToID("_SSRAccumTexture");

		public static readonly int _SsrAccumPrev = Shader.PropertyToID("_SsrAccumPrev");

		public static readonly int _CameraMotionVectorsTexture = Shader.PropertyToID("_CameraMotionVectorsTexture");

		public static readonly int _ColorPyramidTexture = Shader.PropertyToID("_ColorPyramidTexture");

		public static readonly int _SsrPyramidTexture = Shader.PropertyToID("_SsrPyramidTexture");
	}

	private const int kMaxSsrPyramidLodCount = 6;

	private ComputeShader m_SssrCs;

	private ComputeShaderKernelDescriptor m_RaytraceKernel;

	private ComputeShaderKernelDescriptor m_ReprojectionKernel;

	private ComputeShaderKernelDescriptor m_AccumulateKernel;

	private ComputeShaderKernelDescriptor m_BlurKernel;

	private Material m_BlitMaterial;

	private Material m_SsrBlurMaterial;

	private int m_SsrBlurPass;

	private int m_SsrCompositeSsrPass;

	private WaaaghPipelineAsset.TextureResources m_PipelineTextures;

	private ScreenSpaceReflections m_Settings;

	private LocalKeyword m_SsrApproxKeyword;

	public override string Name => "StochasticScreenSpaceReflectionsPass";

	public StochasticScreenSpaceReflectionsPass(RenderPassEvent evt, ComputeShader ssrrCs, Material blitMaterial, Material ssrBlurMaterial, WaaaghPipelineAsset.TextureResources textures)
		: base(evt)
	{
		m_SssrCs = ssrrCs;
		m_RaytraceKernel = m_SssrCs.GetKernelDescriptor("ScreenSpaceReflectionsTracing");
		m_ReprojectionKernel = m_SssrCs.GetKernelDescriptor("ScreenSpaceReflectionsReprojection");
		m_AccumulateKernel = m_SssrCs.GetKernelDescriptor("ScreenSpaceReflectionsAccumulate");
		m_BlurKernel = m_SssrCs.GetKernelDescriptor("ScreenSpaceBilateralBlur");
		m_SsrApproxKeyword = new LocalKeyword(m_SssrCs, "SSR_APPROX");
		m_BlitMaterial = blitMaterial;
		m_SsrBlurMaterial = ssrBlurMaterial;
		m_SsrBlurPass = m_SsrBlurMaterial.FindPass("BilateralBlur");
		m_SsrCompositeSsrPass = m_SsrBlurMaterial.FindPass("CompositeSSR");
		m_PipelineTextures = textures;
	}

	protected override void Setup(RenderGraphBuilder builder, StochasticScreenSpaceReflectionsPassData data, ref RenderingData renderingData)
	{
		builder.DependsOn(in data.Resources.RendererLists.OpaqueGBuffer.List);
		builder.AllowRendererListCulling(value: true);
		WaaaghCameraBuffer waaaghCameraBuffer = WaaaghCameraBuffers.EnsureCamera(ref renderingData.CameraData);
		VolumeStack stack = VolumeManager.instance.stack;
		m_Settings = stack.GetComponent<ScreenSpaceReflections>();
		if (!m_Settings.IsActive())
		{
			return;
		}
		data.SssrCS = m_SssrCs;
		data.RaytraceKernel = m_RaytraceKernel;
		data.ReprojectionKernel = m_ReprojectionKernel;
		data.AccumulateKernel = m_AccumulateKernel;
		data.BilateralBlurKernel = m_BlurKernel;
		data.SsrApproxKeyword = m_SsrApproxKeyword;
		data.BlitMaterial = m_BlitMaterial;
		data.SsrBlurMaterial = m_SsrBlurMaterial;
		data.SsrBlurPass = m_SsrBlurPass;
		data.SsrCompositeSsrPass = m_SsrCompositeSsrPass;
		int2 @int = new int2(renderingData.CameraData.CameraTargetDescriptor.width, renderingData.CameraData.CameraTargetDescriptor.height);
		switch (m_Settings.Quality.value)
		{
		case ScreenSpaceReflectionsQuality.Half:
			@int /= 2;
			data.MinDepthLevel = 1;
			break;
		case ScreenSpaceReflectionsQuality.Full:
			data.MinDepthLevel = 0;
			break;
		}
		data.MaxRaySteps = m_Settings.MaxRaySteps.value;
		Camera camera = renderingData.CameraData.Camera;
		float nearClipPlane = camera.nearClipPlane;
		float farClipPlane = camera.farClipPlane;
		float value = m_Settings.ObjectThickness.value;
		data.ThicknessScale = 1f / (1f + value);
		data.ThicknessBias = (0f - nearClipPlane) / (farClipPlane - nearClipPlane) * (value * data.ThicknessScale);
		data.MaxRoughness = m_Settings.MaxRoughness.value;
		data.SsrScreenSize = new Vector4(@int.x, @int.y, 1f / (float)@int.x, 1f / (float)@int.y);
		data.BRDFBias = 0.7f;
		data.FrameCount = renderingData.TimeData.FrameId;
		data.EdgeFadeRcpLength = Mathf.Min(1f / m_Settings.ScreenFadeDistance.value, float.MaxValue);
		float value2 = m_Settings.RoughnessFadeStart.value;
		float value3 = m_Settings.MaxRoughness.value;
		float num = value3 - value2;
		data.RoughnessFadeEndTimesRcpLength = ((num != 0f) ? (value3 * (1f / num)) : 1f);
		data.RoughnessFadeRcpLength = ((num != 0f) ? (1f / num) : 0f);
		data.AccumulationAmount = 1f - m_Settings.HistoryInfluence.value;
		data.SpeedRejectionScalerFactor = Mathf.Pow(m_Settings.SpeedRejectionScalerFactor.value * 0.1f, 2f);
		data.SpeedRejection = Mathf.Clamp01(1f - m_Settings.SpeedRejectionParam.value);
		data.BlurEnabled = m_Settings.BlurEnabled.value;
		data.FresnelPower = m_Settings.FresnelPower.value;
		data.UseReprojectedHistory = Application.isPlaying;
		data.RoughnessRemap = m_Settings.RoughnessRemap.value;
		data.RoughnessRemap.z = data.RoughnessRemap.y - data.RoughnessRemap.x;
		float4 @float = (Vector4)m_Settings.RoughnessRemap.value;
		@float.xy = 1f - @float.xy;
		@float.xy = @float.yx;
		@float.z = @float.y - @float.x;
		data.SmoothnessRemap = @float;
		data.IsStochastic = m_Settings.StochasticSSR.value;
		TextureDesc desc = RenderingUtils.CreateTextureDesc("SsrHiPointRT", renderingData.CameraData.CameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.R16G16_SFloat;
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Point;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.enableRandomWrite = true;
		desc.width = @int.x;
		desc.height = @int.y;
		data.SsrHitPointRT = builder.CreateTransientTexture(in desc);
		if (data.IsStochastic)
		{
			waaaghCameraBuffer.AllocSsrHistoryBuffer(m_Settings.Quality.value, m_Settings.ColorPrecision.value, 2);
			data.Resources.SsrRT = renderingData.RenderGraph.ImportTexture(waaaghCameraBuffer.GetCurrentFrameRT(WaaaghCameraBuffer.HistoryType.SSR));
			data.SsrRT = data.Resources.SsrRT;
			TextureHandle ssrRTPrev = renderingData.RenderGraph.ImportTexture(waaaghCameraBuffer.GetPreviousFrameRT(WaaaghCameraBuffer.HistoryType.SSR));
			data.SsrRTPrev = ssrRTPrev;
		}
		else
		{
			TextureDesc desc2 = RenderingUtils.CreateTextureDesc("SsrRT", renderingData.CameraData.CameraTargetDescriptor);
			desc2.depthBufferBits = DepthBits.None;
			desc2.filterMode = FilterMode.Bilinear;
			desc2.wrapMode = TextureWrapMode.Clamp;
			desc2.width = @int.x;
			desc2.height = @int.y;
			desc2.enableRandomWrite = true;
			if (m_Settings.ColorPrecision.value == ColorPrecision.Color32)
			{
				desc2.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
			}
			else
			{
				desc2.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
			}
			bool temporalAccumulation = renderingData.CameraData.PostProcessEnabled && renderingData.CameraData.Antialiasing == AntialiasingMode.TemporalAntialiasing;
			data.TemporalAccumulation = temporalAccumulation;
			if (data.TemporalAccumulation)
			{
				waaaghCameraBuffer.AllocSsrHistoryBuffer(m_Settings.Quality.value, m_Settings.ColorPrecision.value, 2);
				data.Resources.SsrRT = renderingData.RenderGraph.ImportTexture(waaaghCameraBuffer.GetCurrentFrameRT(WaaaghCameraBuffer.HistoryType.SSR));
				data.SsrRT = data.Resources.SsrRT;
				TextureHandle ssrRTPrev2 = renderingData.RenderGraph.ImportTexture(waaaghCameraBuffer.GetPreviousFrameRT(WaaaghCameraBuffer.HistoryType.SSR));
				data.SsrRTPrev = ssrRTPrev2;
			}
			else
			{
				data.Resources.SsrRT = renderingData.RenderGraph.CreateTexture(in desc2);
				data.SsrRT = builder.ReadWriteTexture(in data.Resources.SsrRT);
			}
			TextureDesc desc3 = desc2;
			desc3.name = "SsrPyramidMipsRT";
			desc3.autoGenerateMips = false;
			desc3.useMipMap = true;
			desc3.filterMode = FilterMode.Trilinear;
			data.SsrPyramidMips = builder.CreateTransientTexture(in desc3);
		}
		data.CameraDepthCopyRT = builder.ReadTexture(in data.Resources.CameraDepthCopyRT);
		data.CameraDepthPyramidRT = builder.ReadTexture(in data.Resources.CameraDepthPyramidRT);
		TextureHandle input = data.Resources.CameraHistoryColorBuffer;
		data.CameraHistoryColorRT = builder.ReadTexture(in input);
		data.CameraNormalsRT = builder.ReadTexture(in data.Resources.CameraNormalsRT);
		data.CameraTranslucencyRT = builder.ReadTexture(in data.Resources.CameraTranslucencyRT);
		data.CameraMotionVectorsRT = builder.ReadTexture(in data.Resources.CameraMotionVectorsRT);
		data.RankingTileXSPP = m_PipelineTextures.RankingTile1SPP;
		data.OwenScrambledTexture = m_PipelineTextures.OwenScrambled256Tex;
		data.ScramblingTileXSPP = m_PipelineTextures.ScramblingTile1SPP;
	}

	protected override void Render(StochasticScreenSpaceReflectionsPassData data, RenderGraphContext context)
	{
		context.cmd.SetRenderTarget(data.SsrHitPointRT);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraTranslucencyRT, data.CameraTranslucencyRT);
		context.cmd.SetComputeIntParam(data.SssrCS, ShaderConstants._SsrIterLimit, data.MaxRaySteps);
		context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrThicknessScale, data.ThicknessScale);
		context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrThicknessBias, data.ThicknessBias);
		context.cmd.SetComputeIntParam(data.SssrCS, ShaderConstants._SsrMinDepthMipLevel, data.MinDepthLevel);
		context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrRoughnessFadeEnd, data.MaxRoughness);
		context.cmd.SetComputeVectorParam(data.SssrCS, ShaderConstants._SsrHitPointTextureSize, data.SsrScreenSize);
		context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrBRDFBias, data.BRDFBias);
		context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrFrameCount, data.FrameCount);
		context.cmd.SetComputeVectorParam(data.SssrCS, ShaderConstants._SsrRoughnessRemap, data.RoughnessRemap);
		context.cmd.SetComputeTextureParam(data.SssrCS, data.RaytraceKernel.Index, ShaderPropertyId._CameraDepthRT, data.CameraDepthCopyRT);
		context.cmd.SetComputeTextureParam(data.SssrCS, data.RaytraceKernel.Index, ShaderPropertyId._CameraDepthPyramidRT, data.CameraDepthPyramidRT);
		context.cmd.SetComputeTextureParam(data.SssrCS, data.RaytraceKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
		context.cmd.SetComputeTextureParam(data.SssrCS, data.RaytraceKernel.Index, ShaderConstants._RankingTileXSPP, data.RankingTileXSPP);
		context.cmd.SetComputeTextureParam(data.SssrCS, data.RaytraceKernel.Index, ShaderConstants._OwenScrambledTexture, data.OwenScrambledTexture);
		context.cmd.SetComputeTextureParam(data.SssrCS, data.RaytraceKernel.Index, ShaderConstants._ScramblingTileXSPP, data.ScramblingTileXSPP);
		context.cmd.SetKeyword(data.SssrCS, in data.SsrApproxKeyword, !data.IsStochastic);
		int3 dispatchSize = data.RaytraceKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
		context.cmd.DispatchCompute(data.SssrCS, data.RaytraceKernel.Index, dispatchSize.x, dispatchSize.y, dispatchSize.z);
		context.cmd.SetRenderTarget(data.SsrRT);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrEdgeFadeRcpLength, data.EdgeFadeRcpLength);
		context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrRoughnessFadeEndTimesRcpLength, data.RoughnessFadeEndTimesRcpLength);
		context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrRoughnessFadeRcpLength, data.RoughnessFadeRcpLength);
		context.cmd.SetComputeTextureParam(data.SssrCS, data.ReprojectionKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
		context.cmd.SetComputeTextureParam(data.SssrCS, data.ReprojectionKernel.Index, ShaderConstants._CameraMotionVectorsTexture, data.CameraMotionVectorsRT);
		context.cmd.SetComputeTextureParam(data.SssrCS, data.ReprojectionKernel.Index, ShaderConstants._ColorPyramidTexture, data.CameraHistoryColorRT);
		context.cmd.SetComputeTextureParam(data.SssrCS, data.ReprojectionKernel.Index, ShaderConstants._SSRAccumTexture, data.SsrRT);
		int3 dispatchSize2 = data.ReprojectionKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
		context.cmd.DispatchCompute(data.SssrCS, data.ReprojectionKernel.Index, dispatchSize2.x, dispatchSize2.y, dispatchSize2.z);
		if (data.IsStochastic)
		{
			if (FrameDebugger.enabled)
			{
				context.cmd.SetRenderTarget(data.SsrRTPrev);
			}
			context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrAccumulationAmount, data.AccumulationAmount);
			context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrPRBSpeedRejectionScalerFactor, data.SpeedRejectionScalerFactor);
			context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrPBRSpeedRejection, data.SpeedRejection);
			context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrUseReprojectedHistory, data.UseReprojectedHistory ? 1 : 0);
			context.cmd.SetComputeTextureParam(data.SssrCS, data.AccumulateKernel.Index, ShaderConstants._CameraMotionVectorsTexture, data.CameraMotionVectorsRT);
			context.cmd.SetComputeTextureParam(data.SssrCS, data.AccumulateKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
			context.cmd.SetComputeTextureParam(data.SssrCS, data.AccumulateKernel.Index, ShaderConstants._SsrAccumPrev, data.SsrRTPrev);
			context.cmd.SetComputeTextureParam(data.SssrCS, data.AccumulateKernel.Index, ShaderConstants._SSRAccumTexture, data.SsrRT);
			int3 dispatchSize3 = data.AccumulateKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
			context.cmd.DispatchCompute(data.SssrCS, data.AccumulateKernel.Index, dispatchSize3.x, dispatchSize3.y, dispatchSize3.z);
			if (data.BlurEnabled)
			{
				ComputeShaderKernelDescriptor bilateralBlurKernel = data.BilateralBlurKernel;
				context.cmd.SetComputeTextureParam(data.SssrCS, data.BilateralBlurKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
				context.cmd.SetComputeTextureParam(data.SssrCS, bilateralBlurKernel.Index, ShaderConstants._SSRAccumTexture, data.SsrRT);
				int3 dispatchSize4 = bilateralBlurKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
				context.cmd.DispatchCompute(data.SssrCS, bilateralBlurKernel.Index, dispatchSize4.x, dispatchSize4.y, dispatchSize4.z);
			}
		}
		else
		{
			int num = 0;
			int num2 = (int)data.SsrScreenSize.x;
			int num3 = (int)data.SsrScreenSize.y;
			int num4 = num2;
			int num5 = num3;
			Vector4 value = new Vector4(1f, 1f, 0f, 0f);
			context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.SsrRT);
			context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
			context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
			context.cmd.SetGlobalFloat(ShaderConstants._HighlightSuppression, 1f);
			context.cmd.SetRenderTarget(data.SsrPyramidMips, 0);
			context.cmd.SetViewport(new Rect(0f, 0f, num2, num3));
			context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
			while (num2 >= 8 && num3 >= 8)
			{
				int num6 = math.max(1, num2 >> 1);
				int num7 = math.max(1, num3 >> 1);
				context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.SsrPyramidMips);
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(1f, 1f, 0f, 0f));
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(1f, 1f, 1f / (float)num2, 0f));
				context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, num);
				context.cmd.SetRenderTarget(data.SsrRT, 0);
				context.cmd.SetViewport(new Rect(0f, 0f, num6, num7));
				context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrBlurMaterial, data.SsrBlurPass, MeshTopology.Triangles, 3, 1);
				float x = (float)num6 / (float)num4;
				float y = (float)num7 / (float)num5;
				context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.SsrRT);
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(x, y, 0f, 0f));
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(((float)num6 - 0.5f) / (float)num4, ((float)num7 - 0.5f) / (float)num5, 0f, 0.5f / (float)num5));
				context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, 0f);
				context.cmd.SetRenderTarget(data.SsrPyramidMips, num + 1);
				context.cmd.SetViewport(new Rect(0f, 0f, num6, num7));
				context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrBlurMaterial, data.SsrBlurPass, MeshTopology.Triangles, 3, 1);
				num++;
				num2 >>= 1;
				num3 >>= 1;
				if (num > 6)
				{
					break;
				}
			}
			context.cmd.SetGlobalFloat(ShaderConstants._SsrPyramidLodCount, num);
			context.cmd.SetGlobalTexture(ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
			context.cmd.SetGlobalTexture(ShaderConstants._SsrPyramidTexture, data.SsrPyramidMips);
			context.cmd.SetGlobalVector(ShaderConstants._SsrScreenSize, data.SsrScreenSize);
			context.cmd.SetRenderTarget(data.SsrRT);
			context.cmd.SetGlobalFloat(ShaderConstants._SsrBlruEnabled, 1f);
			context.cmd.SetGlobalVector(ShaderConstants._SsrRoughnessRemap, data.SmoothnessRemap);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrBlurMaterial, data.SsrCompositeSsrPass, MeshTopology.Triangles, 3);
			if (data.TemporalAccumulation)
			{
				if (FrameDebugger.enabled)
				{
					context.cmd.SetRenderTarget(data.SsrRTPrev);
				}
				context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrAccumulationAmount, data.AccumulationAmount);
				context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrPRBSpeedRejectionScalerFactor, data.SpeedRejectionScalerFactor);
				context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrPBRSpeedRejection, data.SpeedRejection);
				context.cmd.SetComputeFloatParam(data.SssrCS, ShaderConstants._SsrUseReprojectedHistory, data.UseReprojectedHistory ? 1 : 0);
				context.cmd.SetComputeTextureParam(data.SssrCS, data.AccumulateKernel.Index, ShaderConstants._CameraMotionVectorsTexture, data.CameraMotionVectorsRT);
				context.cmd.SetComputeTextureParam(data.SssrCS, data.AccumulateKernel.Index, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
				context.cmd.SetComputeTextureParam(data.SssrCS, data.AccumulateKernel.Index, ShaderConstants._SsrAccumPrev, data.SsrRTPrev);
				context.cmd.SetComputeTextureParam(data.SssrCS, data.AccumulateKernel.Index, ShaderConstants._SSRAccumTexture, data.SsrRT);
				int3 dispatchSize5 = data.AccumulateKernel.GetDispatchSize(data.SsrScreenSize.x, data.SsrScreenSize.y, 1f);
				context.cmd.DispatchCompute(data.SssrCS, data.AccumulateKernel.Index, dispatchSize5.x, dispatchSize5.y, dispatchSize5.z);
			}
		}
		context.cmd.SetGlobalFloat(ShaderConstants._SsrFresnelPower, data.FresnelPower);
	}
}
