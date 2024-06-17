using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Particles.SnapController;

internal sealed class PlaybackState
{
	private static readonly Stack<PlaybackState> s_Pool = new Stack<PlaybackState>();

	private ParticleSystem m_ParticleSystem;

	private ParticleSystemRenderer m_ParticleSystemRenderer;

	private SnapMapBase m_SnapMap;

	private AnimationSampler m_AnimationSampler;

	private bool m_PersistWhenDisabled;

	private ISnapBehaviour m_SnapBehaviour;

	private AnimationSample m_LastAnimationSample;

	private bool m_ParticleSystemUpdatedOnce;

	private float m_PlaybackTime;

	public static PlaybackState GetPooled(ParticleSystem particleSystem, ParticleSystemRenderer particleSystemRenderer, SnapMapBase snapMap, AnimationSampler animationSampler, bool persistWhenDisabled, ISnapBehaviour snapBehaviour)
	{
		if (!s_Pool.TryPop(out var result))
		{
			result = new PlaybackState();
		}
		result.m_ParticleSystem = particleSystem;
		result.m_ParticleSystemRenderer = particleSystemRenderer;
		result.m_SnapMap = snapMap;
		result.m_AnimationSampler = animationSampler;
		result.m_PersistWhenDisabled = persistWhenDisabled;
		result.m_SnapBehaviour = snapBehaviour;
		result.m_ParticleSystemUpdatedOnce = false;
		return result;
	}

	private PlaybackState()
	{
	}

	public void Recycle()
	{
		m_SnapBehaviour.Recycle();
		m_ParticleSystem = null;
		m_ParticleSystemRenderer = null;
		m_SnapMap = null;
		m_AnimationSampler = default(AnimationSampler);
		m_PersistWhenDisabled = false;
		m_SnapBehaviour = null;
		m_LastAnimationSample = default(AnimationSample);
		m_ParticleSystemUpdatedOnce = false;
		m_PlaybackTime = 0f;
		s_Pool.Push(this);
	}

	public Vector3 GetCurrentOffset()
	{
		return m_LastAnimationSample.offset;
	}

	public void OnStart()
	{
		m_LastAnimationSample = m_AnimationSampler.Sample(0f);
		m_SnapBehaviour.Setup();
		if (m_ParticleSystem != null)
		{
			m_ParticleSystem.Play();
		}
	}

	public void OnStop()
	{
		if (m_ParticleSystem != null)
		{
			m_ParticleSystem.Stop();
			m_ParticleSystem.Clear(withChildren: true);
		}
	}

	public void OnParticleUpdateJobScheduled()
	{
		m_SnapBehaviour.OnParticleUpdateJobScheduled();
	}

	public PlaybackStateUpdateResult Update(PlaybackStateUpdateData data)
	{
		if (m_ParticleSystem == null)
		{
			return PlaybackStateUpdateResult.Failure;
		}
		if (m_ParticleSystemRenderer == null)
		{
			return PlaybackStateUpdateResult.Failure;
		}
		if (m_SnapMap == null)
		{
			return PlaybackStateUpdateResult.Failure;
		}
		if (!m_PersistWhenDisabled && !IsSnapMapEnabled(m_SnapMap))
		{
			return PlaybackStateUpdateResult.Failure;
		}
		m_PlaybackTime += Time.deltaTime;
		if (data.particleSystemVisible || !m_ParticleSystemUpdatedOnce)
		{
			m_LastAnimationSample = m_AnimationSampler.Sample(m_PlaybackTime);
			m_SnapBehaviour.Update(in data.cameraData, in m_LastAnimationSample);
			m_ParticleSystemUpdatedOnce = true;
		}
		return PlaybackStateUpdateResult.Success;
	}

	private static bool IsSnapMapEnabled(SnapMapBase snapMap)
	{
		if (!snapMap.gameObject.activeInHierarchy)
		{
			return snapMap.enabled;
		}
		return true;
	}
}
