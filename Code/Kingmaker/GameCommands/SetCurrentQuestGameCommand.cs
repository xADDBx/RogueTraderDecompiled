using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.EntitySystem;

namespace Kingmaker.GameCommands;

public sealed class SetCurrentQuestGameCommand : GameCommand
{
	private EntityFactRef<Quest> m_QuestRef;

	public SetCurrentQuestGameCommand(Quest quest)
	{
		m_QuestRef = quest;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.SetCurrentQuest(m_QuestRef.Fact);
	}
}
