using UnityEngine;
using UnityEngine.EventSystems;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public class ScrollHandler : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	[HideInInspector]
	public bool IsVertical;

	private bool m_IsScrolling;

	private Vector2 m_ScrollDelta;

	public Vector2 ScrollDelta => m_ScrollDelta;

	public bool IsScrolling => m_IsScrolling;

	public void OnScroll(PointerEventData data)
	{
		m_ScrollDelta = data.scrollDelta;
		m_IsScrolling = data.IsScrolling();
	}

	public void Refresh()
	{
		m_IsScrolling = false;
	}
}
