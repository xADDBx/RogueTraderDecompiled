using System;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(Scrollbar))]
public class ScrollbarExtensions : MonoBehaviour
{
	[SerializeField]
	private OwlcatButton m_ScrollUpButton;

	[SerializeField]
	private OwlcatButton m_ScrollDownButton;

	private static float s_DeltaButton = 0.1f;

	private IDisposable m_UpSub;

	private IDisposable m_DownSub;

	private IDisposable m_CheckActiveSub;

	public void Start()
	{
		if (GetComponent<Scrollbar>() == null)
		{
			m_ScrollUpButton.gameObject.SetActive(value: false);
			m_ScrollDownButton.gameObject.SetActive(value: false);
		}
		SetButtonsSounds();
	}

	public void OnEnable()
	{
		Scrollbar scrollbar = GetComponent<Scrollbar>();
		m_UpSub = m_ScrollUpButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			ScrollToTop(scrollbar);
		});
		m_DownSub = m_ScrollDownButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			ScrollToBottom(scrollbar);
		});
		m_CheckActiveSub = (from _ in Observable.EveryUpdate()
			where scrollbar != null
			select _).Subscribe(delegate
		{
			CheckButtonsActive(scrollbar);
		});
	}

	private void Unsubsribe()
	{
		m_UpSub?.Dispose();
		m_UpSub = null;
		m_DownSub?.Dispose();
		m_DownSub = null;
		m_CheckActiveSub?.Dispose();
		m_CheckActiveSub = null;
	}

	public void OnDisable()
	{
		Unsubsribe();
	}

	public void OnDestroy()
	{
		Unsubsribe();
	}

	private static void ScrollToTop(Scrollbar scrollRect)
	{
		scrollRect.value += s_DeltaButton;
	}

	private static void ScrollToBottom(Scrollbar scrollRect)
	{
		scrollRect.value -= s_DeltaButton;
	}

	private void CheckButtonsActive(Scrollbar scrollbar)
	{
		if (m_ScrollUpButton != null)
		{
			m_ScrollUpButton.gameObject.SetActive(scrollbar.IsActive());
		}
		if (m_ScrollDownButton != null)
		{
			m_ScrollDownButton.gameObject.SetActive(scrollbar.IsActive());
		}
		if (scrollbar.IsActive())
		{
			if (m_ScrollUpButton != null)
			{
				m_ScrollUpButton.Interactable = scrollbar.value != 1f && scrollbar.size != 1f;
			}
			if (m_ScrollDownButton != null)
			{
				m_ScrollDownButton.Interactable = scrollbar.value != 0f && scrollbar.size != 1f;
			}
			scrollbar.handleRect.gameObject.SetActive(scrollbar.size < 0.9999999f);
		}
	}

	private void SetButtonsSounds()
	{
		if (m_ScrollUpButton != null)
		{
			m_ScrollUpButton.HoverSoundType = 0;
			m_ScrollUpButton.ClickSoundType = 0;
		}
		if (m_ScrollDownButton != null)
		{
			m_ScrollDownButton.HoverSoundType = 0;
			m_ScrollDownButton.ClickSoundType = 0;
		}
	}
}
