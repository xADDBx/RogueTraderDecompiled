using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Units;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("0cc349250bab417c91ccd0b93481c3bb")]
public class MomentumReachedTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformMomentumChange>, IRulebookHandler<RulePerformMomentumChange>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public class Data : IEntityFactComponentSavableData, IHashable
	{
		public int MomentumMaximumThisCombat { get; set; }

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public ActionList Actions;

	public int MaximumToReach;

	public void OnEventAboutToTrigger(RulePerformMomentumChange evt)
	{
	}

	public void OnEventDidTrigger(RulePerformMomentumChange evt)
	{
		MomentumGroup group = Game.Instance.TurnController.MomentumController.GetGroup(base.Owner);
		Data data = RequestSavableData<Data>();
		if (!evt.Initiator.IsPlayerFaction || group == null)
		{
			return;
		}
		int momentum = group.Momentum;
		if (momentum > data.MomentumMaximumThisCombat && momentum >= MaximumToReach)
		{
			ActionList actions = Actions;
			if (actions != null && actions.HasActions)
			{
				base.Fact.RunActionInContext(Actions, base.Owner);
			}
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
