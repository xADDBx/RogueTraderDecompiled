using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("77f8a0667de149a5bd6c94dd58b0c81a")]
public class TacticalAdvantagePassive : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformMomentumChange>, IRulebookHandler<RulePerformMomentumChange>, ISubscriber, IGlobalRulebookSubscriber, ITurnBasedModeHandler, IHashable
{
	public class Data : IEntityFactComponentSavableData, IHashable
	{
		public int MomentumThisCombat { get; set; }

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public int Percent;

	[SerializeField]
	private BlueprintBuffReference m_LinchpinBuff;

	[SerializeField]
	private BlueprintBuffReference m_TacticalAdvantageBuff;

	public StatType LinchpinStat;

	public int LinchpinStatBonus;

	public int LinchpinBaseBonus;

	[SerializeField]
	private BlueprintUnitFactReference m_ComfortInConformityFact;

	public int ComfortInConformityHeal;

	public BlueprintBuff LinchpinBuff => m_LinchpinBuff?.Get();

	public BlueprintBuff TacticalAdvantageBuff => m_TacticalAdvantageBuff?.Get();

	public BlueprintUnitFact ComfortInConformityFact => m_ComfortInConformityFact?.Get();

	public void OnEventAboutToTrigger(RulePerformMomentumChange evt)
	{
	}

	public void OnEventDidTrigger(RulePerformMomentumChange evt)
	{
		if (!evt.Initiator.IsPlayerFaction || !base.Owner.IsInPlayerParty || base.Owner.IsDead)
		{
			return;
		}
		CompanionState? companionState = base.Owner.GetCompanionOptional()?.State;
		if (companionState.HasValue && companionState.GetValueOrDefault() != CompanionState.InParty)
		{
			return;
		}
		Data data = RequestSavableData<Data>();
		MechanicEntity mechanicEntity = (MechanicEntity)evt.Initiator;
		if (mechanicEntity == null || evt.ResultDeltaValue <= 0)
		{
			return;
		}
		int num = Percent + (mechanicEntity.Facts.Contains(LinchpinBuff) ? (LinchpinBaseBonus + (base.Owner.GetOptional<PartStatsContainer>()?.GetAttributeOptional(LinchpinStat)?.Bonus).GetValueOrDefault() * LinchpinStatBonus) : 0);
		data.MomentumThisCombat += evt.ResultDeltaValue * num;
		int num2 = data.MomentumThisCombat / 100;
		data.MomentumThisCombat -= num2 * 100;
		BuffDuration duration = new BuffDuration(null, BuffEndCondition.CombatEnd);
		if (num2 >= 1)
		{
			Buff buff = base.Owner.Buffs.Add(TacticalAdvantageBuff, base.Context, duration);
			if (num2 > 1)
			{
				buff?.AddRank(num2 - 1);
			}
		}
		PartHealth healthOptional = mechanicEntity.GetHealthOptional();
		if (base.Owner.Facts.Contains(ComfortInConformityFact) && healthOptional != null && healthOptional.HitPointsLeft > 0)
		{
			RuleHealDamage rule = new RuleHealDamage(base.Owner, mechanicEntity, new DiceFormula(0, DiceType.Zero), num2 * ComfortInConformityHeal);
			base.Context.TriggerRule(rule);
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			RequestSavableData<Data>().MomentumThisCombat = 0;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
