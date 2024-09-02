using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/ScrollRectEx")]
public class ScrollRectEx : ScrollRect
{
	private bool routeToParent;

	private void DoForParents<T>(Action<T> action) where T : IEventSystemHandler
	{
		Transform parent = base.transform.parent;
		while (parent != null)
		{
			Component[] components = parent.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (component is T)
				{
					action((T)(IEventSystemHandler)component);
				}
			}
			parent = parent.parent;
		}
	}

	public override void OnInitializePotentialDrag(PointerEventData eventData)
	{
		DoForParents(delegate(IInitializePotentialDragHandler parent)
		{
			parent.OnInitializePotentialDrag(eventData);
		});
		base.OnInitializePotentialDrag(eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (routeToParent)
		{
			DoForParents(delegate(IDragHandler parent)
			{
				parent.OnDrag(eventData);
			});
		}
		else
		{
			base.OnDrag(eventData);
		}
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (!base.horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
		{
			routeToParent = true;
		}
		else if (!base.vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
		{
			routeToParent = true;
		}
		else
		{
			routeToParent = false;
		}
		if (routeToParent)
		{
			DoForParents(delegate(IBeginDragHandler parent)
			{
				parent.OnBeginDrag(eventData);
			});
		}
		else
		{
			base.OnBeginDrag(eventData);
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		if (routeToParent)
		{
			DoForParents(delegate(IEndDragHandler parent)
			{
				parent.OnEndDrag(eventData);
			});
		}
		else
		{
			base.OnEndDrag(eventData);
		}
		routeToParent = false;
	}

	public override void OnScroll(PointerEventData eventData)
	{
		if (!base.horizontal && Math.Abs(eventData.scrollDelta.x) > Math.Abs(eventData.scrollDelta.y))
		{
			routeToParent = true;
		}
		else if (!base.vertical && Math.Abs(eventData.scrollDelta.x) < Math.Abs(eventData.scrollDelta.y))
		{
			routeToParent = true;
		}
		else
		{
			routeToParent = false;
		}
		if (routeToParent)
		{
			DoForParents(delegate(IScrollHandler parent)
			{
				parent.OnScroll(eventData);
			});
		}
		else
		{
			base.OnScroll(eventData);
		}
	}
}
