using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("dc5eb27f0bb8409581dc204d42dc5a5d")]
public class IncreaseAbilityGroupRange : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityRange>, IRulebookHandler<RuleCalculateAbilityRange>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	public ContextValue AdditionalRange;

	public bool OnlyNonMelee;

	[SerializeField]
	private BlueprintAbilityGroupReference m_AbilityGroup;

	public BlueprintAbilityGroup AbilityGroup => m_AbilityGroup.Get();

	public void OnEventAboutToTrigger(RuleCalculateAbilityRange evt)
	{
		if (!Restrictions.IsPassed(base.Fact, evt, evt.Ability) || (AbilityGroup != null && !evt.Ability.AbilityGroups.Contains(AbilityGroup)))
		{
			return;
		}
		if (AdditionalRange == null)
		{
			PFLog.Default.Error("Range calculator is missing");
			return;
		}
		if (OnlyNonMelee)
		{
			if (evt.Ability.Blueprint.Range == AbilityRange.Touch || (evt.Ability.Blueprint.Range == AbilityRange.Custom && evt.Ability.Blueprint.CustomRange <= 1))
			{
				return;
			}
			if (evt.Ability.Blueprint.Range == AbilityRange.Weapon)
			{
				ItemEntityWeapon weapon = evt.Ability.Weapon;
				if (weapon == null || weapon.Blueprint.Range != WeaponRange.Ranged)
				{
					return;
				}
			}
		}
		new MechanicsContext(evt.ConcreteInitiator, evt.ConcreteInitiator, base.Fact.Blueprint);
		evt.Bonus += AdditionalRange.Calculate(base.Context);
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
