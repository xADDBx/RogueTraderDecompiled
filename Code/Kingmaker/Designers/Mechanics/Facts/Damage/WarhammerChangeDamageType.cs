using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[TypeId("ce330f021a5d43ce8f3aa7ad3cb0b6bb")]
public class WarhammerChangeDamageType : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool CheckDamageType;

	[SerializeField]
	[ShowIf("CheckDamageType")]
	private DamageType m_InitialDamageType;

	public DamageType OverrideDamageType;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (Restrictions.IsPassed(base.Fact))
		{
			evt.OverrideDamageType = OverrideDamageType;
			evt.CheckDamageType = (CheckDamageType ? new DamageType?(m_InitialDamageType) : null);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
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
