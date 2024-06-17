using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("880a006aa0534ca2b04b15789a4e62b5")]
public class MomentumChangedTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformMomentumChange>, IRulebookHandler<RulePerformMomentumChange>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public ActionList Actions;

	[SerializeField]
	private bool m_RunActionsOnFactOwner;

	public void OnEventAboutToTrigger(RulePerformMomentumChange evt)
	{
	}

	public void OnEventDidTrigger(RulePerformMomentumChange evt)
	{
		MechanicEntity mechanicEntity = (m_RunActionsOnFactOwner ? base.Owner : base.Context.MaybeCaster);
		if (mechanicEntity != null)
		{
			base.Fact.RunActionInContext(Actions, mechanicEntity.ToITargetWrapper());
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
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
