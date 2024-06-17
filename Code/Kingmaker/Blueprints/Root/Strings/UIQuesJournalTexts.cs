using System;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIQuesJournalTexts
{
	public LocalizedString QuestComplite;

	public LocalizedString QuestFailed;

	public LocalizedString QuestTimeToFail;

	public LocalizedString QuestTimeToRealFail;

	public LocalizedString RumourPlaceMarker;

	public LocalizedString AllRumoursTitle;

	public LocalizedString RumoursAboutUsTitle;

	public LocalizedString OrderResourcesYourResources;

	public LocalizedString NoData;

	public LocalizedString RequiredResources;

	public LocalizedString RewardsResources;

	public LocalizedString CompleteOrder;

	public LocalizedString ShowCompletedQuests;

	public LocalizedString HideCompletedQuests;

	public LocalizedString Quests;

	public LocalizedString Rumours;

	public LocalizedString Orders;

	public LocalizedString NoNameOfTheListObjectsAvailable;

	public static UIQuesJournalTexts Instance => UIStrings.Instance.QuesJournalTexts;

	public string GetActiveTabLabel(JournalTab tab)
	{
		return tab switch
		{
			JournalTab.Quests => Quests, 
			JournalTab.Rumors => Rumours, 
			JournalTab.Orders => Orders, 
			_ => string.Empty, 
		};
	}
}
