using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker;

[TypeId("d1991b323914d1c46aaf338b030ffaab")]
public abstract class EnableRicochet : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private bool m_DisableFriendlyFire;

	[SerializeField]
	private RestrictionCalculator m_Restrictions;

	public bool DisableFriendlyFire => m_DisableFriendlyFire;

	protected void ApplyToEvent(RuleCalculateOverpenetration evt)
	{
		PropertyContext context = new PropertyContext(evt.ConcreteInitiator, null, evt.MaybeTarget, null, evt, evt.Ability);
		if (m_Restrictions.IsPassed(context))
		{
			evt.OverpenetrationDamage.IsRicochet = true;
			evt.OverpenetrationDamage.IsRicochetFriendlyFireDisabled = DisableFriendlyFire;
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
