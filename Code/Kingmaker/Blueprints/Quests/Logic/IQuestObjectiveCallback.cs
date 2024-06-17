namespace Kingmaker.Blueprints.Quests.Logic;

public interface IQuestObjectiveCallback
{
	void OnComplete();

	void OnFail();
}
