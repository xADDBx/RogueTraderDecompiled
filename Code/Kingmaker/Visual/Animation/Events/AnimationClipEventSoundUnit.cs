using System;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEventSoundUnit : AnimationClipEvent
{
	public enum SoundTargetType
	{
		Weapon,
		Ability
	}

	public enum SoundType
	{
		None = 0,
		MainWeaponEquip = 3,
		OffWeaponEquip = 4,
		MainWeaponUnequip = 5,
		OffWeaponUnequip = 6,
		ArmorFoley = 7
	}

	[SerializeField]
	private SoundType m_Type;

	[SerializeField]
	private UnitSoundAnimationEventType m_TypeForAbility;

	[SerializeField]
	private SoundTargetType m_TargetType;

	public SoundType Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public UnitSoundAnimationEventType TypeForAbility
	{
		get
		{
			return m_TypeForAbility;
		}
		set
		{
			m_TypeForAbility = value;
		}
	}

	public SoundTargetType TargetType
	{
		get
		{
			return m_TargetType;
		}
		set
		{
			m_TargetType = value;
		}
	}

	public AnimationClipEventSoundUnit(float time)
		: this(time, SoundType.None)
	{
	}

	public AnimationClipEventSoundUnit(float time, SoundType type)
		: base(time)
	{
		m_Type = type;
	}

	public override Action Start(AnimationManager animationManager)
	{
		if (TargetType == SoundTargetType.Weapon)
		{
			switch (Type)
			{
			case SoundType.MainWeaponEquip:
				animationManager.GetComponent<UnitAnimationCallbackReceiver>().PostMainWeaponEquipEvent();
				break;
			case SoundType.OffWeaponEquip:
				animationManager.GetComponent<UnitAnimationCallbackReceiver>().PostOffWeaponEquipEvent();
				break;
			case SoundType.MainWeaponUnequip:
				animationManager.GetComponent<UnitAnimationCallbackReceiver>().PostMainWeaponUnequipEvent();
				break;
			case SoundType.OffWeaponUnequip:
				animationManager.GetComponent<UnitAnimationCallbackReceiver>().PostOffWeaponUnequipEvent();
				break;
			case SoundType.ArmorFoley:
				animationManager.GetComponent<UnitAnimationCallbackReceiver>().PostArmorFoleyEvent();
				break;
			default:
				PFLog.Default.Error(string.Format("{0} does not support {1} of {2}.", "AnimationClipEventSoundUnit", "SoundType", Type));
				break;
			}
		}
		else
		{
			animationManager.GetComponent<UnitAnimationCallbackReceiver>().AbilityAnimationEvent(TypeForAbility);
		}
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventSoundUnit(base.Time, Type);
	}

	public override string ToString()
	{
		return $"{Type} unit sound event at {base.Time}";
	}
}
