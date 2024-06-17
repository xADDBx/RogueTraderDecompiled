using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.GameCommands;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.QuestNotification;

public class QuestNotificationEntityVM : VMBase
{
	public readonly Quest Quest;

	public readonly QuestNotificationState State;

	public readonly string QuestName;

	public readonly string Title;

	public readonly string Description;

	public readonly bool IsAddendum;

	public readonly bool IsErrandObjective;

	public readonly ReactiveProperty<QuestNotificationEntityVM> AdditionalObjective = new ReactiveProperty<QuestNotificationEntityVM>();

	public QuestNotificationEntityVM(QuestObjective objective, QuestNotificationState state)
	{
		Quest = objective.Quest;
		State = state;
		QuestName = objective.Quest.Blueprint.Title;
		IsAddendum = objective.Blueprint.IsAddendum;
		IsErrandObjective = objective.Blueprint.IsErrandObjective;
		Description = objective.Blueprint.GetDescription();
		Title = objective.Blueprint.GetTitile();
	}

	public void AddObjective(QuestNotificationEntityVM objectiveVM)
	{
		AdditionalObjective.Value?.Dispose();
		AdditionalObjective.Value = objectiveVM;
	}

	public void SetCurrentQuest()
	{
		GameCommandHelper.SetCurrentQuest(Quest);
	}

	protected override void DisposeImplementation()
	{
		AdditionalObjective.Value?.Dispose();
		AdditionalObjective.Value = null;
	}
}
