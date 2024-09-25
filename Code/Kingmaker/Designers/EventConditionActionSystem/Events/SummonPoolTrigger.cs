using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/SummonPoolTrigger")]
[AllowMultipleComponents]
[TypeId("5ce1080e9c809614daae11db4baa37a4")]
public class SummonPoolTrigger : EntityFactComponentDelegate, ISummonPoolHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public enum ChangeTypes
	{
		Both,
		Ascending,
		Descending
	}

	[FormerlySerializedAs("count")]
	public int Count;

	public ChangeTypes ChangeType;

	[FormerlySerializedAs("summonPool")]
	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	public ConditionsChecker Conditions;

	public ActionList Actions;

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	public void HandleUnitAdded(ISummonPool pool)
	{
		if (ChangeType == ChangeTypes.Ascending || ChangeType == ChangeTypes.Both)
		{
			OnPoolChanged(pool);
		}
	}

	public void HandleUnitRemoved(ISummonPool pool)
	{
		if (ChangeType == ChangeTypes.Descending || ChangeType == ChangeTypes.Both)
		{
			OnPoolChanged(pool);
		}
	}

	public void HandleLastUnitRemoved(ISummonPool pool)
	{
	}

	private void OnPoolChanged(ISummonPool pool)
	{
		if (SummonPool == pool.Blueprint && pool.Count == Count && Conditions.Check())
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
