using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.FX;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.Attributes;
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

	private SFXWrapper m_SfxWrapper;

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
		m_SfxWrapper = new SFXWrapperVisualEffect(m_SoundEventReferences);
		Game.Instance.CameraFXSoundController.TryStartEvent(m_SfxWrapper);
	}

	private void StopSoundEvent()
	{
		m_RunWeatherEffect.PreparingCompleteEvent -= StopSoundEvent;
		Game.Instance.CameraFXSoundController.TryStopEvent(m_SfxWrapper);
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
