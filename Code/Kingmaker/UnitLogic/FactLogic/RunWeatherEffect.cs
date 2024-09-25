using System;
using System.Collections;
using Kingmaker.Controllers;
using Kingmaker.Utility.ManualCoroutines;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
public class RunWeatherEffect : RunEffectBase
{
	[SerializeField]
	private VisualStateEffectType m_EffectType;

	[SerializeField]
	private InclemencyType m_InclemencyType;

	[SerializeField]
	private float m_WeatherFadeOutSpeed;

	[SerializeField]
	private float m_EffectFadeInSpeed;

	private CoroutineHandler m_FadeCoroutine;

	public event Action PreparingCompleteEvent;

	public override void Activate(Action completeCallback)
	{
		base.Activate(completeCallback);
		if (m_FadeCoroutine.IsRunning)
		{
			Game.Instance.CoroutinesController.Stop(m_FadeCoroutine);
		}
		if (!VFXWeatherSystem.Instance)
		{
			OnActivate();
		}
		else
		{
			m_FadeCoroutine = Game.Instance.CoroutinesController.Start(FadeCoroutine(m_WeatherFadeOutSpeed, m_EffectFadeInSpeed, m_InclemencyType, base.OnActivate));
		}
	}

	public override void Deactivate()
	{
		if (m_FadeCoroutine.IsRunning)
		{
			Game.Instance.CoroutinesController.Stop(m_FadeCoroutine);
		}
		if ((bool)VFXWeatherSystem.Instance)
		{
			m_FadeCoroutine = Game.Instance.CoroutinesController.Start(FadeCoroutine(m_EffectFadeInSpeed, m_WeatherFadeOutSpeed));
		}
	}

	private IEnumerator FadeCoroutine(float fadeOutSpeed, float fadeInSpeed, InclemencyType? targetInclemency = null, Action completeCallback = null)
	{
		WeatherController.Instance.SetWeatherEffectInclemency(InclemencyType.Clear, instantly: false, fadeOutSpeed);
		float seconds = VFXWeatherSystem.Instance.CurrentWeatherIntensity * fadeOutSpeed;
		yield return YieldInstructions.WaitForSecondsGameTime(seconds);
		this.PreparingCompleteEvent?.Invoke();
		if (!targetInclemency.HasValue)
		{
			WeatherController.Instance.ResetWeatherVisualEffect(fadeInSpeed);
		}
		else
		{
			WeatherController.Instance.SetWeatherVisualEffect(targetInclemency.Value, m_EffectType);
			WeatherController.Instance.SetWeatherEffectInclemency(targetInclemency.Value, instantly: false, fadeInSpeed);
		}
		completeCallback?.Invoke();
	}
}
