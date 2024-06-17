using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Common;

public static class EventDataExtention
{
	public static bool IsAnyHover(this PointerEventData eventData)
	{
		return eventData.hovered.Count > 0;
	}

	public static bool HoverAt<T>(this PointerEventData eventData) where T : IEventSystemHandler
	{
		foreach (GameObject item in eventData.hovered)
		{
			if (ExecuteEvents.CanHandleEvent<T>(item))
			{
				return true;
			}
		}
		return false;
	}
}
