using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility;

namespace Kingmaker.RuleSystem.Rules;

[Serializable]
public class RulePerformCollision : RulebookTargetEvent<MechanicEntity, MechanicEntity>
{
	private const int PushMultiplier = 2;

	public readonly CompositeModifiersManager DamageModifiers = new CompositeModifiersManager(0);

	public int DamageRank { get; set; }

	public int ResultDamage { get; set; }

	public MechanicEntity Pushed => base.Target;

	public MechanicEntity Pusher => base.Initiator;

	public new NotImplementedException Initiator
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public new NotImplementedException Target
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public RulePerformCollision([NotNull] MechanicEntity pusher, MechanicEntity pushingEntity, int damageRank)
		: base(pusher, pushingEntity)
	{
		DamageRank = damageRank;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		RuleDealDamage preparedDealDamageRule = GetPreparedDealDamageRule();
		Rulebook.Trigger(preparedDealDamageRule);
		if (!(Pushed is UnitEntity))
		{
			return;
		}
		RulePerformSkillCheck rulePerformSkillCheck = new RulePerformSkillCheck(Pushed, StatType.WarhammerAgility, 0);
		Rulebook.Trigger(rulePerformSkillCheck);
		if (!rulePerformSkillCheck.ResultIsSuccess)
		{
			BlueprintBuff control = BlueprintRoot.Instance.WarhammerRoot.UnitConditionBuffs.GetControl(UnitCondition.Prone);
			if (control != null)
			{
				Pushed.Buffs.Add(control, Pushed, 1.Rounds());
			}
		}
		ResultDamage = preparedDealDamageRule.Result;
	}

	private RuleDealDamage GetPreparedDealDamageRule()
	{
		int value = DamageRank * 2;
		DamageData resultDamage = Rulebook.Trigger(new RuleCalculateDamage(Pusher, Pushed, null, null, new DamageData(DamageType.Impact, value))).ResultDamage;
		resultDamage.Modifiers.CopyFrom(DamageModifiers);
		return new RuleDealDamage(Pusher, Pushed, resultDamage)
		{
			IsCollisionDamage = true
		};
	}
}
