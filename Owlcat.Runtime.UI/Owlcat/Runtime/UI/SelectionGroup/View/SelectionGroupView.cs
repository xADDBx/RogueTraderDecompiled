using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.SelectionGroup.View;

public abstract class SelectionGroupView<TToggleGroupVM, TEntityVM, TEntityView> : ViewBase<TToggleGroupVM> where TToggleGroupVM : SelectionGroupVM<TEntityVM> where TEntityVM : SelectionGroupEntityVM where TEntityView : SelectionGroupEntityView<TEntityVM>
{
	[Header("Virtual List")]
	[SerializeField]
	[UsedImplicitly]
	protected VirtualListComponent VirtualList;

	[SerializeField]
	[UsedImplicitly]
	public TEntityView SlotPrefab;

	[SerializeField]
	[UsedImplicitly]
	public bool HasSorter;

	private List<TEntityVM> m_SortedCollection = new List<TEntityVM>();

	private bool m_IsInit;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			VirtualList.Initialize(new VirtualListElementTemplate<TEntityVM>(SlotPrefab));
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		Initialize();
		if (HasSorter)
		{
			AddDisposable(base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
			{
				ApplySorting();
			}));
			ApplySorting();
			AddDisposable(VirtualList.Subscribe(m_SortedCollection.ToReactiveCollection()));
		}
		else
		{
			AddDisposable(VirtualList.Subscribe(base.ViewModel.EntitiesCollection));
		}
		VirtualList.ScrollController.ForceScrollToTop();
	}

	public void ApplySorting()
	{
		m_SortedCollection = base.ViewModel.EntitiesCollection.ToList();
		m_SortedCollection.Sort(EntityComparer);
	}

	protected virtual int EntityComparer(TEntityVM x, TEntityVM y)
	{
		return 0;
	}

	public virtual bool TrySelectEntity(TEntityView entityView)
	{
		return base.ViewModel.TrySelectEntity(entityView.GetViewModel() as TEntityVM);
	}

	public virtual bool SelectFirstValidEntity()
	{
		return base.ViewModel.TrySelectFirstValidEntity();
	}

	public virtual bool SelectLastValidEntity()
	{
		return base.ViewModel.TrySelectLastValidEntity();
	}

	public bool TryScrollToLastSelectedEntity()
	{
		return TryScrollToEntity(base.ViewModel.LastChangedEntity.Value);
	}

	public ConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		return VirtualList.GetNavigationBehaviour();
	}

	private bool TryScrollToEntity(TEntityVM entityVM)
	{
		VirtualList.ScrollController.ForceScrollToElement(entityVM);
		return true;
	}

	public IConsoleEntity GetView(IViewModel viewModel)
	{
		return VirtualList.TryGetNavigationEntity(viewModel);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
