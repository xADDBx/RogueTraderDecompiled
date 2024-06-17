using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("5a67d0c6937040a6a5b86c774aa1d6c4")]
public abstract class ForceMoveTrigger : MechanicEntityFactComponentDelegate, IHashable
{
	protected enum UnitForceMoveType
	{
		Push,
		Overpenetration
	}

	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	protected UnitForceMoveType ForceMoveType;

	protected void TryTrigger(RulePerformAttack rule)
	{
		if (Restrictions.IsPassed(base.Fact, rule, rule.Ability) && rule.ResultDamageRule != null && (ForceMoveType != 0 || !rule.IsOverpenetration) && (ForceMoveType != UnitForceMoveType.Overpenetration || rule.IsOverpenetration))
		{
			OnTrigger(rule);
		}
	}

	protected abstract void OnTrigger(RulePerformAttack rule);

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
