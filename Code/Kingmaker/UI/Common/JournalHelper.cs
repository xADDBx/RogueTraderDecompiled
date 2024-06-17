using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.GameCommands;

namespace Kingmaker.UI.Common;

public static class JournalHelper
{
	public static Quest CurrentQuest => Game.Instance.Player.UISettings.CurrentQuest;

	public static bool HasCurrentQuest => CurrentQuest != null;

	public static bool IsCurrentQuest(this Quest quest)
	{
		return quest == CurrentQuest;
	}

	public static bool ChangeCurrentQuest(Quest quest)
	{
		if (quest.IsCurrentQuest())
		{
			return false;
		}
		Game.Instance.GameCommandQueue.SetCurrentQuest(quest);
		return true;
	}
}
