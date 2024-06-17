using System;
using Newtonsoft.Json;

namespace Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;

[Serializable]
public sealed class FeedbackPopupItem
{
	[JsonProperty]
	public FeedbackPopupItemType ItemType;

	[JsonProperty]
	public string Url;
}
