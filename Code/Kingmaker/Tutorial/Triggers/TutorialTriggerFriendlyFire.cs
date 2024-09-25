using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[ClassInfoBox("Tags: 't|SolutionAbility' - attack ability\n 't|TargetUnit' - unit who has been attacked\n 't|SolutionUnit' - unit who can cast ability")]
[TypeId("cc5ec330c0c941f08342d7806ce13d0a")]
public class TutorialTriggerFriendlyFire : TutorialTrigger, IWarhammerAttackHandler, ISubscriber, IHashable
{
	private enum AllyType
	{
		PartyMembers,
		AllAllies
	}

	[SerializeField]
	private AllyType m_AllyType;

	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		if (withWeaponAttackHit.ResultIsHit && IsAlly(withWeaponAttackHit, m_AllyType))
		{
			TryToTrigger(withWeaponAttackHit, delegate(TutorialContext context)
			{
				context.SolutionAbility = withWeaponAttackHit.Ability;
				context.TargetUnit = withWeaponAttackHit.TargetUnit;
				context.SolutionUnit = (BaseUnitEntity)withWeaponAttackHit.Initiator;
			});
		}
	}

	private static bool IsAlly(RulePerformAttack withWeaponAttackHit, AllyType allyType)
	{
		if (allyType != 0)
		{
			return withWeaponAttackHit.ConcreteTarget.IsAlly(withWeaponAttackHit.Initiator);
		}
		return withWeaponAttackHit.ConcreteTarget.IsInPlayerParty;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
