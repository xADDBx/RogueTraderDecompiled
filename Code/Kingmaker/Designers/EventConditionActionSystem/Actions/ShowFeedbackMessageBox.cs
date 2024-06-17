using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.UI.Common;
using TMPro;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("f774341e970749e3b395bd3e9c56b640")]
public class ShowFeedbackMessageBox : ShowMessageBox
{
	public override string GetCaption()
	{
		return "Show feedback message box";
	}

	public override void RunAction()
	{
		UIUtility.ShowMessageBox(Text, DialogMessageBoxBase.BoxType.Message, delegate
		{
			OnClose.Run();
		}, OnLinkInvoke, null, null, WaitTime);
	}

	private void OnLinkInvoke(TMP_LinkInfo linkInfo)
	{
		FeedbackPopupItem value;
		if (!Enum.TryParse<FeedbackPopupItemType>(linkInfo.GetLinkID(), out var result))
		{
			PFLog.UI.Error("Cannot parse link feedback type!");
		}
		else if (!FeedbackPopupConfigLoader.Instance.ItemsMap.TryGetValue(result, out value))
		{
			PFLog.UI.Error($"Cannot get feedback item with type: {result}!");
		}
		else
		{
			Application.OpenURL(value.Url);
		}
	}
}
