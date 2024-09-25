using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Owlcat.Runtime.UI.Utility;

public static class WidgetFactory
{
	private class WidgetNode
	{
		public MonoBehaviour Value;

		public bool IsAvailable;
	}

	private static readonly Dictionary<Type, WidgetsStack<MonoBehaviour>> s_Widgets = new Dictionary<Type, WidgetsStack<MonoBehaviour>>();

	private static readonly Dictionary<Type, WidgetsQueue<MonoBehaviour>> s_DisposedWidgets = new Dictionary<Type, WidgetsQueue<MonoBehaviour>>();

	private static readonly Dictionary<MonoBehaviour, List<WidgetNode>> s_StrictMatchingWidgets = new Dictionary<MonoBehaviour, List<WidgetNode>>();

	private static readonly List<Type> s_NoReparentWidgets = new List<Type>();

	private static WidgetFactoryStash Stash => WidgetFactoryStash.Instance;

	public static TWidget GetWidget<TWidget>(TWidget widget, bool activate = true, bool strictMatching = false) where TWidget : MonoBehaviour
	{
		try
		{
			TWidget val = TryGetWidget(widget, strictMatching);
			if (val == null)
			{
				val = UnityEngine.Object.Instantiate(widget);
				if (val is IWidget widget2)
				{
					widget2.OnWidgetInstantiated();
				}
				if (strictMatching)
				{
					PushToStrict(widget, val);
				}
			}
			if (val is IWidget widget3)
			{
				widget3.OnWidgetTaken();
			}
			val.gameObject.SetActive(activate);
			return val;
		}
		finally
		{
		}
	}

	public static void DisposeWidget<TWidget>(TWidget widget) where TWidget : MonoBehaviour
	{
		try
		{
			if (widget == null || (s_Widgets.TryGetValue(widget.GetType(), out var value) && value != null && value.Contains(widget) && widget.transform.parent == Stash.transform))
			{
				return;
			}
			if (widget is IWidget widget2)
			{
				widget2.OnWidgetReturned();
			}
			if (!WidgetFactoryStash.Exists)
			{
				UnityEngine.Object.Destroy(widget);
				return;
			}
			if (!s_DisposedWidgets.TryGetValue(widget.GetType(), out var value2) || value2 == null)
			{
				value2 = new WidgetsQueue<MonoBehaviour>();
				s_DisposedWidgets[widget.GetType()] = value2;
			}
			else if (value2.Contains(widget))
			{
				return;
			}
			value2.Enqueue(widget);
			ReleaseStrict(widget);
		}
		finally
		{
		}
	}

	public static void SetNoReparentForType(Type type)
	{
		if (!s_NoReparentWidgets.Contains(type))
		{
			s_NoReparentWidgets.Add(type);
		}
	}

	public static void InstantiateWidget<TWidget>(TWidget widget, int count, Transform parent = null) where TWidget : MonoBehaviour
	{
		try
		{
			WidgetsStack<MonoBehaviour> value;
			int num = ((s_Widgets.TryGetValue(widget.GetType(), out value) && value != null) ? value.Count : 0);
			if (parent != null)
			{
				SetNoReparentForType(widget.GetType());
			}
			for (int i = num; i < count; i++)
			{
				TWidget widget2 = ((!(parent != null)) ? UnityEngine.Object.Instantiate(widget) : UnityEngine.Object.Instantiate(widget, parent, worldPositionStays: false));
				DisposeWidget(widget2);
			}
		}
		finally
		{
		}
	}

	public static void DeactivateDisposedWidgets()
	{
		try
		{
			if (s_DisposedWidgets.Count <= 0)
			{
				return;
			}
			while (s_DisposedWidgets.Count > 0)
			{
				foreach (Type item in s_DisposedWidgets.Keys.ToList())
				{
					if (!s_Widgets.TryGetValue(item, out var value) || value == null)
					{
						value = new WidgetsStack<MonoBehaviour>();
						s_Widgets[item] = value;
					}
					WidgetsQueue<MonoBehaviour> widgetsQueue = s_DisposedWidgets[item];
					s_DisposedWidgets.Remove(item);
					while (widgetsQueue.Count > 0)
					{
						MonoBehaviour monoBehaviour = widgetsQueue.Dequeue();
						if (!(monoBehaviour == null))
						{
							if (!s_NoReparentWidgets.Contains(monoBehaviour.GetType()))
							{
								monoBehaviour.gameObject.SetActive(value: false);
								monoBehaviour.transform.SetParent(Stash.transform, worldPositionStays: false);
							}
							value.Push(monoBehaviour);
						}
					}
				}
			}
			s_DisposedWidgets.Clear();
		}
		finally
		{
		}
	}

	public static void DestroyAll(bool fromOnDestroy = false)
	{
		if (!fromOnDestroy)
		{
			DeactivateDisposedWidgets();
		}
		s_DisposedWidgets.Clear();
		s_StrictMatchingWidgets.Clear();
		s_NoReparentWidgets.Clear();
		try
		{
			foreach (WidgetsStack<MonoBehaviour> value in s_Widgets.Values)
			{
				foreach (MonoBehaviour item in value)
				{
					if ((bool)item)
					{
						UnityEngine.Object.Destroy(item.gameObject);
					}
				}
				value.Clear();
			}
			s_Widgets.Clear();
		}
		finally
		{
		}
	}

	private static TWidget TryGetWidget<TWidget>(TWidget widget, bool strictMatching) where TWidget : MonoBehaviour
	{
		TWidget val = null;
		if (s_DisposedWidgets.TryGetValue(widget.GetType(), out var value) && value != null && value.Count > 0)
		{
			if (strictMatching)
			{
				val = TryGetFromStrict(widget);
				value.Remove(val);
			}
			else
			{
				val = (TWidget)value.Dequeue();
			}
			if (val != null)
			{
				val.transform.SetAsLastSibling();
			}
		}
		if (val == null && s_Widgets.TryGetValue(widget.GetType(), out var value2) && value2 != null && value2.Count > 0)
		{
			if (strictMatching)
			{
				val = TryGetFromStrict(widget);
				value2.Remove(val);
			}
			else
			{
				val = (TWidget)value2.Pop();
			}
		}
		return val;
	}

	private static void PushToStrict<TWidget>(TWidget parent, TWidget instance) where TWidget : MonoBehaviour
	{
		if (!s_StrictMatchingWidgets.TryGetValue(parent, out var value) || value == null)
		{
			s_StrictMatchingWidgets[parent] = new List<WidgetNode>();
		}
		s_StrictMatchingWidgets[parent].Add(new WidgetNode
		{
			Value = instance,
			IsAvailable = false
		});
	}

	private static TWidget TryGetFromStrict<TWidget>(TWidget parent) where TWidget : MonoBehaviour
	{
		if (s_StrictMatchingWidgets.TryGetValue(parent, out var value) && value != null)
		{
			WidgetNode widgetNode = value.FirstOrDefault((WidgetNode n) => n.IsAvailable);
			if (widgetNode != null)
			{
				widgetNode.IsAvailable = false;
				return (TWidget)widgetNode.Value;
			}
		}
		return null;
	}

	private static void ReleaseStrict<TWidget>(TWidget instance) where TWidget : MonoBehaviour
	{
		foreach (List<WidgetNode> value in s_StrictMatchingWidgets.Values)
		{
			WidgetNode widgetNode = value.FirstOrDefault((WidgetNode n) => n.Value == instance);
			if (widgetNode != null)
			{
				widgetNode.IsAvailable = true;
				break;
			}
		}
	}
}
