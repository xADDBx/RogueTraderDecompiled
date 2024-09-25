using System;
using Kingmaker.Sound;
using UnityEngine;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class ShieldHitEntry
{
	public ShieldType Type;

	public bool ShowEntityBlood;

	public bool ShowSpecialEffect;

	public GameObject SpecialEffect;

	public bool HitSoundEvent = true;

	public bool OverrideTargetTypeSwitch;

	[SerializeField]
	private AkSwitchReference m_targetTypeSwitch;

	[SerializeField]
	private AkSwitchReference m_muffledTypeSwitch;

	public bool ShowImpact;

	public bool HitInSphere;

	public float SphereRadius;

	public bool ShowHitAnimation;

	public AkSwitchReference TargetTypeSwitch => m_targetTypeSwitch;

	public AkSwitchReference MuffledTypeSwitch => m_muffledTypeSwitch;
}
