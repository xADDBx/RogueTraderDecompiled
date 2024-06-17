using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

public class DragPanelController : Graphic, IPointerDownHandler, IEventSystemHandler, IDragHandler
{
	private Vector2 m_OriginalLocalPointerPosition;

	private Vector3 m_OriginalPanelLocalPosition;

	public RectTransform TargetPanel;

	public RectTransform ParentArea;

	[UsedImplicitly]
	protected override void Awake()
	{
		base.Awake();
		if (!TargetPanel)
		{
			TargetPanel = base.transform.parent as RectTransform;
		}
		if (!ParentArea)
		{
			ParentArea = TargetPanel.parent as RectTransform;
		}
		Color color = this.color;
		color.a = 0f;
		this.color = color;
		raycastTarget = true;
	}

	public void OnPointerDown(PointerEventData data)
	{
		m_OriginalPanelLocalPosition = TargetPanel.localPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentArea, data.position, data.pressEventCamera, out m_OriginalLocalPointerPosition);
	}

	public virtual void OnDrag(PointerEventData data)
	{
		if (!(TargetPanel == null) && !(ParentArea == null))
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentArea, data.position, data.pressEventCamera, out var localPoint))
			{
				SetTargetLocalPosition(localPoint);
			}
			ClampToWindow();
		}
	}

	private void ClampToWindow()
	{
		Vector3 localPosition = TargetPanel.localPosition;
		Vector3 vector = ParentArea.rect.min - TargetPanel.rect.min;
		Vector3 vector2 = ParentArea.rect.max - TargetPanel.rect.max;
		localPosition.x = Mathf.Clamp(TargetPanel.localPosition.x, vector.x, vector2.x);
		localPosition.y = Mathf.Clamp(TargetPanel.localPosition.y, vector.y, vector2.y);
		TargetPanel.localPosition = localPosition;
	}

	protected virtual void SetTargetLocalPosition(Vector2 localPointerPosition)
	{
		Vector3 vector = localPointerPosition - m_OriginalLocalPointerPosition;
		TargetPanel.localPosition = m_OriginalPanelLocalPosition + vector;
	}
}
