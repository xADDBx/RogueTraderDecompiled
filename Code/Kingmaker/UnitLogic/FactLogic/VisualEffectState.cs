using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("eb775beff91944ef879e7d3f14aa45e4")]
public class VisualEffectState : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private VisualStateEffectType m_VisualStateEffectType;

	[SerializeField]
	private bool m_InstantExpire;

	[SerializeField]
	private bool m_UsePostEffect;

	[ShowIf("m_UsePostEffect")]
	[SerializeField]
	private RunPostEffect m_RunPostEffect = new RunPostEffect();

	[Space(20f)]
	[SerializeField]
	private bool m_UseWeatherEffect;

	[ShowIf("m_UseWeatherEffect")]
	[SerializeField]
	private RunWeatherEffect m_RunWeatherEffect = new RunWeatherEffect();

	private PostProcessingEffectsLibrary.SoundEventReferences m_SoundEventReferences;

	private Buff m_OwnerBuff;

	protected override void OnActivateOrPostLoad()
	{
		base.OnActivate();
		if (BlueprintRoot.Instance.WarhammerRoot.PostProcessingEffectsLibrary.GetEffectWwiseEvents.TryGetValue(m_VisualStateEffectType, out m_SoundEventReferences))
		{
			if (m_UseWeatherEffect)
			{
				m_RunWeatherEffect.PreparingCompleteEvent += StartSoundEvent;
			}
			else
			{
				StartSoundEvent();
			}
		}
		if (m_UsePostEffect)
		{
			m_RunPostEffect.Activate(OnActivationComplete);
		}
		if (m_UseWeatherEffect)
		{
			m_RunWeatherEffect.Activate(OnActivationComplete);
		}
	}

	protected override void OnFactAttached()
	{
		base.OnFactAttached();
		m_OwnerBuff = base.Fact as Buff;
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		if (m_SoundEventReferences != null)
		{
			if (m_UseWeatherEffect)
			{
				m_RunWeatherEffect.PreparingCompleteEvent += StopSoundEvent;
			}
			else
			{
				StopSoundEvent();
			}
		}
		if (m_UsePostEffect)
		{
			m_RunPostEffect.Deactivate();
		}
		if (m_UseWeatherEffect)
		{
			m_RunWeatherEffect.Deactivate();
		}
	}

	private void StartSoundEvent()
	{
		m_RunWeatherEffect.PreparingCompleteEvent -= StartSoundEvent;
		AkSoundEngine.SetState(m_SoundEventReferences.State.Group, m_SoundEventReferences.State.Value);
		VFXWeatherSystem instance = VFXWeatherSystem.Instance;
		GameObject gameObject = ((instance != null) ? instance.gameObject : null);
		SoundEventsManager.PostEvent(m_SoundEventReferences.StartEventName, gameObject);
	}

	private void StopSoundEvent()
	{
		m_RunWeatherEffect.PreparingCompleteEvent -= StopSoundEvent;
		AkSoundEngine.SetState(m_SoundEventReferences.State.Group, "None");
		VFXWeatherSystem instance = VFXWeatherSystem.Instance;
		GameObject gameObject = ((instance != null) ? instance.gameObject : null);
		SoundEventsManager.PostEvent(m_SoundEventReferences.StopEventName, gameObject);
	}

	private void OnActivationComplete()
	{
		if (m_InstantExpire && m_UsePostEffect == m_RunPostEffect.WasActivated && m_UseWeatherEffect == m_RunWeatherEffect.WasActivated && m_OwnerBuff != null)
		{
			m_OwnerBuff.Remove();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
