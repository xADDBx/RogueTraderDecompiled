using System;

namespace Kingmaker.Settings.Graphics;

[Serializable]
public class GraphicsPreset : IComparable<GraphicsPreset>
{
	public QualityPresetOption GraphicsQuality;

	public VSyncModeOptions VSyncMode;

	public bool FrameRateLimitEnabled;

	public int FrameRateLimit;

	public QualityOptionDisactivatable ShadowsQuality;

	public QualityOption TexturesQuality;

	public bool DepthOfField;

	public bool Bloom;

	public QualityOptionDisactivatable SSAOQuality;

	public QualityOptionDisactivatable SSRQuality;

	public AntialiasingMode AntialiasingMode;

	public QualityOption AntialiasingQuality;

	public PositionBasedDynamicsExecutionPath PBDExecutionPath;

	public FootprintsMode FootprintsMode;

	public FsrMode FsrMode;

	public float FsrSharpness = 0.92f;

	public QualityOption VolumetricLightingQuality;

	public bool ParticleSystemsLightingEnabled = true;

	public bool ParticleSystemsShadowsEnabled;

	public bool FilmGrainEnabled;

	public float UIFrequentTimerInterval;

	public float UIInfrequentTimerInterval;

	public CrowdQualityOptions CrowdQuality;

	public int CompareTo(GraphicsPreset other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		int num = VSyncMode.CompareTo(other.VSyncMode);
		if (num != 0)
		{
			return num;
		}
		int num2 = FrameRateLimitEnabled.CompareTo(other.FrameRateLimitEnabled);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = FrameRateLimit.CompareTo(other.FrameRateLimit);
		if (num3 != 0)
		{
			return num3;
		}
		int num4 = ShadowsQuality.CompareTo(other.ShadowsQuality);
		if (num4 != 0)
		{
			return num4;
		}
		int num5 = TexturesQuality.CompareTo(other.TexturesQuality);
		if (num5 != 0)
		{
			return num5;
		}
		int num6 = DepthOfField.CompareTo(other.DepthOfField);
		if (num6 != 0)
		{
			return num6;
		}
		int num7 = Bloom.CompareTo(other.Bloom);
		if (num7 != 0)
		{
			return num7;
		}
		int num8 = SSAOQuality.CompareTo(other.SSAOQuality);
		if (num8 != 0)
		{
			return num8;
		}
		int num9 = SSRQuality.CompareTo(other.SSRQuality);
		if (num9 != 0)
		{
			return num9;
		}
		int num10 = AntialiasingMode.CompareTo(other.AntialiasingMode);
		if (num10 != 0)
		{
			return num10;
		}
		int num11 = AntialiasingQuality.CompareTo(other.AntialiasingQuality);
		if (num11 != 0)
		{
			return num11;
		}
		int num12 = PBDExecutionPath.CompareTo(other.PBDExecutionPath);
		if (num12 != 0)
		{
			return num12;
		}
		int num13 = FootprintsMode.CompareTo(other.FootprintsMode);
		if (num13 != 0)
		{
			return num13;
		}
		int num14 = FsrMode.CompareTo(other.FsrMode);
		if (num14 != 0)
		{
			return num14;
		}
		int num15 = FsrSharpness.CompareTo(other.FsrSharpness);
		if (num15 != 0)
		{
			return num15;
		}
		int num16 = VolumetricLightingQuality.CompareTo(other.VolumetricLightingQuality);
		if (num16 != 0)
		{
			return num16;
		}
		int num17 = ParticleSystemsLightingEnabled.CompareTo(other.ParticleSystemsLightingEnabled);
		if (num17 != 0)
		{
			return num17;
		}
		int num18 = ParticleSystemsShadowsEnabled.CompareTo(other.ParticleSystemsShadowsEnabled);
		if (num18 != 0)
		{
			return num18;
		}
		int num19 = FilmGrainEnabled.CompareTo(other.FilmGrainEnabled);
		if (num19 != 0)
		{
			return num19;
		}
		int num20 = UIFrequentTimerInterval.CompareTo(other.UIFrequentTimerInterval);
		if (num20 != 0)
		{
			return num20;
		}
		int num21 = CrowdQuality.CompareTo(other.CrowdQuality);
		if (num21 != 0)
		{
			return num21;
		}
		return UIInfrequentTimerInterval.CompareTo(other.UIInfrequentTimerInterval);
	}
}
