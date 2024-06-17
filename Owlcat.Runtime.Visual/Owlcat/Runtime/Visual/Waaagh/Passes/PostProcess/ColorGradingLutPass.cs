using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public class ColorGradingLutPass : ScriptableRenderPass<ColorGradingLutPassData>
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

	private readonly Material m_LutBuilderLdr;

	private readonly Material m_LutBuilderHdr;

	private GraphicsFormat m_HdrLutFormat;

	private GraphicsFormat m_LdrLutFormat;

	public override string Name => "ColorGradingLutPass";

	public ColorGradingLutPass(RenderPassEvent evt, PostProcessData data)
		: base(evt)
	{
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
				UnityEngine.Debug.LogErrorFormat("Missing shader. " + GetType().DeclaringType.Name + " render pass will not execute. Check for missing reference in the renderer resources.");
				return null;
			}
			return CoreUtils.CreateEngineMaterial(shader);
		}
	}

	protected override void Setup(RenderGraphBuilder builder, ColorGradingLutPassData data, ref RenderingData renderingData)
	{
		ref PostProcessingData postProcessingData = ref renderingData.PostProcessingData;
		bool flag = postProcessingData.GradingMode == ColorGradingMode.HighDynamicRange;
		int lutSize = postProcessingData.LutSize;
		int num = lutSize * lutSize;
		GraphicsFormat colorFormat = (flag ? m_HdrLutFormat : m_LdrLutFormat);
		TextureDesc desc = new TextureDesc(num, lutSize);
		desc.name = "ColorGradingLut";
		desc.colorFormat = colorFormat;
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Bilinear;
		data.Resources.ColorGradingLut = renderingData.RenderGraph.CreateTexture(in desc);
		data.Output = builder.WriteTexture(in data.Resources.ColorGradingLut);
		data.LutWidth = num;
		data.LutHeight = lutSize;
		data.IsHdr = flag;
		VolumeStack stack = VolumeManager.instance.stack;
		data.ChannelMixer = stack.GetComponent<ChannelMixer>();
		data.ColorAdjustments = stack.GetComponent<ColorAdjustments>();
		data.ColorCurves = stack.GetComponent<ColorCurves>();
		data.LiftGammaGain = stack.GetComponent<LiftGammaGain>();
		data.SlopePowerOffset = stack.GetComponent<SlopePowerOffset>();
		data.ShadowsMidtonesHighlights = stack.GetComponent<ShadowsMidtonesHighlights>();
		data.SplitToning = stack.GetComponent<SplitToning>();
		data.Tonemapping = stack.GetComponent<Tonemapping>();
		data.WhiteBalance = stack.GetComponent<WhiteBalance>();
		data.Material = (flag ? m_LutBuilderHdr : m_LutBuilderLdr);
	}

	protected override void Render(ColorGradingLutPassData data, RenderGraphContext context)
	{
		Vector3 vector = ColorBalanceToLMSCoeffs(data.WhiteBalance.temperature.value, data.WhiteBalance.tint.value);
		Vector4 value = new Vector4(data.ColorAdjustments.hueShift.value / 360f, data.ColorAdjustments.saturation.value / 100f + 1f, data.ColorAdjustments.contrast.value / 100f + 1f, 0f);
		Vector4 value2 = new Vector4(data.ChannelMixer.redOutRedIn.value / 100f, data.ChannelMixer.redOutGreenIn.value / 100f, data.ChannelMixer.redOutBlueIn.value / 100f, 0f);
		Vector4 value3 = new Vector4(data.ChannelMixer.greenOutRedIn.value / 100f, data.ChannelMixer.greenOutGreenIn.value / 100f, data.ChannelMixer.greenOutBlueIn.value / 100f, 0f);
		Vector4 value4 = new Vector4(data.ChannelMixer.blueOutRedIn.value / 100f, data.ChannelMixer.blueOutGreenIn.value / 100f, data.ChannelMixer.blueOutBlueIn.value / 100f, 0f);
		Vector4 value5 = new Vector4(data.ShadowsMidtonesHighlights.shadowsStart.value, data.ShadowsMidtonesHighlights.shadowsEnd.value, data.ShadowsMidtonesHighlights.highlightsStart.value, data.ShadowsMidtonesHighlights.highlightsEnd.value);
		Vector4 inShadows = data.ShadowsMidtonesHighlights.shadows.value;
		Vector4 inMidtones = data.ShadowsMidtonesHighlights.midtones.value;
		Vector4 inHighlights = data.ShadowsMidtonesHighlights.highlights.value;
		(Vector4, Vector4, Vector4) tuple = PrepareShadowsMidtonesHighlights(in inShadows, in inMidtones, in inHighlights);
		Vector4 item = tuple.Item1;
		Vector4 item2 = tuple.Item2;
		Vector4 item3 = tuple.Item3;
		inShadows = data.LiftGammaGain.lift.value;
		inMidtones = data.LiftGammaGain.gamma.value;
		inHighlights = data.LiftGammaGain.gain.value;
		(Vector4, Vector4, Vector4) tuple2 = PrepareLiftGammaGain(in inShadows, in inMidtones, in inHighlights);
		Vector4 item4 = tuple2.Item1;
		Vector4 item5 = tuple2.Item2;
		Vector4 item6 = tuple2.Item3;
		(Vector4, Vector4, Vector4) tuple3 = PrepareSlopePowerOffset(data.SlopePowerOffset.slope.value, data.SlopePowerOffset.power.value, data.SlopePowerOffset.offset.value);
		Vector4 item7 = tuple3.Item1;
		Vector4 item8 = tuple3.Item2;
		Vector4 item9 = tuple3.Item3;
		inShadows = data.SplitToning.shadows.value;
		inMidtones = data.SplitToning.highlights.value;
		(Vector4, Vector4) tuple4 = PrepareSplitToning(in inShadows, in inMidtones, data.SplitToning.balance.value);
		Vector4 item10 = tuple4.Item1;
		Vector4 item11 = tuple4.Item2;
		int lutWidth = data.LutWidth;
		int lutHeight = data.LutHeight;
		Vector4 value6 = new Vector4(lutHeight, 0.5f / (float)lutWidth, 0.5f / (float)lutHeight, (float)lutHeight / ((float)lutHeight - 1f));
		data.Material.SetVector(ShaderConstants._Lut_Params, value6);
		data.Material.SetVector(ShaderConstants._ColorBalance, vector);
		data.Material.SetVector(ShaderConstants._ColorFilter, data.ColorAdjustments.colorFilter.value.linear);
		data.Material.SetVector(ShaderConstants._ChannelMixerRed, value2);
		data.Material.SetVector(ShaderConstants._ChannelMixerGreen, value3);
		data.Material.SetVector(ShaderConstants._ChannelMixerBlue, value4);
		data.Material.SetVector(ShaderConstants._HueSatCon, value);
		data.Material.SetVector(ShaderConstants._Shadows, item);
		data.Material.SetVector(ShaderConstants._Midtones, item2);
		data.Material.SetVector(ShaderConstants._Highlights, item3);
		data.Material.SetVector(ShaderConstants._ShaHiLimits, value5);
		data.Material.SetVector(ShaderConstants._SplitShadows, item10);
		data.Material.SetVector(ShaderConstants._SplitHighlights, item11);
		data.Material.SetVector(ShaderConstants._Slope, item7);
		data.Material.SetVector(ShaderConstants._Power, item8);
		data.Material.SetVector(ShaderConstants._Offset, item9);
		data.Material.SetVector(ShaderConstants._Lift, item4);
		data.Material.SetVector(ShaderConstants._Gamma, item5);
		data.Material.SetVector(ShaderConstants._Gain, item6);
		data.Material.SetTexture(ShaderConstants._CurveMaster, data.ColorCurves.master.value.GetTexture());
		data.Material.SetTexture(ShaderConstants._CurveRed, data.ColorCurves.red.value.GetTexture());
		data.Material.SetTexture(ShaderConstants._CurveGreen, data.ColorCurves.green.value.GetTexture());
		data.Material.SetTexture(ShaderConstants._CurveBlue, data.ColorCurves.blue.value.GetTexture());
		data.Material.SetTexture(ShaderConstants._CurveHueVsHue, data.ColorCurves.hueVsHue.value.GetTexture());
		data.Material.SetTexture(ShaderConstants._CurveHueVsSat, data.ColorCurves.hueVsSat.value.GetTexture());
		data.Material.SetTexture(ShaderConstants._CurveLumVsSat, data.ColorCurves.lumVsSat.value.GetTexture());
		data.Material.SetTexture(ShaderConstants._CurveSatVsSat, data.ColorCurves.satVsSat.value.GetTexture());
		if (data.IsHdr)
		{
			data.Material.shaderKeywords = null;
			switch (data.Tonemapping.mode.value)
			{
			case TonemappingMode.Neutral:
				data.Material.EnableKeyword(ShaderKeywordStrings.TonemapNeutral);
				var (value7, value8) = PrepareNeutralTonemapping(data.Tonemapping.neutralBlackIn.value, data.Tonemapping.neutralWhiteIn.value, data.Tonemapping.neutralBlackOut.value, data.Tonemapping.neutralWhiteOut.value, data.Tonemapping.neutralWhiteLevel.value, data.Tonemapping.neutralWhiteClip.value);
				data.Material.SetVector(ShaderConstants._NeutralTonemapperParams1, value7);
				data.Material.SetVector(ShaderConstants._NeutralTonemapperParams2, value8);
				break;
			case TonemappingMode.ACES:
				data.Material.EnableKeyword(ShaderKeywordStrings.TonemapACES);
				break;
			}
		}
		context.cmd.SetRenderTarget(data.Output);
		context.cmd.Blit(data.Output, data.Output, data.Material);
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

	public void Cleanup()
	{
		CoreUtils.Destroy(m_LutBuilderLdr);
		CoreUtils.Destroy(m_LutBuilderHdr);
	}
}
