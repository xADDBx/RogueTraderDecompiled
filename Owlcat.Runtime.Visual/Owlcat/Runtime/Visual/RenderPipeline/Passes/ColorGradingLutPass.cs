using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using Owlcat.Runtime.Visual.RenderPipeline.PostProcess;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class ColorGradingLutPass : ScriptableRenderPass
{
	private static class ShaderConstants
	{
		public static readonly int _Lut_Params = Shader.PropertyToID("_Lut_Params");

		public static readonly int _ColorBalance = Shader.PropertyToID("_ColorBalance");

		public static readonly int _ColorFilter = Shader.PropertyToID("_ColorFilter");

		public static readonly int _ChannelMixerRed = Shader.PropertyToID("_ChannelMixerRed");

		public static readonly int _ChannelMixerGreen = Shader.PropertyToID("_ChannelMixerGreen");

		public static readonly int _ChannelMixerBlue = Shader.PropertyToID("_ChannelMixerBlue");

		public static readonly int _HueSatCon = Shader.PropertyToID("_HueSatCon");

		public static readonly int _Lift = Shader.PropertyToID("_Lift");

		public static readonly int _Gamma = Shader.PropertyToID("_Gamma");

		public static readonly int _Gain = Shader.PropertyToID("_Gain");

		public static readonly int _Slope = Shader.PropertyToID("_Slope");

		public static readonly int _Power = Shader.PropertyToID("_Power");

		public static readonly int _Offset = Shader.PropertyToID("_Offset");

		public static readonly int _Shadows = Shader.PropertyToID("_Shadows");

		public static readonly int _Midtones = Shader.PropertyToID("_Midtones");

		public static readonly int _Highlights = Shader.PropertyToID("_Highlights");

		public static readonly int _ShaHiLimits = Shader.PropertyToID("_ShaHiLimits");

		public static readonly int _SplitShadows = Shader.PropertyToID("_SplitShadows");

		public static readonly int _SplitHighlights = Shader.PropertyToID("_SplitHighlights");

		public static readonly int _CurveMaster = Shader.PropertyToID("_CurveMaster");

		public static readonly int _CurveRed = Shader.PropertyToID("_CurveRed");

		public static readonly int _CurveGreen = Shader.PropertyToID("_CurveGreen");

		public static readonly int _CurveBlue = Shader.PropertyToID("_CurveBlue");

		public static readonly int _CurveHueVsHue = Shader.PropertyToID("_CurveHueVsHue");

		public static readonly int _CurveHueVsSat = Shader.PropertyToID("_CurveHueVsSat");

		public static readonly int _CurveLumVsSat = Shader.PropertyToID("_CurveLumVsSat");

		public static readonly int _CurveSatVsSat = Shader.PropertyToID("_CurveSatVsSat");

		public static readonly int _NeutralTonemapperParams1 = Shader.PropertyToID("_NeutralTonemapperParams1");

		public static readonly int _NeutralTonemapperParams2 = Shader.PropertyToID("_NeutralTonemapperParams2");
	}

	private const string m_ProfilerTag = "Color Grading LUT";

	private readonly Material m_LutBuilderLdr;

	private readonly Material m_LutBuilderHdr;

	private readonly GraphicsFormat m_HdrLutFormat;

	private readonly GraphicsFormat m_LdrLutFormat;

	private RenderTargetHandle m_InternalLut;

	public ColorGradingLutPass(RenderPassEvent evt, PostProcessData data)
	{
		base.RenderPassEvent = evt;
		m_LutBuilderLdr = Load(data.Shaders.LutBuilderLdrPS);
		m_LutBuilderHdr = Load(data.Shaders.LutBuilderHdrPS);
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Blend))
		{
			m_HdrLutFormat = GraphicsFormat.R16G16B16A16_SFloat;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Blend))
		{
			m_HdrLutFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		}
		else
		{
			m_HdrLutFormat = GraphicsFormat.R8G8B8A8_UNorm;
		}
		m_LdrLutFormat = GraphicsFormat.R8G8B8A8_UNorm;
		Material Load(Shader shader)
		{
			if (shader == null)
			{
				Debug.LogErrorFormat("Missing shader. " + GetType().DeclaringType.Name + " render pass will not execute. Check for missing reference in the renderer resources.");
				return null;
			}
			return CoreUtils.CreateEngineMaterial(shader);
		}
	}

	public void Setup(in RenderTargetHandle internalLut)
	{
		m_InternalLut = internalLut;
	}

	public static Vector3 ColorBalanceToLMSCoeffs(float temperature, float tint)
	{
		float num = temperature / 65f;
		float num2 = tint / 65f;
		float x = 0.31271f - num * ((num < 0f) ? 0.1f : 0.05f);
		float y = ColorUtils.StandardIlluminantY(x) + num2 * 0.05f;
		Vector3 vector = new Vector3(0.949237f, 1.03542f, 1.08728f);
		Vector3 vector2 = ColorUtils.CIExyToLMS(x, y);
		return new Vector3(vector.x / vector2.x, vector.y / vector2.y, vector.z / vector2.z);
	}

	public static (Vector4, Vector4, Vector4) PrepareShadowsMidtonesHighlights(in Vector4 inShadows, in Vector4 inMidtones, in Vector4 inHighlights)
	{
		Vector4 item = inShadows;
		item.x = Mathf.GammaToLinearSpace(item.x);
		item.y = Mathf.GammaToLinearSpace(item.y);
		item.z = Mathf.GammaToLinearSpace(item.z);
		float num = item.w * ((Mathf.Sign(item.w) < 0f) ? 1f : 4f);
		item.x = Mathf.Max(item.x + num, 0f);
		item.y = Mathf.Max(item.y + num, 0f);
		item.z = Mathf.Max(item.z + num, 0f);
		item.w = 0f;
		Vector4 item2 = inMidtones;
		item2.x = Mathf.GammaToLinearSpace(item2.x);
		item2.y = Mathf.GammaToLinearSpace(item2.y);
		item2.z = Mathf.GammaToLinearSpace(item2.z);
		num = item2.w * ((Mathf.Sign(item2.w) < 0f) ? 1f : 4f);
		item2.x = Mathf.Max(item2.x + num, 0f);
		item2.y = Mathf.Max(item2.y + num, 0f);
		item2.z = Mathf.Max(item2.z + num, 0f);
		item2.w = 0f;
		Vector4 item3 = inHighlights;
		item3.x = Mathf.GammaToLinearSpace(item3.x);
		item3.y = Mathf.GammaToLinearSpace(item3.y);
		item3.z = Mathf.GammaToLinearSpace(item3.z);
		num = item3.w * ((Mathf.Sign(item3.w) < 0f) ? 1f : 4f);
		item3.x = Mathf.Max(item3.x + num, 0f);
		item3.y = Mathf.Max(item3.y + num, 0f);
		item3.z = Mathf.Max(item3.z + num, 0f);
		item3.w = 0f;
		return (item, item2, item3);
	}

	public static (Vector4, Vector4, Vector4) PrepareLiftGammaGain(in Vector4 inLift, in Vector4 inGamma, in Vector4 inGain)
	{
		Vector4 vector = inLift;
		vector.x = Mathf.GammaToLinearSpace(vector.x) * 0.15f;
		vector.y = Mathf.GammaToLinearSpace(vector.y) * 0.15f;
		vector.z = Mathf.GammaToLinearSpace(vector.z) * 0.15f;
		Color color = vector;
		float num = ColorUtils.Luminance(in color);
		vector.x = vector.x - num + vector.w;
		vector.y = vector.y - num + vector.w;
		vector.z = vector.z - num + vector.w;
		vector.w = 0f;
		Vector4 vector2 = inGamma;
		vector2.x = Mathf.GammaToLinearSpace(vector2.x) * 0.8f;
		vector2.y = Mathf.GammaToLinearSpace(vector2.y) * 0.8f;
		vector2.z = Mathf.GammaToLinearSpace(vector2.z) * 0.8f;
		color = vector2;
		float num2 = ColorUtils.Luminance(in color);
		vector2.w += 1f;
		vector2.x = 1f / Mathf.Max(vector2.x - num2 + vector2.w, 0.001f);
		vector2.y = 1f / Mathf.Max(vector2.y - num2 + vector2.w, 0.001f);
		vector2.z = 1f / Mathf.Max(vector2.z - num2 + vector2.w, 0.001f);
		vector2.w = 0f;
		Vector4 vector3 = inGain;
		vector3.x = Mathf.GammaToLinearSpace(vector3.x) * 0.8f;
		vector3.y = Mathf.GammaToLinearSpace(vector3.y) * 0.8f;
		vector3.z = Mathf.GammaToLinearSpace(vector3.z) * 0.8f;
		color = vector3;
		float num3 = ColorUtils.Luminance(in color);
		vector3.w += 1f;
		vector3.x = vector3.x - num3 + vector3.w;
		vector3.y = vector3.y - num3 + vector3.w;
		vector3.z = vector3.z - num3 + vector3.w;
		vector3.w = 0f;
		return (vector, vector2, vector3);
	}

	private static Vector4 NormalizeColor(Vector4 c)
	{
		float num = (c.x + c.y + c.z) / 3f;
		if (Mathf.Approximately(num, 0f))
		{
			return new Color(1f, 1f, 1f, c.w);
		}
		Color color = default(Color);
		color.r = c.x / num;
		color.g = c.y / num;
		color.b = c.z / num;
		color.a = c.w;
		return color;
	}

	private static Vector4 ClampVector(Vector4 vector, float min, float max)
	{
		return new Vector4(Mathf.Clamp(vector.x, min, max), Mathf.Clamp(vector.y, min, max), Mathf.Clamp(vector.z, min, max), Mathf.Clamp(vector.w, min, max));
	}

	public static (Vector4, Vector4, Vector4) PrepareSlopePowerOffset(Vector4 inSlope, Vector4 inPower, Vector4 inOffset)
	{
		Vector4 vector = NormalizeColor(inSlope);
		float num = (vector.x + vector.y + vector.z) / 3f;
		inSlope.w *= 0.5f;
		Vector4 vector2 = default(Vector4);
		vector2.x = (vector.x - num) * 0.1f + inSlope.w + 1f;
		vector2.y = (vector.y - num) * 0.1f + inSlope.w + 1f;
		vector2.z = (vector.z - num) * 0.1f + inSlope.w + 1f;
		vector2.w = 0f;
		vector2 = ClampVector(vector2, 0f, 2f);
		Vector4 vector3 = NormalizeColor(inPower);
		float num2 = (vector3.x + vector3.y + vector3.z) / 3f;
		inPower.w *= 0.5f;
		float b = (vector3.x - num2) * 0.1f + inPower.w + 1f;
		float b2 = (vector3.y - num2) * 0.1f + inPower.w + 1f;
		float b3 = (vector3.z - num2) * 0.1f + inPower.w + 1f;
		float x = 1f / Mathf.Max(0.01f, b);
		float y = 1f / Mathf.Max(0.01f, b2);
		float z = 1f / Mathf.Max(0.01f, b3);
		Vector4 item = ClampVector(new Vector4(x, y, z, 0f), 0.5f, 2.5f);
		Vector4 vector4 = NormalizeColor(inOffset);
		float num3 = (vector4.x + vector4.y + vector4.z) / 3f;
		inOffset.w *= 0.5f;
		float x2 = (vector4.x - num3) * 0.05f + inOffset.w;
		float y2 = (vector4.y - num3) * 0.05f + inOffset.w;
		float z2 = (vector4.z - num3) * 0.05f + inOffset.w;
		Vector4 item2 = ClampVector(new Vector4(x2, y2, z2, 0f), -0.8f, 0.8f);
		return (vector2, item, item2);
	}

	public static (Vector4, Vector4) PrepareSplitToning(in Vector4 inShadows, in Vector4 inHighlights, float balance)
	{
		Vector4 item = inShadows;
		Vector4 item2 = inHighlights;
		item.w = balance / 100f;
		item2.w = 0f;
		return (item, item2);
	}

	public static (Vector4, Vector4) PrepareNeutralTonemapping(float neutralBlackIn, float neutralWhiteIn, float neutralBlackOut, float neutralWhiteOut, float neutralWhiteLevel, float neutralWhiteClip)
	{
		float num = neutralBlackIn * 20f + 1f;
		float num2 = neutralBlackOut * 10f + 1f;
		float num3 = neutralWhiteIn / 20f;
		float num4 = 1f - neutralWhiteOut / 20f;
		float t = num / num2;
		float t2 = num3 / num4;
		float y = Mathf.Max(0f, Mathf.LerpUnclamped(0.57f, 0.37f, t));
		float z = Mathf.LerpUnclamped(0.01f, 0.24f, t2);
		float w = Mathf.Max(0f, Mathf.LerpUnclamped(0.02f, 0.2f, t));
		Vector4 item = new Vector4(0.2f, y, z, w);
		Vector4 item2 = new Vector4(0.02f, 0.3f, neutralWhiteLevel, neutralWhiteClip / 10f);
		return (item, item2);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get("Color Grading LUT");
		VolumeStack stack = VolumeManager.instance.stack;
		ChannelMixer component = stack.GetComponent<ChannelMixer>();
		ColorAdjustments component2 = stack.GetComponent<ColorAdjustments>();
		ColorCurves component3 = stack.GetComponent<ColorCurves>();
		LiftGammaGain component4 = stack.GetComponent<LiftGammaGain>();
		SlopePowerOffset component5 = stack.GetComponent<SlopePowerOffset>();
		ShadowsMidtonesHighlights component6 = stack.GetComponent<ShadowsMidtonesHighlights>();
		SplitToning component7 = stack.GetComponent<SplitToning>();
		Tonemapping component8 = stack.GetComponent<Tonemapping>();
		WhiteBalance component9 = stack.GetComponent<WhiteBalance>();
		ref PostProcessingData postProcessingData = ref renderingData.PostProcessingData;
		bool flag = postProcessingData.GradingMode == ColorGradingMode.HighDynamicRange;
		int lutSize = postProcessingData.LutSize;
		int num = lutSize * lutSize;
		GraphicsFormat colorFormat = (flag ? m_HdrLutFormat : m_LdrLutFormat);
		Material material = (flag ? m_LutBuilderHdr : m_LutBuilderLdr);
		RenderTextureDescriptor desc = new RenderTextureDescriptor(num, lutSize, colorFormat, 0);
		desc.vrUsage = VRTextureUsage.None;
		commandBuffer.GetTemporaryRT(m_InternalLut.Id, desc, FilterMode.Bilinear);
		Vector3 vector = ColorBalanceToLMSCoeffs(component9.temperature.value, component9.tint.value);
		Vector4 value = new Vector4(component2.hueShift.value / 360f, component2.saturation.value / 100f + 1f, component2.contrast.value / 100f + 1f, 0f);
		Vector4 value2 = new Vector4(component.redOutRedIn.value / 100f, component.redOutGreenIn.value / 100f, component.redOutBlueIn.value / 100f, 0f);
		Vector4 value3 = new Vector4(component.greenOutRedIn.value / 100f, component.greenOutGreenIn.value / 100f, component.greenOutBlueIn.value / 100f, 0f);
		Vector4 value4 = new Vector4(component.blueOutRedIn.value / 100f, component.blueOutGreenIn.value / 100f, component.blueOutBlueIn.value / 100f, 0f);
		Vector4 value5 = new Vector4(component6.shadowsStart.value, component6.shadowsEnd.value, component6.highlightsStart.value, component6.highlightsEnd.value);
		Vector4 inShadows = component6.shadows.value;
		Vector4 inMidtones = component6.midtones.value;
		Vector4 inHighlights = component6.highlights.value;
		(Vector4, Vector4, Vector4) tuple = PrepareShadowsMidtonesHighlights(in inShadows, in inMidtones, in inHighlights);
		Vector4 item = tuple.Item1;
		Vector4 item2 = tuple.Item2;
		Vector4 item3 = tuple.Item3;
		inShadows = component4.lift.value;
		inMidtones = component4.gamma.value;
		inHighlights = component4.gain.value;
		(Vector4, Vector4, Vector4) tuple2 = PrepareLiftGammaGain(in inShadows, in inMidtones, in inHighlights);
		Vector4 item4 = tuple2.Item1;
		Vector4 item5 = tuple2.Item2;
		Vector4 item6 = tuple2.Item3;
		(Vector4, Vector4, Vector4) tuple3 = PrepareSlopePowerOffset(component5.slope.value, component5.power.value, component5.offset.value);
		Vector4 item7 = tuple3.Item1;
		Vector4 item8 = tuple3.Item2;
		Vector4 item9 = tuple3.Item3;
		inShadows = component7.shadows.value;
		inMidtones = component7.highlights.value;
		var (value6, value7) = PrepareSplitToning(in inShadows, in inMidtones, component7.balance.value);
		material.SetVector(value: new Vector4(lutSize, 0.5f / (float)num, 0.5f / (float)lutSize, (float)lutSize / ((float)lutSize - 1f)), nameID: ShaderConstants._Lut_Params);
		material.SetVector(ShaderConstants._ColorBalance, vector);
		material.SetVector(ShaderConstants._ColorFilter, component2.colorFilter.value.linear);
		material.SetVector(ShaderConstants._ChannelMixerRed, value2);
		material.SetVector(ShaderConstants._ChannelMixerGreen, value3);
		material.SetVector(ShaderConstants._ChannelMixerBlue, value4);
		material.SetVector(ShaderConstants._HueSatCon, value);
		material.SetVector(ShaderConstants._Shadows, item);
		material.SetVector(ShaderConstants._Midtones, item2);
		material.SetVector(ShaderConstants._Highlights, item3);
		material.SetVector(ShaderConstants._ShaHiLimits, value5);
		material.SetVector(ShaderConstants._SplitShadows, value6);
		material.SetVector(ShaderConstants._SplitHighlights, value7);
		material.SetVector(ShaderConstants._Slope, item7);
		material.SetVector(ShaderConstants._Power, item8);
		material.SetVector(ShaderConstants._Offset, item9);
		material.SetVector(ShaderConstants._Lift, item4);
		material.SetVector(ShaderConstants._Gamma, item5);
		material.SetVector(ShaderConstants._Gain, item6);
		material.SetTexture(ShaderConstants._CurveMaster, component3.master.value.GetTexture());
		material.SetTexture(ShaderConstants._CurveRed, component3.red.value.GetTexture());
		material.SetTexture(ShaderConstants._CurveGreen, component3.green.value.GetTexture());
		material.SetTexture(ShaderConstants._CurveBlue, component3.blue.value.GetTexture());
		material.SetTexture(ShaderConstants._CurveHueVsHue, component3.hueVsHue.value.GetTexture());
		material.SetTexture(ShaderConstants._CurveHueVsSat, component3.hueVsSat.value.GetTexture());
		material.SetTexture(ShaderConstants._CurveLumVsSat, component3.lumVsSat.value.GetTexture());
		material.SetTexture(ShaderConstants._CurveSatVsSat, component3.satVsSat.value.GetTexture());
		if (flag)
		{
			material.shaderKeywords = null;
			switch (component8.mode.value)
			{
			case TonemappingMode.Neutral:
				material.EnableKeyword(ShaderKeywordStrings.TonemapNeutral);
				var (value9, value10) = PrepareNeutralTonemapping(component8.neutralBlackIn.value, component8.neutralWhiteIn.value, component8.neutralBlackOut.value, component8.neutralWhiteOut.value, component8.neutralWhiteLevel.value, component8.neutralWhiteClip.value);
				material.SetVector(ShaderConstants._NeutralTonemapperParams1, value9);
				material.SetVector(ShaderConstants._NeutralTonemapperParams2, value10);
				break;
			case TonemappingMode.ACES:
				material.EnableKeyword(ShaderKeywordStrings.TonemapACES);
				break;
			}
		}
		Blit(commandBuffer, m_InternalLut.Id, m_InternalLut.Id, material);
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		cmd.ReleaseTemporaryRT(m_InternalLut.Id);
	}
}
