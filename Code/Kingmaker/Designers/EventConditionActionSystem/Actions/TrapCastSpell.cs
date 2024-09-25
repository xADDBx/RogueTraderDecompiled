using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.View.MapObjects.Traps.Simple;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[TypeId("dd879c8cbefa4b948bed7440931873a3")]
public class TrapCastSpell : GameAction
{
	private enum Type
	{
		Spell,
		Weapon
	}

	[SerializeField]
	private Type m_Type;

	[SerializeField]
	[ValidateNotNull]
	[ShowIf("IsSpell")]
	private BlueprintAbilityReference m_Spell;

	[SerializeField]
	[ShowIf("IsWeapon")]
	private WeaponAbilityType m_WeaponAbilityType = WeaponAbilityType.SingleShot;

	[SerializeField]
	[ShowIf("IsWeapon")]
	private BlueprintItemWeaponReference m_Weapon;

	[SerializeField]
	[InfoBox("Usable for burst abilities for emulating delay between shots (negative value or zero means shoot all projectiles immediately)")]
	private float m_SecondsBetweenAbilityActions = -1f;

	[SerializeReference]
	[ValidateNotNull]
	public MapObjectEvaluator TrapObject;

	[SerializeReference]
	[InfoBox("Required if Spell can not target Point (like Scorching ray)")]
	public AbstractUnitEvaluator TriggeringUnit;

	[SerializeReference]
	[InfoBox("Required if Spell can target Point (like Fireball)")]
	public PositionEvaluator TargetPoint;

	[SerializeReference]
	public PositionEvaluator ActorPosition;

	public bool DisableBattleLog;

	public bool OverrideDC;

	[ShowIf("OverrideDC")]
	public int DC;

	public bool OverrideSpellLevel;

	[ShowIf("OverrideSpellLevel")]
	public int SpellLevel;

	[CanBeNull]
	public BlueprintAbility Spell
	{
		get
		{
			if (!IsSpell)
			{
				return null;
			}
			return m_Spell?.Get();
		}
	}

	[CanBeNull]
	public BlueprintItemWeapon Weapon => IsWeapon ? m_Weapon : null;

	private bool IsSpell => m_Type == Type.Spell;

	private bool IsWeapon => m_Type == Type.Weapon;

	public override string GetDescription()
	{
		return $"Пытается скастовать спелл {Spell} из ловушки {TrapObject}\n" + "Внутри механики ловушки саму ловушку можно получить эвалюатором Trap";
	}

	public override string GetCaption()
	{
		return $"Trap cast spell {Spell}";
	}

	protected override void RunAction()
	{
		TrapObjectData trapObjectData = (TrapObjectData)TrapObject.GetValue();
		Ability ability;
		if (IsSpell && Spell != null)
		{
			ability = trapObjectData.RequestAbility(Spell);
		}
		else
		{
			if (!IsWeapon || Weapon == null)
			{
				throw new Exception("Can't find ability");
			}
			ability = trapObjectData.RequestWeaponAbility(Weapon, m_WeaponAbilityType);
		}
		SimpleCaster free = SimpleCaster.GetFree();
		free.IsTrap = true;
		if (trapObjectData.View is SimpleTrapObjectView simpleTrapObjectView)
		{
			free.NameInLog = simpleTrapObjectView.NameInLog;
		}
		if (ActorPosition != null)
		{
			free.Position = ActorPosition.GetValue();
		}
		TargetWrapper target = ability.Data.TargetAnchor switch
		{
			AbilityTargetAnchor.Owner => free, 
			AbilityTargetAnchor.Unit => TriggeringUnit.GetValue(), 
			AbilityTargetAnchor.Point => TargetPoint?.GetValue() ?? TriggeringUnit.GetValue().Position, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		AbilityData abilityData = new AbilityData(ability, free);
		if (OverrideDC)
		{
			abilityData.OverrideDC = DC;
		}
		if (OverrideSpellLevel)
		{
			abilityData.OverrideSpellLevel = SpellLevel;
		}
		Rulebook.Trigger(new RulePerformAbility(abilityData, target)
		{
			DisableGameLog = DisableBattleLog
		}).Context.RewindActionIndex(m_SecondsBetweenAbilityActions.Seconds());
	}
}
