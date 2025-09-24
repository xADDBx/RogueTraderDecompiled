using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.View.MapObjects.Traps.Simple;

[Serializable]
public class SimpleTrapObjectInfo
{
	private enum Type
	{
		Spell,
		Weapon
	}

	[SerializeField]
	private Type m_Type;

	public bool OverridePerceptionRadius;

	[ShowIf("OverridePerceptionRadius")]
	public float PerceptionRadius;

	[ShowIf("IsSpell")]
	public BlueprintAbilityReference BlueprintSpell;

	[ShowIf("IsWeapon")]
	public BlueprintItemWeaponReference BlueprintWeapon;

	[ShowIf("IsWeapon")]
	public WeaponAbilityType WeaponAbilityType;

	[InfoBox("Usable for burst abilities for emulating delay between shots (negative value or zero means shoot all projectiles immediately)")]
	public float SecondsBetweenAbilityActions = -1f;

	public TrapSpellAnchor SpellAnchor;

	public int AdditionalCR;

	public LocalizedString TrapTriggeredText;

	public SkillCheckDifficulty DisableDifficulty;

	[ShowIf("DifficultyIsCustom")]
	public int DC;

	public bool DoNotHideWhenInactive;

	public StatType DisarmSkill = StatType.SkillDemolition;

	private bool DifficultyIsCustom => DisableDifficulty == SkillCheckDifficulty.Custom;

	public bool IsSpell => m_Type == Type.Spell;

	public bool IsWeapon => m_Type == Type.Weapon;
}
