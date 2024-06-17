using Kingmaker.Settings;
using Kingmaker.UI.Sound;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Common;

public class DoubleClickableElement : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	private int m_ClickCount;

	private int m_LastClickCount;

	private float m_ClearTime;

	private bool m_ClickDetected;

	public ReactiveCommand OnDoubleClickCommand = new ReactiveCommand();

	private static float ClickDelay => SettingsRoot.Controls.MouseClickDelay;

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			m_ClickCount++;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
		{
			m_ClickDetected = true;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			OnRightClick();
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		Reset();
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
	}

	protected virtual void OnClick()
	{
		UISounds.Instance.Sounds.Buttons.ButtonClick.Play();
	}

	protected virtual void OnDoubleClick()
	{
		OnDoubleClickCommand.Execute();
	}

	protected virtual void OnRightClick()
	{
		UISounds.Instance.Sounds.Buttons.ButtonClick.Play();
	}

	private void OnClicks()
	{
		switch (m_ClickCount)
		{
		case 1:
			OnClick();
			break;
		default:
			OnDoubleClick();
			break;
		case 0:
			break;
		}
	}

	private void Update()
	{
		if (!base.gameObject.activeSelf || !m_ClickDetected || m_ClickCount <= 0)
		{
			return;
		}
		if (m_LastClickCount >= m_ClickCount)
		{
			m_ClearTime += Time.fixedDeltaTime;
			if (!(m_ClearTime < ClickDelay))
			{
				if (m_ClickCount < 2)
				{
					OnClicks();
				}
				Reset();
			}
		}
		else
		{
			m_LastClickCount = m_ClickCount;
			if (m_ClickCount > 1)
			{
				OnClicks();
			}
			m_ClearTime = 0f;
		}
	}

	private void Reset()
	{
		m_LastClickCount = 0;
		m_ClickCount = 0;
		m_ClearTime = 0f;
		m_ClickDetected = false;
	}
}
