using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.QuestNotification;

public class QuestNotificationQuestVM : VMBase
{
	public readonly Quest Quest;

	public readonly QuestNotificationState State;

	public readonly string Title;

	public readonly string Description;

	public QuestNotificationQuestVM(Quest quest, QuestNotificationState state)
	{
		Quest = quest;
		State = state;
		Title = quest.Blueprint.Title;
		Description = ((quest.State == QuestState.Completed && quest.Blueprint.CompletionText.IsSet()) ? quest.Blueprint.CompletionText : quest.Blueprint.Description);
	}

	public void SetCurrentQuest()
	{
		JournalHelper.ChangeCurrentQuest(Quest);
	}

	protected override void DisposeImplementation()
	{
	}
}
