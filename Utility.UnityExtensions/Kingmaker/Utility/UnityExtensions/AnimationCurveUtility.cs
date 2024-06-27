using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public static class AnimationCurveUtility
{
	public static readonly AnimationCurve LinearAnimationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 0f, 0f))
	{
		postWrapMode = WrapMode.Once
	};
}
