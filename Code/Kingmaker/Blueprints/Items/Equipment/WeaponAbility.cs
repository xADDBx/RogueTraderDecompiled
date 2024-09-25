using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Equipment.Modes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Equipment;

[Serializable]
public class WeaponAbility
{
	public WeaponAbilityType Type;

	[HideIf("HideFields")]
	public ItemEquipmentModeType Mode;

	[SerializeField]
	[HideIf("HideFields")]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	[HideIf("HideFields")]
	private BlueprintAbilityFXSettings.Reference m_FXSettings;

	[HideIf("HideFields")]
	public OnHitOverrideType OnHitOverrideType;

	[SerializeField]
	[HideIf("HideFields")]
	[ShowIf("ShowOnHitActions")]
	private BlueprintAbilityAdditionalEffect.Reference m_OnHitActions;

	[HideIf("HideFields")]
	public int AP;

	[CanBeNull]
	public BlueprintAbility Ability => IsNone ? null : m_Ability;

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => m_FXSettings;

	[CanBeNull]
	public BlueprintAbilityAdditionalEffect OnHitActions => m_OnHitActions;

	public bool IsNone => Type == WeaponAbilityType.None;

	private bool HideFields => Type == WeaponAbilityType.None;

	private bool ShowOnHitActions => OnHitOverrideType != OnHitOverrideType.None;
}
