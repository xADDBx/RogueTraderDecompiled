using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;

public class BaseJournalItemBaseView : ViewBase<JournalQuestVM>
{
	[Header("Animator")]
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[Header("Layout Groups")]
	[SerializeField]
	private RectTransform m_HeaderRectTransform;

	[SerializeField]
	private RectTransform m_ContentRectTransform;

	private bool m_IsInit;

	private bool m_ContentRefreshing;

	private bool m_IsShowed;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			m_FadeAnimator.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		if (m_IsShowed)
		{
			ContentUpdate();
		}
		else
		{
			Show();
		}
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		m_ContentRefreshing = false;
	}

	private void ContentUpdate()
	{
		if (!m_ContentRefreshing)
		{
			m_ContentRefreshing = true;
			m_FadeAnimator.DisappearAnimation(delegate
			{
				m_FadeAnimator.AppearAnimation();
				UpdateView();
				m_ContentRefreshing = false;
			});
		}
	}

	private void Show()
	{
		if (!m_ContentRefreshing)
		{
			m_ContentRefreshing = true;
			m_FadeAnimator.AppearAnimation();
			UpdateView();
			m_IsShowed = true;
			m_ContentRefreshing = false;
		}
	}

	private void Hide()
	{
		m_FadeAnimator.DisappearAnimation();
		m_IsShowed = false;
	}

	protected virtual void UpdateView()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_HeaderRectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_ContentRectTransform);
	}

	protected void SetTextItem(GameObject itemGO, TextMeshProUGUI itemLabel, string itemString)
	{
		if (!string.IsNullOrEmpty(itemString))
		{
			itemGO.SetActive(value: true);
			itemLabel.text = itemString;
		}
		else
		{
			itemGO.SetActive(value: false);
		}
	}
}
