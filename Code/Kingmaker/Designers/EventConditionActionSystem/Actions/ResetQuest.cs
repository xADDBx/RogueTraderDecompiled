using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("2a4d2a4c33b3d6f4592972051e98bee3")]
public class ResetQuest : PlayerUpgraderOnlyAction
{
	[SerializeField]
	private BlueprintQuestReference m_Quest;

	[SerializeField]
	private BlueprintQuestObjectiveReference m_ObjectiveToStart;

	[SerializeField]
	private BlueprintQuestObjectiveReference[] m_ObjectivesToReset;

	public override string GetCaption()
	{
		if (!m_ObjectiveToStart.IsEmpty())
		{
			return "Reset quest " + m_Quest.NameSafe() + " at objective " + m_ObjectiveToStart.NameSafe();
		}
		return "Remove quest " + m_Quest.NameSafe() + " from journal";
	}

	public override string GetDescription()
	{
		if (!m_ObjectiveToStart.IsEmpty())
		{
			return "Restarts completed quest. Quest " + m_Quest.NameSafe() + " will become Started. Objectives in the list will be reset (remove from quest log), " + m_ObjectiveToStart.NameSafe() + " will be reset and then started as new.";
		}
		return "Removes quest " + m_Quest.NameSafe() + " from journal";
	}

	protected override void RunActionOverride()
	{
		Game.Instance.Player.QuestBook.ResetQuest(m_Quest.Get(), m_ObjectiveToStart.Get(), m_ObjectivesToReset.Dereference());
	}
}
