using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Enums.Sound;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.View.Animation;
using Kingmaker.Visual.FX;
using Kingmaker.Visual.Particles.Blueprints;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Visual.Blueprints;

[TypeId("b3be5ca3c50947c4bbf339bd29a95405")]
public class BlueprintAbilityVisualFXSettings : BlueprintScriptableObject, IFXSettingsProvider
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintAbilityVisualFXSettings>
	{
	}

	[Serializable]
	public class AnimationEventData : IFXSettings
	{
		[SerializeField]
		private MappedAnimationEventType m_Event;

		[SerializeField]
		private BlueprintAbilityFXSettings.AnimationEventTarget m_Target;

		[SerializeField]
		private FXSettings m_Settings;

		public MappedAnimationEventType? AnimationEvent => m_Event;

		AbilityEventType? IFXSettings.AbilityEvent => null;

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

		public bool OverrideTargetOrientationSource => false;

		public FXSettings Settings => m_Settings;
	}

	[Serializable]
	public class AbilityEventData : IFXSettings
	{
		[SerializeField]
		private AbilityEventType m_Event;

		[SerializeField]
		private FXTarget m_Target;

		[SerializeField]
		[Tooltip("Use OverrideRotatablePart as orientation source instead of root. Used for turrets.")]
		private bool m_OverrideTargetOrientationSource;

		[SerializeField]
		private FXSettings m_Settings;

		MappedAnimationEventType? IFXSettings.AnimationEvent => null;

		public AbilityEventType? AbilityEvent => m_Event;

		public FXTarget Target => m_Target;

		public bool OverrideTargetOrientationSource => m_OverrideTargetOrientationSource;

		public FXSettings Settings => m_Settings;
	}

	[SerializeField]
	private BlueprintProjectileReference[] m_Projectiles;

	[SerializeField]
	private BlueprintAbilityFXSettings.AnimationEventTarget m_ProjectileOriginType = BlueprintAbilityFXSettings.AnimationEventTarget.CasterWeapon;

	[SerializeField]
	private BlueprintFxLocatorGroup.Reference m_ProjectileOrigin;

	[SerializeField]
	private BlueprintFxLocatorGroup.Reference m_ProjectileTarget;

	[SerializeField]
	private bool m_UseRandomLocatorInGroup;

	public RecoilStrength Recoil;

	public AnimationEventData[] AnimationEvents;

	public AbilityEventData[] MechanicalEvents;

	public ReferenceArrayProxy<BlueprintProjectile> Projectiles
	{
		get
		{
			BlueprintReference<BlueprintProjectile>[] projectiles = m_Projectiles;
			return projectiles;
		}
	}

	public BlueprintFxLocatorGroup ProjectileOrigin => m_ProjectileOrigin;

	public BlueprintFxLocatorGroup ProjectileTarget => m_ProjectileTarget;

	public bool UseRandomLocatorInGroup => m_UseRandomLocatorInGroup;

	public bool ProjectileOriginIsWeapon => m_ProjectileOriginType == BlueprintAbilityFXSettings.AnimationEventTarget.CasterWeapon;

	public IEnumerable<IFXSettings> GetFXs(MappedAnimationEventType eventType)
	{
		AnimationEventData[] animationEvents = AnimationEvents;
		foreach (AnimationEventData animationEventData in animationEvents)
		{
			if (animationEventData.AnimationEvent == eventType)
			{
				yield return animationEventData;
			}
		}
	}

	public IEnumerable<IFXSettings> GetFXs(AbilityEventType eventType)
	{
		AbilityEventData[] mechanicalEvents = MechanicalEvents;
		foreach (AbilityEventData abilityEventData in mechanicalEvents)
		{
			if (abilityEventData.AbilityEvent == eventType)
			{
				yield return abilityEventData;
			}
		}
	}
}
