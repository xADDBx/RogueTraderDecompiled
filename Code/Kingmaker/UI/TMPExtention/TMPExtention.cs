using System;
using TMPro;
using UniRx;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.TMPExtention;

public static class TMPExtention
{
	public static bool IsAnyVisibleChars(this TextMeshProUGUI text)
	{
		if (text == null)
		{
			return false;
		}
		if (text.textInfo.characterCount <= 0)
		{
			return false;
		}
		return Array.Exists(text.textInfo.characterInfo, (TMP_CharacterInfo x) => x.isVisible);
	}

	public static IObservable<Tuple<PointerEventData, TMP_LinkInfo>> OnClickAsObservable(this TMPLinkHandler linkHandler)
	{
		return linkHandler.OnClick.AsObservable();
	}
}
