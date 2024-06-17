using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Models.Log;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Common;

public class ResizePanel : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	public enum ResizePivot
	{
		TopLeft,
		Top,
		TopRight,
		Left,
		Right,
		BottomLeft,
		Bottom,
		BottomRight
	}

	public Dictionary<ResizePivot, Texture2D> CursorDictionary;

	public Vector2 MaxSize = new Vector2(400f, 400f);

	public Vector2 MinSize = new Vector2(100f, 100f);

	private Vector2 m_OriginalLocalPointerPosition;

	private Vector2 m_OriginalSizeDelta;

	public ResizePivot Pivot;

	[Space(10f)]
	public IResizeElement Target;

	private bool IsInteractable = true;

	public void Initialize(IResizeElement target)
	{
		Target = target;
	}

	public void OnDrag(PointerEventData data)
	{
		if (IsInteractable && Target != null)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(Target.GetTransform(), data.position, data.pressEventCamera, out var localPoint);
			Vector2 vector = m_OriginalSizeDelta + GetOffset(localPoint);
			vector = new Vector2((int)Mathf.Clamp(vector.x, MinSize.x, MaxSize.x), (int)Mathf.Clamp(vector.y, MinSize.y, MaxSize.y));
			Target.SetSizeDelta(vector);
		}
	}

	public void OnPointerDown(PointerEventData data)
	{
		if (IsInteractable && Target != null)
		{
			m_OriginalSizeDelta = Target.GetSize();
			RectTransformUtility.ScreenPointToLocalPointInRectangle(Target.GetTransform(), data.position, data.pressEventCamera, out m_OriginalLocalPointerPosition);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (IsInteractable)
		{
			Game.Instance.CursorController.SetCursor(GetCursorType(), force: true);
			Game.Instance.CursorController.SetLock(@lock: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (IsInteractable)
		{
			Game.Instance.CursorController.SetLock(@lock: false);
			Game.Instance.CursorController.ClearCursor(force: true);
		}
	}

	private CursorType GetCursorType()
	{
		switch (Pivot)
		{
		case ResizePivot.Top:
		case ResizePivot.Bottom:
			return CursorType.Vertical;
		case ResizePivot.TopRight:
		case ResizePivot.BottomLeft:
			return CursorType.DiagonalRight;
		case ResizePivot.TopLeft:
		case ResizePivot.BottomRight:
			return CursorType.DiagonalLeft;
		case ResizePivot.Left:
		case ResizePivot.Right:
			return CursorType.Horizontal;
		default:
			return CursorType.Default;
		}
	}

	private Vector2 GetOffset(Vector2 localPointerPosition)
	{
		Vector3 vector = localPointerPosition - m_OriginalLocalPointerPosition;
		float x = 0f;
		float y = 0f;
		switch (Pivot)
		{
		case ResizePivot.Bottom:
			y = 0f - vector.y;
			break;
		case ResizePivot.BottomLeft:
			x = vector.x;
			y = vector.y;
			break;
		case ResizePivot.BottomRight:
			x = vector.x;
			y = 0f - vector.y;
			break;
		case ResizePivot.Left:
			x = 0f - vector.x;
			break;
		case ResizePivot.Right:
			x = vector.x;
			break;
		case ResizePivot.Top:
			y = vector.y;
			break;
		case ResizePivot.TopLeft:
			x = 0f - vector.x;
			y = vector.y;
			break;
		case ResizePivot.TopRight:
			x = 0f - vector.x;
			y = 0f - vector.y;
			break;
		}
		return new Vector2(x, y);
	}
}
