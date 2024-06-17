using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests.Logic;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Quests.Common;

[ComponentName("Common/GiveUnlockOnObjectiveTrigger")]
[TypeId("77e67c40f94b19f4595af830f13a59c1")]
public class GiveUnlockOnObjectiveTrigger : QuestObjectiveComponentDelegate, IQuestObjectiveLogic, IUnlockableFlagReference, IHashable
{
	public QuestObjectiveState objectiveState;

	[SerializeField]
	[FormerlySerializedAs("unlock")]
	private BlueprintUnlockableFlagReference m_unlock;

	public BlueprintUnlockableFlag unlock => m_unlock?.Get();

	void IQuestObjectiveLogic.OnStarted()
	{
		if (objectiveState == QuestObjectiveState.Started)
		{
			unlock.Unlock();
		}
	}

	void IQuestObjectiveLogic.OnCompleted()
	{
		if (objectiveState == QuestObjectiveState.Completed)
		{
			unlock.Unlock();
		}
	}

	void IQuestObjectiveLogic.OnFailed()
	{
		if (objectiveState == QuestObjectiveState.Failed)
		{
			unlock.Unlock();
		}
	}

	void IQuestObjectiveLogic.OnBecameVisible()
	{
	}

	public override string GetDescription()
	{
		return $"Unlock Flag {unlock} when {objectiveState}";
	}

	UnlockableFlagReferenceType IUnlockableFlagReference.GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (unlock == flag)
		{
			return UnlockableFlagReferenceType.Unlock;
		}
		return UnlockableFlagReferenceType.None;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
