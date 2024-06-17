using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUSkinning;

[Serializable]
public class GPUAnimationClip
{
	public string Name;

	public int FrameCount;

	[NonSerialized]
	[HideInInspector]
	public SkinningMatrix[] Bones;
}
