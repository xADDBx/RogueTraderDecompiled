using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("06b0345091976784cb849e08a5c6c1f6")]
public class StarshipDamageAdjacentShields : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleStarshipPerformAttack>, IRulebookHandler<RuleStarshipPerformAttack>, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookHandler<RuleStarshipPerformAttack>, ITargetRulebookSubscriber, IHashable
{
	private enum ModifyWhen
	{
		IsInitiator,
		IsTarget
	}

	[SerializeField]
	private ModifyWhen modifyWhen;

	public bool CheckWeaponBlueprint;

	[SerializeField]
	[ShowIf("CheckWeaponBlueprint")]
	private BlueprintStarshipWeaponReference[] m_WeaponBlueprints;

	public ReferenceArrayProxy<BlueprintStarshipWeapon> WeaponBlueprints
	{
		get
		{
			BlueprintReference<BlueprintStarshipWeapon>[] weaponBlueprints = m_WeaponBlueprints;
			return weaponBlueprints;
		}
	}

	public void OnEventAboutToTrigger(RuleStarshipPerformAttack evt)
	{
		if ((modifyWhen != 0 || evt.Initiator == base.Owner) && (modifyWhen != ModifyWhen.IsTarget || evt.Target == base.Owner) && (!CheckWeaponBlueprint || WeaponBlueprints.Contains(evt.Weapon.Blueprint)))
		{
			evt.DamageAdjacentShields = true;
		}
	}

	public void OnEventDidTrigger(RuleStarshipPerformAttack evt)
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
