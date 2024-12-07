using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[TypeId("2208984b829b49fd878da37b3413ce46")]
public class CastSpell : GameAction
{
	[SerializeReference]
	[CanBeNull]
	public MechanicEntityEvaluator Caster;

	[SerializeReference]
	[CanBeNull]
	[ShowIf("CasterIsNull")]
	public PositionEvaluator CasterPosition;

	[SerializeReference]
	[CanBeNull]
	public MechanicEntityEvaluator TargetEntity;

	[SerializeReference]
	[CanBeNull]
	public PositionEvaluator TargetPoint;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityReference m_Ability;

	public AttackHitPolicyType HitPolicy = AttackHitPolicyType.AutoHit;

	public ActionList ActionToDoIfCastIsNotValid;

	public bool DisableLog;

	public BlueprintAbility Ability => m_Ability;

	private bool CasterIsNull => Caster == null;

	public override string GetCaption()
	{
		if (Caster != null)
		{
			return string.Format("{0} cast {1} on {2}", Caster, Ability, TargetEntity?.ToString() ?? TargetPoint?.ToString() ?? "themself");
		}
		return string.Format("Simple Caster cast {0} on {1}", Ability, TargetEntity?.ToString() ?? TargetPoint?.ToString() ?? "themself");
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = Caster?.GetValue() ?? SimpleCaster.GetFree();
		if (mechanicEntity is SimpleCaster && CasterPosition != null)
		{
			mechanicEntity.Position = CasterPosition.GetValue();
		}
		MechanicEntity mechanicEntity2 = TargetEntity?.GetValue();
		object obj;
		if (mechanicEntity2 == null)
		{
			Vector3? vector = TargetPoint?.GetValue();
			obj = (vector.HasValue ? ((TargetWrapper)vector.GetValueOrDefault()) : ((TargetWrapper)mechanicEntity));
		}
		else
		{
			obj = (TargetWrapper)mechanicEntity2;
		}
		TargetWrapper target = (TargetWrapper)obj;
		AbilityData abilityData = new AbilityData(Ability, mechanicEntity);
		if (abilityData.IsValid(target))
		{
			RulePerformAbility rulePerformAbility = new RulePerformAbility(abilityData, target);
			rulePerformAbility.IgnoreCooldown = true;
			rulePerformAbility.ForceFreeAction = true;
			rulePerformAbility.Context.IsForced = true;
			rulePerformAbility.DisableGameLog = DisableLog;
			rulePerformAbility.Context.DisableLog = DisableLog;
			Rulebook.Trigger(rulePerformAbility);
			rulePerformAbility.Context.HitPolicy = HitPolicy;
			rulePerformAbility.Context.RewindActionIndex();
		}
		else
		{
			ActionToDoIfCastIsNotValid.Run();
		}
	}
}
