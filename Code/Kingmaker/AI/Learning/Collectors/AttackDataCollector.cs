using Kingmaker.AI.Learning.Collections;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.AI.Learning.Collectors;

public class AttackDataCollector : UnitDataCollector, IDamageHandler, ISubscriber
{
	public AttackDataCollector(BaseUnitEntity unit, UnitDataStorage storage)
		: base(unit, storage)
	{
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if (dealDamage.InitiatorUnit == GetSubscribingEntity() && dealDamage.InitiatorUnit != dealDamage.Target && !(dealDamage.SourceAbility == null))
		{
			if (string.IsNullOrEmpty(dealDamage.SourceAbility.UniqueId))
			{
				PFLog.Default.Error("Damage dealt with ability without name!");
			}
			else
			{
				base.Storage[dealDamage.InitiatorUnit].AttackDataCollection.Add(CreateAttackData(dealDamage));
			}
		}
	}

	private AttackData CreateAttackData(RuleDealDamage dealDamage)
	{
		return new AttackData(dealDamage.SourceAbility.UniqueId, dealDamage.InitiatorUnit.DistanceToInCells((MechanicEntity)dealDamage.Target), dealDamage.Result);
	}
}
