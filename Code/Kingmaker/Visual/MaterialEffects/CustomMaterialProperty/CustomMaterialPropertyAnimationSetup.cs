using System;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.CustomMaterialProperty;

internal sealed class CustomMaterialPropertyAnimationSetup : MonoBehaviour
{
	[Serializable]
	internal sealed class Clip
	{
		public string PropertyName;

		public AnimationCurve Curve = AnimationCurve.Constant(0f, 1f, 1f);

		[Min(0f)]
		public float CurveDuration = 1f;

		[Min(0f)]
		public float Duration = 1f;

		public float Delay;

		public int Priority;
	}

	public Clip[] Clips;
}
