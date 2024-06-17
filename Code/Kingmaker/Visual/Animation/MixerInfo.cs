using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation;

public class MixerInfo
{
	private List<PlayableInfo> m_PlayableInfos = new List<PlayableInfo>();

	private List<PlayableInfo> m_PlayableInfosCache = new List<PlayableInfo>();

	private PlayableGraph m_Graph;

	public readonly AnimationLayerMixerPlayable Mixer;

	public readonly AvatarMask AvatarMask;

	public readonly int ActiveTransformCount;

	public readonly bool IsAdditive;

	public IEnumerable<PlayableInfo> Playables => m_PlayableInfos;

	public MixerInfo(AnimationLayerMixerPlayable mixer, AvatarMask avatarMask = null, int activeTransformCount = 0, bool isAdditive = false)
	{
		Mixer = mixer;
		AvatarMask = avatarMask;
		ActiveTransformCount = activeTransformCount;
		IsAdditive = isAdditive;
		m_Graph = Mixer.GetGraph();
	}

	[CanBeNull]
	public PlayableInfo AddPlayableFromCache(AnimationActionHandle handle, Func<PlayableInfo, bool> filter)
	{
		if (!handle.Action.SupportCaching)
		{
			return null;
		}
		foreach (PlayableInfo item in m_PlayableInfosCache)
		{
			if (filter(item))
			{
				m_PlayableInfosCache.Remove(item);
				m_PlayableInfos.Add(item);
				item.Reset(handle);
				return item;
			}
		}
		return null;
	}

	public PlayableInfo AddPlayable(AnimationActionHandle handle, Playable playable, IEnumerable<AnimationClipEvent> events = null)
	{
		int inputCount = Mixer.GetInputCount();
		int num = -1;
		for (int i = 0; i < inputCount; i++)
		{
			if (Mixer.GetInput(i).Equals(Playable.Null))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			Mixer.SetInputCount(inputCount + 1);
			num = inputCount;
		}
		Mixer.ConnectInput(num, playable, 0);
		Mixer.SetInputWeight(num, 0f);
		PlayableInfo playableInfo = new PlayableInfo(handle, playable, events, this, num);
		if (AvatarMask != null)
		{
			Mixer.SetLayerMaskFromAvatarMask((uint)num, AvatarMask);
		}
		Mixer.SetLayerAdditive((uint)num, IsAdditive);
		m_PlayableInfos.Add(playableInfo);
		return playableInfo;
	}

	public void RemovePlayable(PlayableInfo playableInfo)
	{
		if (playableInfo.MixerInfo == this)
		{
			m_PlayableInfos.Remove(playableInfo);
			if (playableInfo.Handle.Action.SupportCaching)
			{
				Mixer.SetInputWeight(playableInfo.InputIndex, 0f);
				m_PlayableInfosCache.Add(playableInfo);
			}
			else
			{
				m_Graph.Disconnect(Mixer, playableInfo.InputIndex);
				m_Graph.DestroySubgraph(playableInfo.Playable);
			}
		}
		else
		{
			UnityEngine.Debug.LogError("You try to remove playable from mixer which not connected.");
		}
	}

	public void NormalizeWeights()
	{
		for (int i = 0; i < m_PlayableInfos.Count; i++)
		{
			PlayableInfo playableInfo = m_PlayableInfos[i];
			float num = playableInfo.GetAdjustedWeight();
			if (num > 0f)
			{
				for (int j = 0; j < m_PlayableInfos.Count; j++)
				{
					if (m_PlayableInfos[j].InputIndex > playableInfo.InputIndex)
					{
						num += m_PlayableInfos[j].GetAdjustedWeight();
					}
				}
			}
			Mixer.SetInputWeight(playableInfo.InputIndex, num);
		}
	}
}
