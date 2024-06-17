using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Animation.CustomIdleComponents;

public class CustomIdleAnimationMonoComponent : MonoBehaviour
{
	public CustomIdleAnimationPreset CustomIdleAnimationPreset;

	public List<AnimationClipWrapper> IdleClips;
}
