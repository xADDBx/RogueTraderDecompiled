namespace Kingmaker.Blueprints.Quests.Logic;

public interface IQuestLogic
{
	void OnStarted();

	void OnCompleted();

	void OnFailed();
}
