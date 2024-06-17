using UnityEngine;

namespace Owlcat.Runtime.Core.Utility;

public static class AnimationCurveExtensions
{
	public static float GetDuration(this AnimationCurve curve)
	{
		if (curve.length == 0)
		{
			return 0f;
		}
		return curve[curve.length - 1].time;
	}

	public static float EvaluateNormalized(this AnimationCurve curve, float t)
	{
		float num = Mathf.Clamp01(t);
		if (curve.length == 0)
		{
			return 0f;
		}
		return curve.Evaluate(curve.GetDuration() * num);
	}
}
