using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[AllowMultipleComponents]
[TypeId("785928ab10ac40b19a015b8e444261a9")]
[ComponentName("Events/FailQuestByEtudeTrigger")]
public class FailQuestByEtudeTrigger : QuestComponentDelegate, IEtudesUpdateHandler, ISubscriber, IHashable
{
	[SerializeField]
	private bool m_FailSilently = true;

	[SerializeField]
	private BlueprintEtudeReference? m_Etude;

	public void OnEtudesUpdate()
	{
		if (m_Etude != null && base.Quest.State != QuestState.Failed && Game.Instance.Player.EtudesSystem.EtudeIsCompleted(m_Etude.Get()))
		{
			base.Quest.FailQuest(m_FailSilently);
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
