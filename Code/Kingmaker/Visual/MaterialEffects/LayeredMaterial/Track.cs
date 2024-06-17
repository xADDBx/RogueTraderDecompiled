using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class Track
{
	private struct AnimationClipData
	{
		public IAnimationClip clip;

		public float beginTime;

		public float endTime;
	}

	private static readonly Stack<Track> s_Pool = new Stack<Track>();

	public int token;

	public Material material;

	public RendererType rendererTypeMask;

	public int priority;

	public float beginTime;

	public float endTime;

	private readonly List<AnimationClipData> m_AnimationClips = new List<AnimationClipData>();

	public static Track Get()
	{
		if (!s_Pool.TryPop(out var result))
		{
			return new Track();
		}
		return result;
	}

	public void AddAnimationClip(IAnimationClip clip, float delay, float duration)
	{
		float num = beginTime + delay;
		m_AnimationClips.Add(new AnimationClipData
		{
			clip = clip,
			beginTime = num,
			endTime = ((duration > 0f) ? (num + duration) : float.PositiveInfinity)
		});
	}

	public void Sample(in PropertyBlock properties, float time)
	{
		foreach (AnimationClipData animationClip in m_AnimationClips)
		{
			if (!(time < animationClip.beginTime) && !(time > animationClip.endTime))
			{
				animationClip.clip.Sample(in properties, time - animationClip.beginTime);
			}
		}
	}

	public void Recycle()
	{
		material = null;
		rendererTypeMask = (RendererType)0;
		token = 0;
		priority = 0;
		beginTime = 0f;
		endTime = 0f;
		m_AnimationClips.Clear();
		s_Pool.Push(this);
	}
}
