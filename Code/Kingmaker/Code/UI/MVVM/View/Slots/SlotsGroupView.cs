using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public abstract class SlotsGroupView<TViewModel> : ViewBase<SlotsGroupVM<TViewModel>> where TViewModel : ItemSlotVM
{
	[SerializeField]
	private bool m_UseVirtualList;

	[ShowIf("m_UseVirtualList")]
	[SerializeField]
	private VirtualListGridVertical m_VirtualList;

	[SerializeField]
	private bool m_UseWidgetList;

	[ShowIf("m_UseWidgetList")]
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[ShowIf("m_UseWidgetList")]
	[SerializeField]
	private int m_ColumnCount;

	private ItemSlotView<TViewModel> m_SlotPrefab;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public WidgetListMVVM WidgetList => m_WidgetList;

	public VirtualListGridVertical VirtualList => m_VirtualList;

	public void Initialize(ItemSlotView<TViewModel> prefab)
	{
		m_SlotPrefab = prefab;
		if (m_UseVirtualList)
		{
			m_VirtualList.Initialize(new VirtualListElementTemplate<TViewModel>(prefab));
		}
	}

	protected override void BindViewImplementation()
	{
		if (m_UseVirtualList)
		{
			AddDisposable(m_VirtualList.Subscribe(base.ViewModel.VisibleCollection));
			m_NavigationBehaviour = m_VirtualList.GetNavigationBehaviour();
		}
		if (m_UseWidgetList)
		{
			AddDisposable(base.ViewModel.CollectionChangedCommand.Subscribe(delegate
			{
				DrawEntities();
			}));
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
			DrawEntities();
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void DrawEntities()
	{
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.VisibleCollection, m_SlotPrefab));
		if (!m_WidgetList.Entries.Empty())
		{
			int num = m_NavigationBehaviour.Entities.IndexOf(m_NavigationBehaviour.DeepestNestedFocus);
			m_NavigationBehaviour.SetEntitiesGrid(m_WidgetList.Entries.Select((IWidgetView e) => e as IConsoleNavigationEntity).ToList(), m_ColumnCount);
			if (num > -1)
			{
				m_NavigationBehaviour.FocusOnEntityManual(m_NavigationBehaviour.Entities.ElementAtOrDefault(num));
			}
		}
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		return m_NavigationBehaviour;
	}

	public void ForceScrollToTop()
	{
		if (m_UseVirtualList)
		{
			m_VirtualList.ScrollController.ForceScrollToTop();
		}
	}

	public void ForceScrollToBottom()
	{
		if (m_UseVirtualList)
		{
			m_VirtualList.ScrollController.ForceScrollToBottom();
		}
	}

	public void ForceScrollToElement(IVirtualListElementData data)
	{
		if (m_UseVirtualList)
		{
			m_VirtualList.ScrollController.ForceScrollToElement(data);
		}
	}
}
