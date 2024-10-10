using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[AllowMultipleComponents]
[TypeId("d3e4fc9142494f99a469c1dd43abe255")]
public class AreaDidLoadTrigger : EntityFactComponentDelegate, IAreaActivationHandler, ISubscriber, IHashable
{
	public ActionList Actions;

	public ConditionsChecker Conditions;

	public void OnAreaActivated()
	{
		if ((bool)ContextData<UnitHelper.PreviewUnit>.Current || base.Owner.IsPreview || !base.Owner.IsInGame)
		{
			return;
		}
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
