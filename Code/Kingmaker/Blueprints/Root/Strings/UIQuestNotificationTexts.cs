using System;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Kingmaker.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIQuestNotificationTexts
{
	public LocalizedString QuestComplite;

	public LocalizedString QuestFailed;

	public LocalizedString QuestNew;

	public LocalizedString QuestUpdate;

	public LocalizedString QuestStarted;

	public LocalizedString QuestPostponed;

	public LocalizedString ToJournal;

	public LocalizedString Quest;

	public LocalizedString Rumour;

	public LocalizedString Order;

	public LocalizedString Failed;

	public LocalizedString Completed;

	public LocalizedString New;

	public LocalizedString Updated;

	public LocalizedString Postponed;

	public static UIQuestNotificationTexts Instance => UIStrings.Instance.QuestNotificationTexts;

	public string GetQuestStateText(QuestState state)
	{
		return state switch
		{
			QuestState.None => "", 
			QuestState.Started => "", 
			QuestState.Completed => QuestComplite, 
			QuestState.Postponed => QuestPostponed, 
			QuestState.Failed => QuestFailed, 
			_ => "", 
		};
	}

	public string GetQuestStateText(int state, QuestType type, QuestGroupId group)
	{
		string text = ((type == QuestType.Rumour || type == QuestType.RumourAboutUs || group == QuestGroupId.Rumours) ? Rumour : ((type == QuestType.Order || group == QuestGroupId.Orders) ? Order : Quest));
		return state switch
		{
			0 => string.Concat(New, " ", text), 
			1 => text + " " + Updated, 
			2 => text + " " + Completed, 
			3 => text + " " + Failed, 
			_ => string.Empty, 
		};
	}

	public string GetQuestHintStateText(int state, QuestType type, QuestGroupId group)
	{
		string text = ((type == QuestType.Rumour || type == QuestType.RumourAboutUs || group == QuestGroupId.Rumours) ? Rumour : ((type == QuestType.Order || group == QuestGroupId.Orders) ? Order : Quest));
		return state switch
		{
			0 => string.Concat(New, " ", text), 
			1 => QuestStarted, 
			2 => text + " " + Completed, 
			3 => text + " " + Failed, 
			4 => text + " " + Updated, 
			5 => text + " " + Postponed, 
			_ => string.Empty, 
		};
	}

	public string GetQuestNotificationStateText(QuestNotificationState state, QuestType type, QuestGroupId group)
	{
		string text = ((type == QuestType.Rumour || type == QuestType.RumourAboutUs || group == QuestGroupId.Rumours) ? Rumour : ((type == QuestType.Order || group == QuestGroupId.Orders) ? Order : Quest));
		return state switch
		{
			QuestNotificationState.Failed => text + " " + Failed, 
			QuestNotificationState.Completed => text + " " + Completed, 
			QuestNotificationState.Updated => text + " " + Updated, 
			QuestNotificationState.Postponed => text + " " + Postponed, 
			_ => string.Concat(New, " ", text), 
		};
	}
}
