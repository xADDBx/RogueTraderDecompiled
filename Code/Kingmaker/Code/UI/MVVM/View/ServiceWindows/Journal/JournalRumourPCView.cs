using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;

public class JournalRumourPCView : BaseJournalItemPCView
{
	[Header("Header")]
	[SerializeField]
	private ScrambledTMP m_TitleLabel;

	[SerializeField]
	private Image m_RumourAreaMarker;

	[SerializeField]
	private TextMeshProUGUI m_RumourAreaMarkerLabel;

	[Header("Completion")]
	[SerializeField]
	private GameObject m_CompletionItem;

	[SerializeField]
	private TextMeshProUGUI m_CompletionLabel;

	[Header("Content")]
	[SerializeField]
	private TextMeshProUGUI m_DescriptionLabel;

	[SerializeField]
	private Image m_DestinationImage;

	[SerializeField]
	private Sprite m_NoDataSprite;

	[SerializeField]
	private TextMeshProUGUI m_NoDataText;

	[SerializeField]
	private float m_DefaultFontSize = 21f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DescriptionLabel.fontSize = m_DefaultFontSize * base.ViewModel.FontMultiplier;
	}

	protected override void UpdateView()
	{
		SetupHeader();
		SetupBody();
		base.UpdateView();
	}

	private void SetupHeader()
	{
		m_TitleLabel.SetText(string.Empty, base.ViewModel.Title);
		m_RumourAreaMarker.gameObject.SetActive(base.ViewModel.IsAtDestinationSystem);
		m_RumourAreaMarker.SetHint(UIStrings.Instance.QuesJournalTexts.RumourPlaceMarker);
		if (m_RumourAreaMarkerLabel != null)
		{
			m_RumourAreaMarkerLabel.text = UIStrings.Instance.QuesJournalTexts.YouAreWithinRange;
		}
	}

	private void SetupBody()
	{
		SetTextItem(m_DescriptionLabel.gameObject, m_DescriptionLabel, base.ViewModel.Description);
		SetTextItem(m_CompletionItem, m_CompletionLabel, base.ViewModel.CompletionText);
		SetupStatuses();
		m_DestinationImage.sprite = (base.ViewModel.HasDestinationImage ? base.ViewModel.DestinationImage : m_NoDataSprite);
		m_NoDataText.gameObject.SetActive(!base.ViewModel.HasDestinationImage);
		m_NoDataText.text = string.Concat("|| \\\\ >", UIStrings.Instance.QuesJournalTexts.NoData, "< ---");
	}
}
