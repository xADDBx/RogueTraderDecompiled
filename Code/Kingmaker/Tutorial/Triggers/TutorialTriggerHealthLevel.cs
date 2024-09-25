using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("8330e08ab1e3406e8f1f77995e88afd8")]
public class TutorialTriggerHealthLevel : TutorialTrigger, IDamageHandler, ISubscriber, IHashable
{
	[SerializeField]
	private float m_Value = 0.5f;

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if (dealDamage.Target.IsPlayerFaction && (float?)dealDamage.TargetHealth?.HitPointsLeft < (float?)dealDamage.TargetHealth?.MaxHitPoints * m_Value && !Game.Instance.IsSpaceCombat)
		{
			TryToTrigger(dealDamage);
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
