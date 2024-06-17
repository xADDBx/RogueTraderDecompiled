using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;

public class JournalRumourConsoleView : BaseJournalItemConsoleView
{
	[Header("Objectives")]
	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Header")]
	[SerializeField]
	private ScrambledTMP m_TitleLabel;

	[SerializeField]
	private Image m_RumourAreaMarker;

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
	private float m_DefaultConsoleFontSize = 21f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DescriptionLabel.fontSize = m_DefaultConsoleFontSize * base.ViewModel.FontMultiplier;
	}

	protected override void UpdateView()
	{
		SetupHeader();
		SetupBody();
		ScrollToTop();
		base.UpdateView();
	}

	private void SetupHeader()
	{
		m_TitleLabel.SetText(string.Empty, base.ViewModel.Title);
		m_RumourAreaMarker.gameObject.SetActive(base.ViewModel.IsAtDestinationSystem);
		m_RumourAreaMarker.SetHint(UIStrings.Instance.QuesJournalTexts.RumourPlaceMarker);
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

	public void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}
}
