using System;
using System.Collections.Generic;
using Owlcat.Runtime.UniRx;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Owlcat.Runtime.UI.Utility.CanvasSorting;

[RequireComponent(typeof(Canvas))]
public class CanvasSortingComponent : MonoBehaviour
{
	private Canvas m_Canvas;

	[SerializeField]
	private List<CanvasSortingComponent> m_NestedComponents;

	private CompositeDisposable m_Disposable;

	private int SortingId { get; set; }

	private int InitialSortingOrder { get; set; }

	private int SortingOrder { get; set; }

	private bool IsPushed { get; set; }

	private void Initialize()
	{
		if (!(m_Canvas != null))
		{
			m_Canvas = GetComponent<Canvas>();
			m_Canvas.overrideSorting = true;
			InitialSortingOrder = m_Canvas.sortingOrder;
		}
	}

	private void OnEnable()
	{
		Initialize();
		if (!IsPushed)
		{
			DoEnable();
		}
	}

	private void DoEnable()
	{
		UpdateOrder();
		if (m_Disposable == null)
		{
			m_Disposable = new CompositeDisposable();
			m_Disposable.Add(CanvasSortingManager.UpdateCommand.Subscribe(UpdateOrder));
			m_Disposable.Add(base.transform.OnTransformParentChangedAsObservable().Subscribe(UpdateOrder));
		}
	}

	private void UpdateOrder()
	{
		Canvas canvas = TryGetCanvas(base.transform.parent);
		if (!(canvas == null))
		{
			SortingOrder = canvas.sortingOrder;
			SetSortingOrderToCanvas(InitialSortingOrder + SortingOrder);
		}
	}

	private void OnDisable()
	{
		if (IsPushed)
		{
			PopView();
		}
		else
		{
			DoDisable();
		}
	}

	private void DoDisable()
	{
		SetSortingOrderToCanvas(InitialSortingOrder);
		SortingOrder = 0;
		SortingId = 0;
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	public IDisposable PushView()
	{
		Initialize();
		int newId = CanvasSortingManager.PushNewId();
		DoPushView(newId);
		m_NestedComponents?.ForEach(delegate(CanvasSortingComponent c)
		{
			c.DoPushView(newId);
		});
		CanvasSortingManager.UpdateCommand.Execute();
		return Disposable.Create(PopView);
	}

	private void DoPushView(int sortingId)
	{
		IsPushed = true;
		SortingId = sortingId;
		SortingOrder = CanvasSortingManager.GetSortingOrder(SortingId);
		SetSortingOrderToCanvas(InitialSortingOrder + SortingOrder);
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	public void PopView()
	{
		if (IsPushed)
		{
			Initialize();
			DoPopView();
			m_NestedComponents?.ForEach(delegate(CanvasSortingComponent c)
			{
				c.DoPopView();
			});
		}
	}

	private void DoPopView()
	{
		IsPushed = false;
		CanvasSortingManager.PopId(SortingId);
		DoDisable();
	}

	private void SetSortingOrderToCanvas(int order)
	{
		m_Canvas.sortingOrder = order;
	}

	private static Canvas TryGetCanvas(Transform obj)
	{
		Canvas component;
		while (true)
		{
			if (!obj)
			{
				return null;
			}
			component = obj.GetComponent<Canvas>();
			if ((bool)component)
			{
				break;
			}
			obj = obj.parent;
		}
		return component;
	}
}
