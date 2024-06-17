using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("d6c024dd16fc41fd8767a5dc61f18a2e")]
public class IsRumourFalse : Condition
{
	[SerializeField]
	private BlueprintQuestReference m_Rumour;

	protected override string GetConditionCaption()
	{
		return "Check if player complete rumour and it is false";
	}

	protected override bool CheckCondition()
	{
		if (m_Rumour.Get().IsRumourFalse)
		{
			return Game.Instance.Player.QuestBook.GetQuestState(m_Rumour.Get()) == QuestState.Completed;
		}
		return false;
	}
}
