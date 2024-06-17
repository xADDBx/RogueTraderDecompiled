using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("0faf2e62c7ca4e688000ecbc439688f5")]
public class TutorialTriggerRumourStarted : TutorialTrigger, IQuestHandler, ISubscriber, IHashable
{
	public void HandleQuestStarted(Quest quest)
	{
		if (quest.Blueprint.Type == QuestType.Rumour || quest.Blueprint.Type == QuestType.RumourAboutUs)
		{
			TryToTrigger(null);
		}
	}

	public void HandleQuestCompleted(Quest objective)
	{
	}

	public void HandleQuestFailed(Quest objective)
	{
	}

	public void HandleQuestUpdated(Quest objective)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
