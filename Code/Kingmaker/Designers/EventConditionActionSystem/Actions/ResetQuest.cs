using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[KDB("Событие для сброса состояния выбранного квеста.")]
[TypeId("2a4d2a4c33b3d6f4592972051e98bee3")]
public class ResetQuest : GameAction
{
	[SerializeField]
	private BlueprintQuestReference m_Quest;

	[KDB("Какие типы под-обжективов в указанном квесте надо сбросить, кроме сброса самих обжективов.")]
	[SerializeField]
	[HideIf("IsObjectiveToStartFilled")]
	private ResetableSubobjectiveTypes m_subobjectiveTypesToReset;

	[KDB("Данный обжектив сначала будет сброшен, а потом начат заново и сделан стартовым для указанного квеста.")]
	[SerializeField]
	private BlueprintQuestObjectiveReference m_ObjectiveToStart;

	[KDB("Доп. обжективы, которые должны сброситься, помимо ObjectiveToStart.")]
	[SerializeField]
	[ShowIf("IsObjectiveToStartFilled")]
	private BlueprintQuestObjectiveReference[] m_ObjectivesToReset;

	private bool IsObjectiveToStartFilled => !m_ObjectiveToStart.IsEmpty();

	public override string GetCaption()
	{
		if (!IsObjectiveToStartFilled)
		{
			return "Remove quest " + m_Quest.NameSafe() + " from journal";
		}
		return "Reset quest " + m_Quest.NameSafe() + " at objective " + m_ObjectiveToStart.NameSafe();
	}

	public override string GetDescription()
	{
		if (!IsObjectiveToStartFilled)
		{
			return "Removes quest " + m_Quest.NameSafe() + " from journal";
		}
		return string.Concat("Restarts completed quest. Quest " + m_Quest.NameSafe() + " will become Started. ", "Objectives in the list ObjectivesToReset will be reset (remove from quest log), ", m_ObjectiveToStart.NameSafe() + " will be reset and then started as new.");
	}

	protected override void RunAction()
	{
		if (m_ObjectiveToStart.IsEmpty())
		{
			Game.Instance.Player.QuestBook.ResetQuest(m_Quest.Get(), m_subobjectiveTypesToReset);
		}
		else
		{
			Game.Instance.Player.QuestBook.ResetQuest(m_Quest.Get(), m_ObjectiveToStart.Get(), m_ObjectivesToReset.Dereference());
		}
	}
}
