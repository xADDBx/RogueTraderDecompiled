using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Legacy.Ping;

public class PingHandler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
{
	public enum TypeEvent
	{
		Hover = 2,
		DragAndDrop = 4
	}

	public string Key;

	[EnumFlagsAsDropdown]
	public TypeEvent IncludeEvents;

	protected static PingHandler m_Current;

	protected PingManager m_CurrentManager;

	protected void Awake()
	{
		m_CurrentManager = PingManager.Instance;
	}

	protected void OnDisable()
	{
		OnPointerExit(null);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (TypeEvent.Hover == (IncludeEvents & TypeEvent.Hover) && m_Current == null)
		{
			m_CurrentManager.Ping(Key, active: true);
		}
		if (TypeEvent.DragAndDrop == (IncludeEvents & TypeEvent.DragAndDrop) && m_Current != null)
		{
			m_CurrentManager.Ping(Key, active: false);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (TypeEvent.DragAndDrop == (IncludeEvents & TypeEvent.DragAndDrop) && m_Current != null)
		{
			m_CurrentManager.Ping(Key, active: true);
		}
		if (m_Current == null && TypeEvent.Hover == (IncludeEvents & TypeEvent.Hover))
		{
			m_CurrentManager.Ping(Key, active: false);
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (TypeEvent.DragAndDrop == (IncludeEvents & TypeEvent.DragAndDrop))
		{
			m_Current = this;
			m_CurrentManager.Ping(Key, active: true);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		m_Current = null;
		m_CurrentManager.Ping(Key, active: false);
	}

	public void OnDrop(PointerEventData eventData)
	{
		m_Current = null;
		m_CurrentManager.Ping(Key, active: false);
	}
}
