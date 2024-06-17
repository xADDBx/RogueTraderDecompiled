using DG.Tweening;
using Kingmaker.UI.Common;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;

public class JournalQuestConsoleView : BaseJournalItemConsoleView
{
	[Header("Objectives")]
	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Header")]
	[SerializeField]
	private ScrambledTMP m_TitleLabel;

	[Header("Location Info")]
	[SerializeField]
	private TextMeshProUGUI m_PlaceLabel;

	[Header("Completion")]
	[SerializeField]
	private GameObject m_CompletionItem;

	[SerializeField]
	private TextMeshProUGUI m_CompletionLabel;

	[Header("Content")]
	[SerializeField]
	private TextMeshProUGUI m_ServiceMessageLabel;

	[SerializeField]
	private TextMeshProUGUI m_DescriptionLabel;

	[Header("Nomos")]
	[SerializeField]
	private GameObject m_NomosTag;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 21f;

	[SerializeField]
	private RectTransform m_LocationGroup;

	[SerializeField]
	private RectTransform m_EagleGroup;

	[SerializeField]
	private RectTransform m_EagleImage;

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
		m_LocationGroup.gameObject.SetActive(!string.IsNullOrWhiteSpace(base.ViewModel.Place));
		m_EagleGroup.gameObject.SetActive(string.IsNullOrWhiteSpace(base.ViewModel.Place));
		m_PlaceLabel.text = base.ViewModel.Place;
		PlayEagleAnimation();
	}

	private void SetupBody()
	{
		SetTextItem(m_ServiceMessageLabel.gameObject, m_ServiceMessageLabel, base.ViewModel.ServiceMessage);
		SetTextItem(m_DescriptionLabel.gameObject, m_DescriptionLabel, base.ViewModel.Description);
		SetTextItem(m_CompletionItem, m_CompletionLabel, base.ViewModel.CompletionText);
		m_NomosTag.SetActive(base.ViewModel.IsAffectedByNomos);
		SetupStatuses();
	}

	private void PlayEagleAnimation()
	{
		if (string.IsNullOrWhiteSpace(base.ViewModel.Place))
		{
			bool flag = m_EagleImage.eulerAngles.y == 0f;
			m_EagleImage.DORotate(new Vector3(0f, flag ? 360f : 0f), 1f).SetUpdate(isIndependentUpdate: true);
		}
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
