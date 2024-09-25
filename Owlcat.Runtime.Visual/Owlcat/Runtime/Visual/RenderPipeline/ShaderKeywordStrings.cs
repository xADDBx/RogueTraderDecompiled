namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class ShaderKeywordStrings
{
	public static readonly string _DEPTH_NO_MSAA = "_DEPTH_NO_MSAA";

	public static readonly string _DEPTH_MSAA_2 = "_DEPTH_MSAA_2";

	public static readonly string _DEPTH_MSAA_4 = "_DEPTH_MSAA_4";

	public static readonly string DEFERRED_ON = "DEFERRED_ON";

	public static readonly string _LINEAR_TO_SRGB_CONVERSION = "_LINEAR_TO_SRGB_CONVERSION";

	public static readonly string DEBUG_DISPLAY = "DEBUG_DISPLAY";

	public static readonly string SHADOWS_SHADOWMASK = "SHADOWS_SHADOWMASK";

	public static readonly string GEOMETRY_CLIP = "GEOMETRY_CLIP";

	public static readonly string SHADOWS_HARD = "SHADOWS_HARD";

	public static readonly string SHADOWS_SOFT = "SHADOWS_SOFT";

	public static readonly string _TRIPLANAR = "_TRIPLANAR";

	public static readonly string SmaaLow = "_SMAA_PRESET_LOW";

	public static readonly string SmaaMedium = "_SMAA_PRESET_MEDIUM";

	public static readonly string SmaaHigh = "_SMAA_PRESET_HIGH";

	public static readonly string PaniniGeneric = "_GENERIC";

	public static readonly string PaniniUnitDistance = "_UNIT_DISTANCE";

	public static readonly string BloomLQ = "_BLOOM_LQ";

	public static readonly string BloomHQ = "_BLOOM_HQ";

	public static readonly string BloomLQDirt = "_BLOOM_LQ_DIRT";

	public static readonly string BloomHQDirt = "_BLOOM_HQ_DIRT";

	public static readonly string ANTI_FLICKER = "ANTI_FLICKER";

	public static readonly string UseRGBM = "_USE_RGBM";

	public static readonly string Distortion = "_DISTORTION";

	public static readonly string ChromaticAberration = "_CHROMATIC_ABERRATION";

	public static readonly string HDRGrading = "_HDR_GRADING";

	public static readonly string TonemapACES = "_TONEMAP_ACES";

	public static readonly string TonemapNeutral = "_TONEMAP_NEUTRAL";

	public static readonly string FilmGrain = "_FILM_GRAIN";

	public static readonly string Fxaa = "_FXAA";

	public static readonly string Dithering = "_DITHERING";

	public static readonly string HighQualitySampling = "_HIGH_QUALITY_SAMPLING";
}
