using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/DeactivateTrigger")]
[AllowMultipleComponents]
[TypeId("b2970e0dadd194546bbf27a691e55d4c")]
public class DeactivateTrigger : EntityFactComponentDelegate, IHashable
{
	public ConditionsChecker Conditions;

	public ActionList Actions;

	protected override void OnDeactivate()
	{
		if (Conditions.Check())
		{
			Actions.Run();
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
