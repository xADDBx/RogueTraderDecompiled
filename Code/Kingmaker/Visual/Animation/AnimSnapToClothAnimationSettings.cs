using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[CreateAssetMenu(menuName = "Character System/AnimSnapToClothAnimationSettings")]
public class AnimSnapToClothAnimationSettings : ScriptableObject
{
	[Serializable]
	public class ClipsPlaySimpleAnim
	{
		public AnimationClip Clip;

		public float Speed = 1f;
	}

	[Serializable]
	public class ClipsPlaySpecialAnim
	{
		public AnimationClip Clip;

		public string Trigger;

		public float Speed = 1f;
	}

	public ClipsPlaySimpleAnim[] ClipsWhichPlaySimpleAnimationLikeIdle;

	public ClipsPlaySpecialAnim[] ClipsWhichPlaySpecialAnimation;
}
