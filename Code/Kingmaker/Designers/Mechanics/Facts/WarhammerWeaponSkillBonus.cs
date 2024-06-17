using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("b9754822222d431189bcbfe02218fc6c")]
public class WarhammerWeaponSkillBonus : UnitBuffComponentDelegate, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateParryChance>, IRulebookHandler<RuleCalculateParryChance>, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValue WeaponSkillBonus;

	public bool BonusAgainstCastersPriorityTarget;

	public bool BonusAgainstAlliedCastersPriorityTarget;

	[ShowIf("PriorityTarget")]
	private BlueprintBuffReference m_TargetBuff;

	[UsedImplicitly]
	private bool PriorityTarget
	{
		get
		{
			if (!BonusAgainstCastersPriorityTarget)
			{
				return BonusAgainstAlliedCastersPriorityTarget;
			}
			return true;
		}
	}

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		if (evt.Initiator == base.Owner && CheckConditions(evt.ConcreteInitiator, evt.ConcreteTarget))
		{
			evt.InitiatorWeaponSkillValueModifiers.Add(WeaponSkillBonus.Calculate(base.Context), base.Fact);
		}
		if (evt.Target == base.Owner && CheckConditions(evt.ConcreteTarget, evt.ConcreteInitiator))
		{
			evt.TargetWeaponSkillValueModifiers.Add(WeaponSkillBonus.Calculate(base.Context), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateParryChance evt)
	{
		if (evt.MaybeAttacker == base.Owner && CheckConditions(evt.MaybeAttacker, evt.Defender))
		{
			evt.AttackerWeaponSkillValueModifiers.Add(WeaponSkillBonus.Calculate(base.Context), base.Fact);
		}
		if (evt.Defender == base.Owner && CheckConditions(evt.Defender, evt.MaybeAttacker))
		{
			evt.DefenderCurrentAttackSkillValueModifiers.Add(WeaponSkillBonus.Calculate(base.Context), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateParryChance evt)
	{
	}

	public bool CheckConditions(MechanicEntity attacker, MechanicEntity defender)
	{
		if (BonusAgainstCastersPriorityTarget)
		{
			MechanicEntity maybeCaster = base.Context.MaybeCaster;
			BaseUnitEntity baseUnitEntity = maybeCaster?.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
			if (baseUnitEntity == null || defender != baseUnitEntity || attacker != maybeCaster)
			{
				return false;
			}
		}
		if (BonusAgainstAlliedCastersPriorityTarget)
		{
			MechanicEntity maybeCaster2 = base.Context.MaybeCaster;
			BaseUnitEntity baseUnitEntity2 = maybeCaster2?.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
			if (baseUnitEntity2 == null || defender != baseUnitEntity2 || attacker == maybeCaster2)
			{
				return false;
			}
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
