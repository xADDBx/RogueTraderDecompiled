using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("dee632de035bfdb48b823d7418a3ccd8")]
public class WarhammerModifyFiringArcRange : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityRange>, IRulebookHandler<RuleCalculateAbilityRange>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	private RestrictedFiringArc firingArc;

	[SerializeField]
	private int bonusRange;

	public void OnEventAboutToTrigger(RuleCalculateAbilityRange evt)
	{
		if ((firingArc == RestrictedFiringArc.Any || evt.Ability.RestrictedFiringArc == firingArc) && evt.Ability.Blueprint.AbilityTag == AbilityTag.StarshipShotAbility)
		{
			evt.FiringArcBonus += bonusRange;
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
