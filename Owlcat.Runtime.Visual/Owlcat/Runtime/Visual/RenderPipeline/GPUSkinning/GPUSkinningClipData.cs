using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.GPUSkinning;

[Serializable]
public class GPUSkinningClipData
{
	public GameObject BoneHierarchyPrefab;

	public int FrameStride;

	public int FrameCount;

	public float FrameDuration;

	public float Duration;

	public Matrix4x4[] Frames;

	public int RuntimeBufferOffset { get; internal set; }

	public Vector4 GetGPUSkinningClipParams()
	{
		return new Vector4(RuntimeBufferOffset, FrameStride, FrameCount - 1, FrameDuration);
	}
}
