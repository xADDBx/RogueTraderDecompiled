using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("9adedba45dfe4964fae8cca22e179e20")]
public class StarshipAbilityRangeExtender : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityRange>, IRulebookHandler<RuleCalculateAbilityRange>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	private bool AE_Only;

	[SerializeField]
	private int extraRange;

	public void OnEventAboutToTrigger(RuleCalculateAbilityRange evt)
	{
		if (base.OwnerBlueprint is BlueprintAbility && evt.Ability.Blueprint == base.OwnerBlueprint && (!AE_Only || (evt.Ability.StarshipWeapon?.Ammo?.Blueprint.IsAE).GetValueOrDefault()))
		{
			evt.Bonus += extraRange;
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityRange evt)
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
