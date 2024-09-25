using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("e504ccf75a2139b45a2fb1d50d2dabf0")]
public class StarshipKillTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	[SerializeField]
	private bool IgnoreSoftUnits;

	[SerializeField]
	private bool EnemyOnly;

	[SerializeField]
	private ActionList ActionsOnSelf;

	[SerializeField]
	private ActionList ActionsOnTarget;

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.TargetHealth != null && evt.HPBeforeDamage > 0 && evt.TargetHealth.HitPointsLeft <= 0 && evt.Target is StarshipEntity starshipEntity && (!IgnoreSoftUnits || !starshipEntity.IsSoftUnit) && (!EnemyOnly || base.Owner.IsEnemy(starshipEntity)))
		{
			MechanicEntity mechanicEntity = (MechanicEntity)evt.Initiator;
			if (mechanicEntity == base.Owner || mechanicEntity.GetOptional<UnitPartSummonedMonster>()?.Summoner == base.Owner)
			{
				base.Fact.RunActionInContext(ActionsOnSelf, base.Owner.ToITargetWrapper());
				base.Fact.RunActionInContext(ActionsOnTarget, starshipEntity.ToITargetWrapper());
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
