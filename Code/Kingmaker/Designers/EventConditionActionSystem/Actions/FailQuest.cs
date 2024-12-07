using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("31e36e8fc8d74f28a50994febb218ee9")]
public class FailQuest : GameAction
{
	[SerializeField]
	private bool m_FailSilently = true;

	[SerializeField]
	private BlueprintQuestReference? m_Quest;

	public override string GetDescription()
	{
		return "Fails given quest";
	}

	public override string GetCaption()
	{
		return "Fail " + m_Quest?.Get()?.name + " quest";
	}

	protected override void RunAction()
	{
		if (m_Quest != null)
		{
			GameHelper.Quests.GetQuest(m_Quest)?.FailQuest(m_FailSilently);
		}
	}
}
