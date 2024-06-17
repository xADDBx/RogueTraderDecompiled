using System;
using System.Collections.Generic;
using System.Text;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal class VirtualListViewsFabric
{
	private Dictionary<int, int> m_Indices;

	private IVirtualListElementView[] m_Prefabs;

	private Queue<IVirtualListElementView>[] m_Pools;

	private static List<VirtualListViewsFabric> s_Instances = new List<VirtualListViewsFabric>();

	private string m_TypesForErrorMessage;

	internal event Action AttachedValidView;

	internal VirtualListViewsFabric(IVirtualListElementTemplate[] templates)
	{
		m_Indices = new Dictionary<int, int>();
		m_Prefabs = new IVirtualListElementView[templates.Length];
		m_Pools = new Queue<IVirtualListElementView>[templates.Length];
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		for (int i = 0; i < templates.Length; i++)
		{
			int elementHashCode = GetElementHashCode(templates[i].ElementType, templates[i].Id);
			m_Indices.Add(elementHashCode, i);
			m_Prefabs[i] = templates[i].View;
			m_Pools[i] = new Queue<IVirtualListElementView>();
			stringBuilder.Append($"({templates[i].ElementType}; {templates[i].Id})");
			if (i < templates.Length - 1)
			{
				stringBuilder.Append("; ");
			}
		}
		stringBuilder.Append("]");
		m_TypesForErrorMessage = stringBuilder.ToString();
	}

	internal void AttachView(VirtualListElement element, RectTransform content)
	{
		int index = GetIndex(element);
		Queue<IVirtualListElementView> queue = m_Pools[index];
		IVirtualListElementView virtualListElementView = ((queue.Count > 0) ? queue.Dequeue() : m_Prefabs[index].Instantiate());
		if (queue.Count == 0)
		{
			virtualListElementView.RectTransform.SetParent(content, worldPositionStays: false);
		}
		virtualListElementView.RectTransform.gameObject.SetActive(value: true);
		element.AttachView(virtualListElementView);
		if (element.IsValid())
		{
			this.AttachedValidView?.Invoke();
		}
	}

	internal void DetachView(VirtualListElement element, bool clearItemsAnyway = false)
	{
		IVirtualListElementView virtualListElementView = element.DetachView();
		if (WidgetFactoryStash.Exists && !clearItemsAnyway)
		{
			int index = GetIndex(element);
			Queue<IVirtualListElementView> obj = m_Pools[index];
			virtualListElementView.RectTransform.gameObject.SetActive(value: false);
			obj.Enqueue(virtualListElementView);
		}
		else
		{
			UnityEngine.Object.Destroy(virtualListElementView.RectTransform.gameObject);
		}
	}

	internal static void DestroyAll()
	{
		foreach (VirtualListViewsFabric s_Instance in s_Instances)
		{
			Queue<IVirtualListElementView>[] pools = s_Instance.m_Pools;
			foreach (Queue<IVirtualListElementView> queue in pools)
			{
				foreach (IVirtualListElementView item in queue)
				{
					if ((bool)item.RectTransform)
					{
						UnityEngine.Object.Destroy(item.RectTransform.gameObject);
					}
				}
				queue.Clear();
			}
		}
	}

	internal int GetIndex(VirtualListElement element)
	{
		IVirtualListElementData data = element.Data;
		Type type = data.GetType();
		int num = 0;
		if (data is IVirtualListElementIdentifier virtualListElementIdentifier)
		{
			num = virtualListElementIdentifier.VirtualListTypeId;
		}
		int elementHashCode = GetElementHashCode(type, num);
		if (!m_Indices.ContainsKey(elementHashCode))
		{
			throw new ArgumentException($"[VirtualList] Can't find element with Type: '{type}' and Id: '{num}'." + "Has only this types and ids: " + m_TypesForErrorMessage);
		}
		return m_Indices[elementHashCode];
	}

	private int GetElementHashCode(Type type, int id)
	{
		return (17 * 31 + type.GetHashCode()) * 31 + id.GetHashCode();
	}
}
