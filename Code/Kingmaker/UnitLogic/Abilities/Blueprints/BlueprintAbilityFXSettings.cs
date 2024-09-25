using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Visual.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Animation.Kingmaker;
using MemoryPack;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Blueprints;

[TypeId("bb62eb389c744c4490b1146890d7e2e4")]
[MemoryPackable(GenerateType.NoGenerate)]
public class BlueprintAbilityFXSettings : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintAbilityFXSettings>
	{
	}

	public enum AnimationEventTarget
	{
		Caster,
		CasterWeapon
	}

	[SerializeField]
	private BlueprintAbilityVisualFXSettings.Reference m_VisualFXSettings;

	[SerializeField]
	private BlueprintAbilitySoundFXSettings.Reference m_SoundFXSettings;

	[SerializeField]
	[Obsolete]
	private UnitAnimationAction m_Action;

	[SerializeField]
	private UnitAnimationActionLink m_ActionLink;

	[SerializeField]
	[Obsolete]
	private UnitAnimationAction m_OffHandAction;

	[SerializeField]
	private UnitAnimationActionLink m_OffHandActionLink;

	[SerializeField]
	[Obsolete]
	private UnitAnimationAction m_CornerAction;

	[SerializeField]
	private UnitAnimationActionLink m_CornerActionLink;

	[SerializeField]
	[Obsolete]
	private UnitAnimationAction m_OffHandCornerAction;

	[SerializeField]
	private UnitAnimationActionLink m_OffHandCornerActionLink;

	[SerializeField]
	[Tooltip("Поворачиваться к цели со смещением. Помогает точнее направлять атаки особо широких юнитов.")]
	private bool m_ShouldOffsetTargetRelativePosition;

	[SerializeField]
	[ShowIf("ShouldOffsetTargetRelativePosition")]
	[Tooltip("Смещение относительно взгляда. Например, [x = -1] - это значит поворачиваться левее положения цели на метр. Для пушки в широкой правой руке.")]
	private Vector3 m_OffsetTargetPosition = Vector3.zero;

	public BlueprintAbilityVisualFXSettings VisualFXSettings => m_VisualFXSettings;

	[CanBeNull]
	public BlueprintAbilitySoundFXSettings SoundFXSettings => m_SoundFXSettings;

	public bool ShouldOffsetTargetRelativePosition => m_ShouldOffsetTargetRelativePosition;

	public Vector3 OffsetTargetPosition => m_OffsetTargetPosition;

	public UnitAnimationActionLink GetAnimation(bool isMainHandAttack, bool isCornerAttack)
	{
		if (!isMainHandAttack)
		{
			if (!isCornerAttack)
			{
				return m_OffHandActionLink;
			}
			return m_OffHandCornerActionLink;
		}
		if (!isCornerAttack)
		{
			return m_ActionLink;
		}
		return m_CornerActionLink;
	}
}
