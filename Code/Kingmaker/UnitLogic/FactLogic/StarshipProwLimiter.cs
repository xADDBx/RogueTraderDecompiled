using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("5aae230dab89cde458108b4b6322d5ad")]
public class StarshipProwLimiter : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		WeaponSlot slot = evt.Spell.StarshipWeaponSlot;
		if (slot == null || slot.Type != WeaponSlotType.Prow || slot.Weapon == null)
		{
			return;
		}
		foreach (WeaponSlot item in (evt.Initiator as StarshipEntity).Hull.WeaponSlots.Where((WeaponSlot s) => s.Type == slot.Type && s != slot && s.Weapon?.Blueprint.WeaponType == slot.Weapon.Blueprint.WeaponType))
		{
			item.Weapon.Charges = 0;
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
