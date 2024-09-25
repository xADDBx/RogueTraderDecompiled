using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[AllowMultipleComponents]
[TypeId("95e41903dacc47f3a21c98338688266f")]
public class AreaTransitionTrigger : EntityFactComponentDelegate, IAreaTransitionHandler, ISubscriber, IHashable
{
	public ActionList Actions;

	public ConditionsChecker Conditions;

	public void HandleAreaTransition()
	{
		using (base.Fact.MaybeContext?.GetDataScope())
		{
			if (Conditions.Check())
			{
				Actions.Run();
			}
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
