using DG.Tweening;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.DragNDrop;

public class DragNDropHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IDropHandler, IDraggableElement
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_CountLabel;

	[SerializeField]
	private Vector2 m_DragImageSize = new Vector2(50f, 50f);

	public bool CanDrag = true;

	public ReactiveCommand OnDragStart = new ReactiveCommand();

	public ReactiveCommand<GameObject> OnDragEnd = new ReactiveCommand<GameObject>();

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

	public void StartDrag()
	{
		m_Icon.DOFade(0.5f, 0.1f).SetUpdate(isIndependentUpdate: true);
		OnDragStart.Execute();
	}

	public void EndDrag(PointerEventData eventData)
	{
		m_Icon.DOFade(1f, 0.1f).SetUpdate(isIndependentUpdate: true);
		OnDragEnd.Execute(DragNDropManager.DropTarget);
	}

	public bool SetDragSlot(DragNDropManager slot)
	{
		if (!CanDrag)
		{
			return false;
		}
		slot.Icon.sprite = m_Icon.Or(null)?.sprite;
		slot.Count.text = m_CountLabel.Or(null)?.text;
		slot.OverideSize = m_DragImageSize;
		return true;
	}

	public void CancelDrag()
	{
	}
}
