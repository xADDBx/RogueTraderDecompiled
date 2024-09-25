using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[TypeId("9dfeedc766aac634eb143f32be1dd4da")]
public class ExperienceTrigger : EntityFactComponentDelegate, IHashable
{
	public int Experience;

	public ConditionsChecker Conditions;

	public ActionList Actions;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
