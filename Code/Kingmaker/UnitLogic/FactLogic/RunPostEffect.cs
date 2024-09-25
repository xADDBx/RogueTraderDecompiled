using System;
using System.Collections;
using Kingmaker.Controllers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.ManualCoroutines;
using Kingmaker.Visual.DayNightCycle;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
public class RunPostEffect : RunEffectBase
{
	[SerializeField]
	private VisualStateEffectType m_EffectType;

	[SerializeField]
	private AnimationCurve m_FadeInCurve;

	[SerializeField]
	private float m_FadeInTime;

	[SerializeField]
	private AnimationCurve m_FadeOutCurve;

	[SerializeField]
	private float m_FadeOutTime;

	[SerializeField]
	private float m_TargetWeight;

	[NonSerialized]
	private CoroutineHandler m_AnimationCoroutine;

	[NonSerialized]
	private CoroutineHandler m_CallbackCoroutine;

	[NonSerialized]
	private Volume m_Volume;

	[NonSerialized]
	private float m_VolumeInitialWeight;

	public VisualStateEffectType EffectType => m_EffectType;

	public override void Activate(Action completeCallback)
	{
		base.Activate(completeCallback);
		Game.Instance.CoroutinesController.Stop(ref m_AnimationCoroutine);
		Game.Instance.CoroutinesController.Stop(ref m_CallbackCoroutine);
		if (TryGetPostProcessingEffectVolume(out var volume))
		{
			m_Volume = volume;
			if (Game.Instance.CameraFXController.TryGetRunningEffectStartValue(m_EffectType, out var startValue))
			{
				m_VolumeInitialWeight = startValue;
			}
			else
			{
				m_VolumeInitialWeight = m_Volume.weight;
				Game.Instance.CameraFXController.RegisterRunningEffect(this, m_VolumeInitialWeight);
			}
			m_AnimationCoroutine = Game.Instance.CoroutinesController.Start(TwinWeightToTargetValue(m_Volume, m_VolumeInitialWeight, m_TargetWeight, m_FadeInCurve, m_FadeInTime));
		}
		else
		{
			m_Volume = null;
			m_VolumeInitialWeight = 0f;
		}
		m_CallbackCoroutine = Game.Instance.CoroutinesController.InvokeInTime(base.OnActivate, m_FadeInTime.Seconds());
	}

	public override void Deactivate()
	{
		Game.Instance.CoroutinesController.Stop(ref m_AnimationCoroutine);
		Game.Instance.CoroutinesController.Stop(ref m_CallbackCoroutine);
		if (m_Volume != null)
		{
			m_AnimationCoroutine = Game.Instance.CoroutinesController.Start(TwinWeightToTargetValue(m_Volume, m_TargetWeight, m_VolumeInitialWeight, m_FadeOutCurve, m_FadeOutTime));
			m_CallbackCoroutine = Game.Instance.CoroutinesController.InvokeInTime(OnDeactivate, m_FadeInTime.Seconds());
		}
		m_Volume = null;
		m_VolumeInitialWeight = 0f;
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		Game.Instance.CameraFXController.UnregisterRunningEffect(this);
	}

	private bool TryGetPostProcessingEffectVolume(out Volume volume)
	{
		LightController active = LightController.Active;
		if (active != null && active.TryGetPostProcessingEffect(m_EffectType, out volume))
		{
			return volume != null;
		}
		volume = null;
		return false;
	}

	private static TimeSpan GetCurrentTime()
	{
		return Game.Instance.TimeController.RealTime;
	}

	private static IEnumerator TwinWeightToTargetValue(Volume volume, float initialWeight, float finalWeight, AnimationCurve curve, float duration)
	{
		TimeSpan currentTime = GetCurrentTime();
		TimeSpan startTime = currentTime;
		TimeSpan endTime = startTime + duration.Seconds();
		while (currentTime < endTime)
		{
			if (volume != null)
			{
				float time = (float)(currentTime - startTime).TotalSeconds / duration;
				float t = curve.Evaluate(time);
				volume.weight = Mathf.Lerp(initialWeight, finalWeight, t);
			}
			yield return null;
			currentTime = GetCurrentTime();
		}
		volume.weight = finalWeight;
	}
}
