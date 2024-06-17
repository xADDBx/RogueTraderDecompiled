using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility.UnityExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.DragNDrop;

public class DragNDropManager : MonoBehaviour, IDragDropEventUIHandler, ISubscriber
{
	public static GameObject DropTarget;

	private static IDraggableElement s_DraggableElement;

	private RectTransform m_RectTransform;

	public Image Icon;

	public TextMeshProUGUI Count;

	public static DragNDropManager Instance { get; protected set; }

	public static bool IsDraggingSomething => ItemBeingDragged != null;

	public static GameObject ItemBeingDragged { get; private set; }

	public Vector2 OverideSize { get; set; } = Vector2.zero;


	public void OnBeginDrag(PointerEventData eventData, GameObject gObject)
	{
		if (gObject == null)
		{
			return;
		}
		s_DraggableElement = gObject.GetComponentInParent<IDraggableElement>();
		if (s_DraggableElement != null)
		{
			if (!s_DraggableElement.SetDragSlot(this))
			{
				s_DraggableElement = null;
				return;
			}
			ItemBeingDragged = gObject;
			s_DraggableElement.StartDrag();
			Icon.gameObject.SetActive(value: true);
			UpdateIconSize();
			EscHotkeyManager.Instance.Subscribe(CancelDrag);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		EscHotkeyManager.Instance.Unsubscribe(CancelDrag);
		Icon.gameObject.SetActive(value: false);
		if (s_DraggableElement != null)
		{
			s_DraggableElement.EndDrag(eventData);
			s_DraggableElement = null;
			OverideSize = Vector2.zero;
		}
		DropTarget = null;
		ItemBeingDragged = null;
		OverideSize = Vector2.zero;
	}

	public void OnDrag(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(UIDollRooms.Instance.RectTransform ? UIDollRooms.Instance.RectTransform : m_RectTransform, eventData.position, UICamera.Instance, out var localPoint);
		Icon.transform.localPosition = localPoint;
	}

	public void Reset()
	{
		Icon.gameObject.SetActive(value: false);
		Cursor.visible = true;
		ItemBeingDragged = null;
		s_DraggableElement = null;
		DropTarget = null;
		OverideSize = Vector2.zero;
	}

	public void OnDrop(PointerEventData eventData, GameObject gObjectTarget)
	{
		DropTarget = gObjectTarget;
	}

	public void Initialize()
	{
		Instance = this;
		m_RectTransform = base.transform as RectTransform;
		Icon.gameObject.SetActive(value: false);
		EventBus.Subscribe(this);
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	public void CancelDrag()
	{
		if (IsDraggingSomething)
		{
			EscHotkeyManager.Instance.Unsubscribe(CancelDrag);
			s_DraggableElement?.CancelDrag();
			Reset();
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!ApplicationFocusEvents.DragDisabled)
		{
			CancelDrag();
		}
	}

	private void UpdateIconSize()
	{
		if (Icon.sprite != null)
		{
			if (OverideSize == Vector2.zero)
			{
				float num = 64f;
				Icon.GetComponent<RectTransform>().sizeDelta = new Vector2(num * ((float)Icon.sprite.texture.width / (float)Icon.sprite.texture.height), num);
			}
			else
			{
				Icon.GetComponent<RectTransform>().sizeDelta = OverideSize - Vector2.one;
			}
		}
	}
}
