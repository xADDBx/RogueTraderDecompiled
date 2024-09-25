namespace Kingmaker.Blueprints.Quests.Logic;

public interface IQuestObjectiveLogic
{
	void OnStarted();

	void OnCompleted();

	void OnFailed();

	void OnBecameVisible();
}
