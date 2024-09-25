using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[TypeId("41193b1de03a67246ab85edc85783d56")]
public class TimeOfDayChangedTrigger : EntityFactComponentDelegate, ITimeOfDayChangedHandler, ISubscriber, IHashable
{
	public ActionList Actions;

	public void OnTimeOfDayChanged()
	{
		Actions.Run();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
