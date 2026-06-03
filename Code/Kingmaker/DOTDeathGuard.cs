using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker;

[TypeId("70e14da28ce0e9b4eb95e36fda098020")]
public class DOTDeathGuard : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	[SerializeField]
	private DOT[] Types;

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
		if (evt.Reason.Fact != null)
		{
			DOTLogic component = evt.Reason.Fact.GetComponent<DOTLogic>();
			if (component != null && Types.Contains(component.Type))
			{
				evt.CantKillTarget = true;
			}
		}
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
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
