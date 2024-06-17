using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Quests.Common;

[ComponentName("Common/SummonPoolCountTrigger")]
[TypeId("957b88bbe2be71a41863b5d635f54b9c")]
public class SummonPoolCountTrigger : QuestObjectiveComponentDelegate, ISummonPoolHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public enum ObjectiveStatus
	{
		Complete,
		Fail
	}

	public int count;

	public ObjectiveStatus setStatus;

	[SerializeField]
	[FormerlySerializedAs("summonPool")]
	private BlueprintSummonPoolReference m_summonPool;

	public BlueprintSummonPool summonPool => m_summonPool?.Get();

	public void HandleUnitAdded(ISummonPool pool)
	{
		OnPoolChanged(pool);
	}

	public void HandleUnitRemoved(ISummonPool pool)
	{
		OnPoolChanged(pool);
	}

	public void HandleLastUnitRemoved(ISummonPool pool)
	{
	}

	private void OnPoolChanged(ISummonPool pool)
	{
		if (summonPool == pool.Blueprint && pool.Count == count)
		{
			ChangeObjectiveStatus();
		}
	}

	private void ChangeObjectiveStatus()
	{
		switch (setStatus)
		{
		case ObjectiveStatus.Complete:
			base.Objective.Complete();
			break;
		case ObjectiveStatus.Fail:
			base.Objective.Fail();
			break;
		}
	}

	public override string GetDescription()
	{
		return $"{setStatus} when {summonPool} count is {count}";
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
