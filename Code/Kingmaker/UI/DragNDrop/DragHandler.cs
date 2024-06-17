using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.DragNDrop;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IDropHandler
{
	public void OnBeginDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnBeginDrag(eventData, base.gameObject);
		});
	}

	public void OnDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrag(eventData);
		});
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnEndDrag(eventData);
		});
	}

	public virtual void OnDrop(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrop(eventData, base.gameObject);
		});
	}
}
