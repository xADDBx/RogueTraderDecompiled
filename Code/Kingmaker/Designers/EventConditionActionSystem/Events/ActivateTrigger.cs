using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/ActivateTrigger")]
[AllowMultipleComponents]
[TypeId("ae201267f654560479fee47303bc7b03")]
public class ActivateTrigger : EntityFactComponentDelegate, ITriggerOnLoad, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public bool AlreadyTriggered;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref AlreadyTriggered);
			return result;
		}
	}

	[SerializeField]
	private bool m_Once;

	[SerializeField]
	private bool m_AlsoOnAreaLoad;

	public ConditionsChecker Conditions;

	public ActionList Actions;

	public bool AlsoOnAreaLoad => m_AlsoOnAreaLoad;

	protected override void OnActivate()
	{
		Trigger();
	}

	private void Trigger()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if ((!m_Once || !savableData.AlreadyTriggered) && Conditions.Check())
		{
			Actions.Run();
			savableData.AlreadyTriggered = true;
		}
	}

	void ITriggerOnLoad.TriggerOnLoad()
	{
		if (AlsoOnAreaLoad)
		{
			Trigger();
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
