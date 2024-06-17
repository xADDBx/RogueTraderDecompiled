using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts.HitChance;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("858abf54f50b4ca4da20cf6962751354")]
public class StarshipHitCritModifier : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleStarshipCalculateHitChances>, IRulebookHandler<RuleStarshipCalculateHitChances>, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookHandler<RuleStarshipCalculateHitChances>, ITargetRulebookSubscriber, IHashable
{
	private enum ModifyWhen
	{
		IsInitiator,
		IsTarget
	}

	[SerializeField]
	private ModifyWhen modifyWhen;

	public int HitBonusPct;

	public int CritBonusPct;

	public float CritBonusMod;

	public bool CheckWeaponType;

	[ShowIf("CheckWeaponType")]
	public StarshipWeaponType WeaponType;

	[SerializeField]
	private BlueprintFeatureReference[] m_TargetAnyFeatureRequired = new BlueprintFeatureReference[0];

	public ReferenceArrayProxy<BlueprintFeature> TargetAnyFeatureRequired
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] targetAnyFeatureRequired = m_TargetAnyFeatureRequired;
			return targetAnyFeatureRequired;
		}
	}

	public void OnEventAboutToTrigger(RuleStarshipCalculateHitChances evt)
	{
		if ((modifyWhen != 0 || evt.Initiator == base.Owner) && (modifyWhen != ModifyWhen.IsTarget || evt.Target == base.Owner) && (!CheckWeaponType || evt.Weapon.Blueprint.WeaponType == WeaponType) && (TargetAnyFeatureRequired.Length == 0 || TargetAnyFeatureRequired.Any((BlueprintFeature feature) => evt.Target.Facts.Contains(feature))))
		{
			evt.BonusHitChance += HitBonusPct;
			evt.BonusCritChance += CritBonusPct;
			evt.CritAdditionalMod += CritBonusMod;
		}
	}

	public void OnEventDidTrigger(RuleStarshipCalculateHitChances evt)
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
