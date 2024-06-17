using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c568ae66919f445fa9cdbb9a91cfe144")]
public class WarhammerAbilityCostLessForEachEnemy : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityActionPointCost>, IRulebookHandler<RuleCalculateAbilityActionPointCost>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public const int Reduction = 1;

	public const int DistanceInCells = 1;

	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	public BlueprintAbility Ability => m_Ability?.Get();

	public void OnEventAboutToTrigger(RuleCalculateAbilityActionPointCost evt)
	{
		if (evt.Blueprint != Ability)
		{
			return;
		}
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			if (allBaseUnit.CombatGroup.IsEnemy(base.Owner) && allBaseUnit.DistanceToInCells(base.Owner) <= 1)
			{
				evt.CostBonus--;
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityActionPointCost evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
