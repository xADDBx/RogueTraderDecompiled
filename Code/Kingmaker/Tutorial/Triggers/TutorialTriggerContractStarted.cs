using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("e3bbbe53e6da4f9083fd822f53a9256d")]
public class TutorialTriggerContractStarted : TutorialTrigger, IQuestHandler, ISubscriber, IHashable
{
	public void HandleQuestStarted(Quest quest)
	{
		if (quest.Blueprint.Type == QuestType.Order)
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
