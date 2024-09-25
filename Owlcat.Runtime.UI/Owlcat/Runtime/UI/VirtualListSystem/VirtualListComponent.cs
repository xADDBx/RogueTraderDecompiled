using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public abstract class VirtualListComponent : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_Viewport;

	[SerializeField]
	private RectTransform m_Content;

	[SerializeField]
	private Scrollbar m_Scrollbar;

	[SerializeField]
	private Button m_BeginButton;

	[SerializeField]
	private Button m_EndButton;

	[SerializeField]
	private VirtualListScrollSettings m_ScrollSettings;

	[SerializeField]
	private bool m_ClearItemsAnyway;

	private VirtualList m_VirtualList;

	private bool m_Binded;

	private ScrollHandler m_ScrollHandler;

	protected abstract IVirtualListLayoutSettings LayoutSettings { get; }

	public IScrollController ScrollController => m_VirtualList?.ScrollController;

	public List<VirtualListElement> Elements => m_VirtualList?.Elements;

	public List<VirtualListElement> ActiveElements => m_VirtualList?.ActiveElements;

	public List<VirtualListElement> VisibleElements => m_VirtualList?.VisibleElements;

	public ReactiveCommand AttachedFirstValidView => m_VirtualList?.AttachedFirstValidView;

	public void Initialize(params IVirtualListElementTemplate[] templates)
	{
		VirtualListViewsFabric fabric = new VirtualListViewsFabric(templates);
		m_VirtualList = new VirtualList(LayoutSettings, m_ScrollSettings, fabric, m_Viewport, m_Content, m_Scrollbar, m_ClearItemsAnyway);
	}

	public IDisposable Subscribe<TData>(IReadOnlyReactiveCollection<TData> datas) where TData : IVirtualListElementData
	{
		if (m_VirtualList == null)
		{
			throw new NullReferenceException("Call Initialize before Bind");
		}
		CompositeDisposable compositeDisposable = new CompositeDisposable();
		AddRange(datas);
		compositeDisposable.Add(datas.ObserveAdd().Subscribe(delegate(CollectionAddEvent<TData> evt)
		{
			Insert(evt.Index, evt.Value);
		}));
		compositeDisposable.Add(datas.ObserveRemove().Subscribe(delegate(CollectionRemoveEvent<TData> evt)
		{
			Remove(evt.Value);
		}));
		compositeDisposable.Add(datas.ObserveReplace().Subscribe(delegate(CollectionReplaceEvent<TData> evt)
		{
			Replace(evt.OldValue, evt.NewValue);
		}));
		compositeDisposable.Add(datas.ObserveMove().Subscribe(delegate(CollectionMoveEvent<TData> evt)
		{
			Move(evt.OldIndex, evt.NewIndex);
		}));
		compositeDisposable.Add(ObservableExtensions.Subscribe(datas.ObserveReset(), delegate
		{
			m_VirtualList.Clear();
		}));
		compositeDisposable.Add(Disposable.Create(m_VirtualList.Dispose));
		m_Binded = true;
		compositeDisposable.Add(Disposable.Create(delegate
		{
			m_Binded = false;
		}));
		if ((bool)m_BeginButton && (bool)m_EndButton)
		{
			compositeDisposable.Add(m_BeginButton.OnClickAsObservable().Subscribe(OnBeginClick));
			compositeDisposable.Add(m_EndButton.OnClickAsObservable().Subscribe(OnEndClick));
		}
		return compositeDisposable;
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		return m_VirtualList.GetNavigationBehaviour();
	}

	public GridConsoleNavigationBehaviour GetActiveNavigationBehaviour()
	{
		return m_VirtualList.GetActiveNavigationBehaviour();
	}

	public void ClearNavigation()
	{
		m_VirtualList.ClearNavigation();
	}

	private void AddRange<TData>(IEnumerable<TData> dataRange) where TData : IVirtualListElementData
	{
		m_VirtualList.AddRange(dataRange);
	}

	private void Insert<TData>(int index, TData data) where TData : IVirtualListElementData
	{
		m_VirtualList.Insert(index, data);
	}

	private void Remove(IVirtualListElementData data)
	{
		m_VirtualList.Remove(data);
	}

	private void Replace(IVirtualListElementData oldData, IVirtualListElementData newData)
	{
		m_VirtualList.Replace(oldData, newData);
	}

	private void Move(int oldIndex, int newIndex)
	{
		m_VirtualList.Move(oldIndex, newIndex);
	}

	private void LateUpdate()
	{
		if (m_Binded && m_VirtualList != null)
		{
			m_VirtualList.Tick();
		}
	}

	private void OnBeginClick()
	{
		if (m_ScrollHandler == null)
		{
			m_ScrollHandler = m_Viewport.GetComponent<ScrollHandler>();
		}
		if (!(m_ScrollHandler == null))
		{
			PointerEventData data = new PointerEventData(null)
			{
				scrollDelta = new Vector2(1f, 1f)
			};
			m_ScrollHandler.OnScroll(data);
		}
	}

	private void OnEndClick()
	{
		if (m_ScrollHandler == null)
		{
			m_ScrollHandler = m_Viewport.GetComponent<ScrollHandler>();
		}
		if (!(m_ScrollHandler == null))
		{
			PointerEventData data = new PointerEventData(null)
			{
				scrollDelta = new Vector2(-1f, -1f)
			};
			m_ScrollHandler.OnScroll(data);
		}
	}

	public void SetViewport(RectTransform viewport)
	{
		m_Viewport = viewport;
	}

	public void SetScrollbar(Scrollbar scrollbar)
	{
		m_Scrollbar = scrollbar;
	}

	public IConsoleEntity TryGetNavigationEntity(IViewModel viewModel)
	{
		return m_VirtualList.TryGetNavigationEntity(viewModel);
	}
}
