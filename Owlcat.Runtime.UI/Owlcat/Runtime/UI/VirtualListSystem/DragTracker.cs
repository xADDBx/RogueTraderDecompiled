using UnityEngine;
using UnityEngine.EventSystems;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal class DragTracker : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler
{
	private bool m_IsDragged;

	public bool IsDragged => m_IsDragged;

	public void OnBeginDrag(PointerEventData eventData)
	{
		m_IsDragged = true;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		m_IsDragged = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
	}
}
