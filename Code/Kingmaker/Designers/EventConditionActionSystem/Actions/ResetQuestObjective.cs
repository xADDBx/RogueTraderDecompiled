using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("f0af50224a8742b5ba869f84d7e3f8d5")]
public class ResetQuestObjective : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[FormerlySerializedAs("Objective")]
	private BlueprintQuestObjectiveReference m_Objective;

	public BlueprintQuestObjective Objective => m_Objective?.Get();

	public override string GetCaption()
	{
		return $"Reset quest objective {Objective}";
	}

	protected override void RunActionOverride()
	{
		Game.Instance.Player.QuestBook.ResetObjective(Objective);
	}
}
