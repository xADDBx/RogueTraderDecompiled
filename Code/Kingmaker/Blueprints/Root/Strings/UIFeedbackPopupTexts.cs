using System;
using Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIFeedbackPopupTexts
{
	public LocalizedString Survey;

	public LocalizedString Discord;

	public LocalizedString Twitter;

	public LocalizedString Facebook;

	public LocalizedString Website;

	public string GetTitleByPopupItemType(FeedbackPopupItemType type)
	{
		return type switch
		{
			FeedbackPopupItemType.Survey => Survey, 
			FeedbackPopupItemType.Discord => Discord, 
			FeedbackPopupItemType.Twitter => Twitter, 
			FeedbackPopupItemType.Facebook => Facebook, 
			FeedbackPopupItemType.Website => Website, 
			_ => string.Empty, 
		};
	}
}
