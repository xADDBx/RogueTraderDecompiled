using Kingmaker.Blueprints;
using UnityEngine;

namespace Kingmaker.Utility;

public static class TrajectoryCalculator
{
	public static Vector3 CalculateShift(BlueprintProjectileTrajectory trajectory, Vector3 targetDirection, float fullDistance, float passedDistance, float progress, float time, bool invertUpDirection)
	{
		float num = CalculateShift(trajectory.PlaneOffset, passedDistance, progress, time);
		float num2 = CalculateShift(trajectory.UpOffset, passedDistance, progress, time);
		Vector3 normalized = Vector3.Cross(targetDirection, trajectory.UpDirection).normalized;
		float num3 = trajectory.AmplitudeScaleByLifetime.Evaluate(progress);
		float num4 = trajectory.AmplitudeScaleByFullDistance.Evaluate(fullDistance);
		Vector3 normalized2 = trajectory.UpDirection.normalized;
		if (invertUpDirection)
		{
			normalized2 = (Vector3.one - normalized2).normalized;
		}
		return (normalized * num + normalized2 * num2) * (num3 * num4);
	}

	private static float CalculateShift(TrajectoryOffset[] offsets, float passedDistance, float progress, float time)
	{
		float num = 0f;
		foreach (TrajectoryOffset trajectoryOffset in offsets)
		{
			num += CalculateShift(trajectoryOffset.Curve, trajectoryOffset.AmplitudeScale, trajectoryOffset.FrequencyScale, trajectoryOffset.OnInitializeStaticOffset, passedDistance, progress, (0f - time) * trajectoryOffset.ScrollSpeed);
		}
		return num;
	}

	private static float CalculateShift(AnimationCurve curve, float amplitudeScale, float frequencyScale, float randomOffset, float passedDistance, float progress, float shift)
	{
		float time = ((curve.postWrapMode == WrapMode.Loop || curve.postWrapMode == WrapMode.PingPong) ? ((passedDistance + shift + randomOffset) / frequencyScale) : progress);
		return amplitudeScale * curve.Evaluate(time);
	}
}
