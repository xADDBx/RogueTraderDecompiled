using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Common;

public class DraggbleWindow : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	[Header("Move controller")]
	private bool m_MoveMode;

	private Vector2 m_MouseStartPos;

	private Vector2 m_ContainerStartPos;

	private Vector2 m_LastMausePos;

	[SerializeField]
	[UsedImplicitly]
	private Vector2 m_TakeDrag;

	[SerializeField]
	[UsedImplicitly]
	private RectTransform m_OwnRectTransform;

	[SerializeField]
	[UsedImplicitly]
	private RectTransform m_ParentRectTransform;

	public void SetParentRectTransform(RectTransform rt)
	{
		m_ParentRectTransform = rt;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			m_MoveMode = true;
			m_MouseStartPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			m_OwnRectTransform.anchoredPosition += m_TakeDrag;
			m_OwnRectTransform.DOAnchorPos(m_OwnRectTransform.anchoredPosition + m_TakeDrag, 0.1f).SetUpdate(isIndependentUpdate: true);
			m_ContainerStartPos = m_OwnRectTransform.anchoredPosition;
			PFLog.UI.Log("m_ContainerStartPos: {0}, {1}", m_ContainerStartPos.x, m_ContainerStartPos.y);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		m_OwnRectTransform.DOAnchorPos(m_OwnRectTransform.anchoredPosition - m_TakeDrag, 0.1f).SetUpdate(isIndependentUpdate: true);
		m_MoveMode = false;
		m_MouseStartPos = default(Vector2);
	}

	public void LateUpdate()
	{
		if (m_MoveMode)
		{
			Vector2 vector = new Vector2(Input.mousePosition.x - m_MouseStartPos.x, Input.mousePosition.y - m_MouseStartPos.y);
			if (!(m_LastMausePos == vector))
			{
				Vector2 nPos = m_ContainerStartPos + vector - m_TakeDrag;
				nPos = UIUtility.LimitPositionRectInRect(nPos, m_ParentRectTransform, m_OwnRectTransform);
				m_OwnRectTransform.anchoredPosition = nPos + m_TakeDrag;
				m_LastMausePos = vector;
			}
		}
	}
}
