using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.Utility.Random;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("7c2b3991e33fb6640b4f4680441de4be")]
public class StarshipLanceProcs : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleStarshipPerformAttack>, IRulebookHandler<RuleStarshipPerformAttack>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public int onEnemyProcChances;

	public int onSelfProcChances;

	[SerializeField]
	private ActionList ActionsOnProc;

	public void OnEventAboutToTrigger(RuleStarshipPerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RuleStarshipPerformAttack evt)
	{
		if (!evt.Weapon.IsFocusedEnergyWeapon || evt.NextAttackInBurst != null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		RuleStarshipPerformAttack ruleStarshipPerformAttack = evt.FirstAttackInBurst;
		while (ruleStarshipPerformAttack.NextAttackInBurst != null)
		{
			num++;
			if (ruleStarshipPerformAttack.ResultIsHit && evt.DamageRule != null && evt.DamageRule.Result > 0)
			{
				num2++;
			}
			ruleStarshipPerformAttack = ruleStarshipPerformAttack.NextAttackInBurst;
		}
		StarshipEntity entity;
		int num3;
		int num4;
		if (num2 > 0 && !evt.Target.IsSoftUnit)
		{
			entity = evt.Target;
			num3 = num2;
			num4 = onEnemyProcChances;
		}
		else
		{
			entity = evt.Initiator;
			num3 = num;
			num4 = onSelfProcChances;
		}
		for (int i = 0; i < num3; i++)
		{
			if (PFStatefulRandom.SpaceCombat.Range(0, 100) < num4)
			{
				using (base.Context.GetDataScope(entity.ToITargetWrapper()))
				{
					ActionsOnProc.Run();
				}
			}
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
