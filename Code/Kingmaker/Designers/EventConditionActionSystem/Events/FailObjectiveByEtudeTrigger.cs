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
[TypeId("60df93db482a47b19ebb4153181b2204")]
[ComponentName("Events/FailObjectiveByEtudeTrigger")]
public class FailObjectiveByEtudeTrigger : QuestObjectiveComponentDelegate, IEtudesUpdateHandler, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintEtudeReference? m_Etude;

	public void OnEtudesUpdate()
	{
		if (m_Etude != null && base.Objective.State != QuestObjectiveState.Failed && Game.Instance.Player.EtudesSystem.EtudeIsCompleted(m_Etude.Get()))
		{
			base.Objective.Fail();
		}
	}

	public override string GetDescription()
	{
		return "Completes objective when etude " + m_Etude?.NameSafe() + " is complete";
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
