using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Screen Space Reflections")]
public class ScreenSpaceReflections : VolumeComponent, IPostProcessComponent
{
	public ScreenSpaceReflectionsQualityParameter Quality = new ScreenSpaceReflectionsQualityParameter(ScreenSpaceReflectionsQuality.None);

	public ColorPrecisionParameter ColorPrecision = new ColorPrecisionParameter(Owlcat.Runtime.Visual.Overrides.ColorPrecision.Color32);

	[HideInInspector]
	public TracingMethodParameter TracingMethod = new TracingMethodParameter(Owlcat.Runtime.Visual.Overrides.TracingMethod.HiZ);

	public ClampedIntParameter MaxRaySteps = new ClampedIntParameter(32, 0, 512);

	public ClampedFloatParameter MaxDistance = new ClampedFloatParameter(50f, 0f, 1000f);

	[HideInInspector]
	public ClampedIntParameter ScreenSpaceStepSize = new ClampedIntParameter(5, 1, 16);

	public ClampedFloatParameter MaxRoughness = new ClampedFloatParameter(1f, 0f, 1f);

	public ClampedFloatParameter RoughnessFadeStart = new ClampedFloatParameter(0f, 0f, 1f);

	public ClampedFloatParameter ObjectThickness = new ClampedFloatParameter(0.001f, 0.001f, 1f);

	public ClampedFloatParameter FresnelPower = new ClampedFloatParameter(2f, 1f, 4f);

	[HideInInspector]
	public ClampedFloatParameter ConfidenceScale = new ClampedFloatParameter(1f, 1f, 10f);

	public BoolParameter UseUpsamplePass = new BoolParameter(value: true);

	[HideInInspector]
	public BoolParameter HighlightSupression = new BoolParameter(value: true);

	public BoolParameter UseMotionVectorsForReprojection = new BoolParameter(value: true);

	public BoolParameter BlurEnabled = new BoolParameter(value: true);

	public FloatRangeParameter RoughnessRemap = new FloatRangeParameter(new Vector2(0f, 1f), 0f, 1f);

	public BoolParameter StochasticSSR = new BoolParameter(value: false);

	public ClampedFloatParameter ScreenFadeDistance = new ClampedFloatParameter(0.1f, 0f, 1f);

	public ClampedFloatParameter HistoryInfluence = new ClampedFloatParameter(0.95f, 0f, 1f);

	public ClampedFloatParameter SpeedRejectionScalerFactor = new ClampedFloatParameter(0.2f, 0.001f, 1f);

	public FloatParameter SpeedRejectionParam = new ClampedFloatParameter(0.5f, 0f, 1f);

	public bool IsActive()
	{
		return Quality.value != ScreenSpaceReflectionsQuality.None;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
