using UnityEngine;

namespace Kingmaker.UI.Selection;

public class SelectFrameController : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_CanvasRect;

	[SerializeField]
	private RectTransform m_SelectorFrameRect;

	private Vector2 m_Anchor;

	public void Initialize()
	{
		ClearBox();
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void StartSelectBox(Vector3 anchor)
	{
		m_Anchor = ScreenToLocal(anchor);
		m_SelectorFrameRect.localPosition = new Vector3(m_Anchor.x, m_Anchor.y, m_SelectorFrameRect.localPosition.z);
	}

	public void DragSelectBox(Vector3 outer)
	{
		Vector2 vector = ScreenToLocal(outer);
		Vector2 sizeDelta = default(Vector2);
		Vector3 localPosition = new Vector3(m_Anchor.x, m_Anchor.y, m_SelectorFrameRect.localPosition.z);
		if (vector.x - m_Anchor.x < 0f)
		{
			sizeDelta.x = m_Anchor.x - vector.x;
			localPosition.x = vector.x;
		}
		else
		{
			sizeDelta.x = vector.x - m_Anchor.x;
		}
		if (vector.y - m_Anchor.y < 0f)
		{
			sizeDelta.y = m_Anchor.y - vector.y;
			localPosition.y = vector.y;
		}
		else
		{
			sizeDelta.y = vector.y - m_Anchor.y;
		}
		m_SelectorFrameRect.sizeDelta = sizeDelta;
		m_SelectorFrameRect.localPosition = localPosition;
	}

	public Vector2 GetSize()
	{
		return m_SelectorFrameRect.sizeDelta;
	}

	private Vector2 ScreenToLocal(Vector3 point)
	{
		Camera instance = UICamera.Instance;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasRect, point, instance, out var localPoint);
		return localPoint;
	}

	public void ClearAll()
	{
		ClearBox();
	}

	public void ClearBox()
	{
		base.gameObject.SetActive(value: false);
		m_SelectorFrameRect.sizeDelta = new Vector2(0f, 0f);
	}
}
