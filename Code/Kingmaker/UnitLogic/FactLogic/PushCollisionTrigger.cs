using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("0f0841e92e334b1ab0f2205d7462f313")]
public class PushCollisionTrigger : UnitBuffComponentDelegate, IGlobalRulebookHandler<RulePerformCollision>, IRulebookHandler<RulePerformCollision>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public ContextValue ValueMultiplier;

	public ContextValue ValueBonus;

	public ContextPropertyName ContextPropertyName;

	public bool OnlyFromOwner;

	public ActionList Actions;

	public void OnEventAboutToTrigger(RulePerformCollision evt)
	{
	}

	public void OnEventDidTrigger(RulePerformCollision evt)
	{
		if (OnlyFromOwner && evt.Pusher != base.Owner)
		{
			return;
		}
		base.Context[ContextPropertyName] = evt.ResultDamage * ValueMultiplier.Calculate(base.Context) + ValueBonus.Calculate(base.Context);
		using (base.Fact.MaybeContext?.GetDataScope(base.Owner))
		{
			base.Fact.RunActionInContext(Actions, evt.Pushed);
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
