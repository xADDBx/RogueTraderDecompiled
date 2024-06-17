using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Visual.FX;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Blueprints;

[TypeId("8b8278f15d7aa2847ab527d82bc25946")]
public class BlueprintAbilitySoundFXSettings : BlueprintScriptableObject, ISoundSettingsProvider
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintAbilitySoundFXSettings>
	{
	}

	[Serializable]
	public enum RTPCDataType
	{
		AmmoAmount
	}

	public enum SwitchDataType
	{
		TailType
	}

	[Serializable]
	public class UnitSoundAnimationEventSettings : ISoundSettings
	{
		[SerializeField]
		private UnitSoundAnimationEventType m_UnitSoundAnimationEventType;

		[SerializeField]
		private SoundSettings m_SoundSettings;

		[SerializeField]
		private BlueprintAbilityFXSettings.AnimationEventTarget m_Target;

		public UnitSoundAnimationEventType? AnimationEvent => m_UnitSoundAnimationEventType;

		public AbilityEventType? AbilityEvent => null;

		public FXTarget Target
		{
			get
			{
				if (m_Target != 0)
				{
					return FXTarget.CasterWeapon;
				}
				return FXTarget.Caster;
			}
		}

		public SoundSettings Settings => m_SoundSettings;
	}

	[Serializable]
	public class MechanicalSoundEventSettings : ISoundSettings
	{
		[SerializeField]
		private AbilityEventType m_AbilityEvent;

		[SerializeField]
		private FXTarget m_Target;

		[SerializeField]
		private SoundSettings m_SoundSettings;

		public UnitSoundAnimationEventType? AnimationEvent => null;

		public AbilityEventType? AbilityEvent => m_AbilityEvent;

		public FXTarget Target => m_Target;

		public SoundSettings Settings => m_SoundSettings;
	}

	[SerializeField]
	[AkEventReference]
	private string m_DopplerStart;

	[SerializeField]
	[AkEventReference]
	private string m_DopplerFinish;

	[SerializeField]
	[AkGameParameterReference]
	private string m_DopplerRtpc;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_ProjectileSpeed = 0.5f;

	[SerializeField]
	private UnitSoundAnimationEventSettings[] m_AnimationEvents;

	[SerializeField]
	private MechanicalSoundEventSettings[] m_MechanicalEvents;

	public string DopplerStart => m_DopplerStart;

	public string DopplerFinish => m_DopplerFinish;

	public string DopplerRTPC => m_DopplerRtpc;

	public float ProjectileSpeed => m_ProjectileSpeed;

	public UnitSoundAnimationEventSettings[] AnimationEvents => m_AnimationEvents;

	public MechanicalSoundEventSettings[] MechanicalEvents => m_MechanicalEvents;

	public IEnumerable<ISoundSettings> GetSounds(UnitSoundAnimationEventType eventType)
	{
		UnitSoundAnimationEventSettings[] animationEvents = AnimationEvents;
		foreach (UnitSoundAnimationEventSettings unitSoundAnimationEventSettings in animationEvents)
		{
			if (unitSoundAnimationEventSettings.AnimationEvent == eventType)
			{
				yield return unitSoundAnimationEventSettings;
			}
		}
	}

	public IEnumerable<ISoundSettings> GetSounds(AbilityEventType abilityEventType)
	{
		MechanicalSoundEventSettings[] mechanicalEvents = MechanicalEvents;
		foreach (MechanicalSoundEventSettings mechanicalSoundEventSettings in mechanicalEvents)
		{
			if (mechanicalSoundEventSettings.AbilityEvent == abilityEventType)
			{
				yield return mechanicalSoundEventSettings;
			}
		}
	}
}
