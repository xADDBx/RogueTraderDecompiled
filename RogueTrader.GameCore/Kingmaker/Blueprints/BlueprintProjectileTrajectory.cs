using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints;

[TypeId("55a39726d0cce8a45877d1ffb0c3e5e6")]
public class BlueprintProjectileTrajectory : BlueprintScriptableObject
{
	public Vector3 UpDirection = Vector3.up;

	[Space(16f)]
	public TrajectoryOffset[] PlaneOffset;

	[Space(16f)]
	public TrajectoryOffset[] UpOffset;

	[Space(16f)]
	public AnimationCurve AmplitudeScaleByLifetime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public AnimationCurve AmplitudeScaleByFullDistance = AnimationCurve.Linear(0f, 1f, 1f, 1f);
}
