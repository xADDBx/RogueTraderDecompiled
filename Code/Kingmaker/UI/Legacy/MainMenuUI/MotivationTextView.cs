using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Stores;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Legacy.MainMenuUI;

public class MotivationTextView : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	[UsedImplicitly]
	private string m_SteamLink;

	[SerializeField]
	[UsedImplicitly]
	private string m_GogLink;

	private UIMeinMenuTexts t => UIStrings.Instance.MainMenu;

	public void Awake()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(t.MotivationStartPart).AppendLine().AppendLine();
		switch (StoreManager.Store)
		{
		case StoreType.Steam:
			stringBuilder.AppendFormat(t.MotivationStartPartFormat, string.Format(t.MotivationLinkPartFormat, m_SteamLink));
			break;
		case StoreType.GoG:
			stringBuilder.AppendFormat(t.MotivationStartPartFormat, string.Format(t.MotivationLinkPartFormat, m_GogLink));
			break;
		default:
			stringBuilder.AppendFormat(t.MotivationStartPartFormat, string.Empty);
			break;
		}
		stringBuilder.AppendLine().AppendLine().Append(t.MotivationEndPart);
		m_Label.text = stringBuilder.ToString();
	}

	public void OnLinkInvoke(PointerEventData eventData, TMP_LinkInfo linkInfo)
	{
		string linkID = linkInfo.GetLinkID();
		if (!(linkID == "steam"))
		{
			if (linkID == "gog")
			{
				Application.OpenURL("https://www.gog.com/game/pathfinder_kingmaker_explorer_edition");
			}
		}
		else
		{
			Application.OpenURL("https://store.steampowered.com/recommended/recommendgame/640820");
		}
	}
}
