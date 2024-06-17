using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("fb2da621e54d4e718a3d8424256cb5f5")]
public class TutorialTriggerAttackMiss : TutorialTrigger, IWarhammerAttackHandler, ISubscriber, IHashable
{
	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		BaseUnitEntity initiatorUnit = withWeaponAttackHit.InitiatorUnit;
		if (initiatorUnit != null && initiatorUnit.IsInPlayerParty && !withWeaponAttackHit.ResultIsHit)
		{
			TryToTrigger(withWeaponAttackHit, delegate(TutorialContext context)
			{
				context.SolutionAbility = withWeaponAttackHit.Ability;
				context.TargetUnit = withWeaponAttackHit.TargetUnit;
			});
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
