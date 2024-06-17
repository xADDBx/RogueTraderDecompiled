using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Legacy.MainMenuUI;

public class MotivationLinkView : MonoBehaviour
{
	public void ClickLink(PointerEventData eventData, TMP_LinkInfo linkInfo)
	{
		if (linkInfo.GetLinkID() == "vote")
		{
			Application.OpenURL("https://owlcatgames.com/vote");
		}
	}
}
