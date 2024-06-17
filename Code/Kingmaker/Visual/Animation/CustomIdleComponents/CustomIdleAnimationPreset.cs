using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Animation.CustomIdleComponents;

[Serializable]
public class CustomIdleAnimationPreset : ScriptableObject
{
	public List<AnimationClipWrapper> IdleClips;
}
