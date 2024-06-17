using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Other.NestedSelectionGroup;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Other.NestedSelectionGroup;

public abstract class NestedSelectionGroupPCView<TToggleGroupVM, TEntityVM, TEntityView> : ViewBase<TToggleGroupVM> where TToggleGroupVM : NestedSelectionGroupVM<TEntityVM> where TEntityVM : NestedSelectionGroupEntityVM where TEntityView : NestedSelectionGroupEntityPCView<TEntityVM>
{
	[Header("Virtual List")]
	[SerializeField]
	[UsedImplicitly]
	protected VirtualListComponent VirtualList;

	[SerializeField]
	[UsedImplicitly]
	public List<TEntityView> SlotPrefabs;

	protected ReactiveCollection<TEntityVM> VisibleCollection = new ReactiveCollection<TEntityVM>();

	private bool m_IsInit;

	public abstract bool HasSorter { get; }

	public abstract bool HasFilter { get; }

	private void Initialize()
	{
		if (!m_IsInit)
		{
			IVirtualListElementTemplate[] array = SlotPrefabs.Select((TEntityView prefab) => new VirtualListElementTemplate<TEntityVM>(prefab, SlotPrefabs.IndexOf(prefab))).ToArray();
			IVirtualListElementTemplate[] templates = array;
			VirtualList.Initialize(templates);
			m_IsInit = true;
		}
	}

	protected void ClearListView()
	{
		VisibleCollection.Clear();
	}

	protected override void BindViewImplementation()
	{
		Initialize();
		ClearListView();
		AddDisposable(base.ViewModel.NestedEntityCollections.ObserveCountChanged().Subscribe(delegate
		{
			OnCollectionChanged();
		}));
		OnCollectionChanged();
		AddDisposable(VirtualList.Subscribe(VisibleCollection));
		TryScrollToSelectedElement();
	}

	protected void TryScrollToSelectedElement()
	{
		NestedSelectionGroupEntityVM nestedSelectionGroupEntityVM = base.ViewModel.NestedEntityCollections.Last().Value?.FirstOrDefault((NestedSelectionGroupEntityVM el) => el.IsSelected.Value);
		if (nestedSelectionGroupEntityVM != null && VisibleCollection.Contains(nestedSelectionGroupEntityVM))
		{
			VirtualList.ScrollController.ForceScrollToElement(nestedSelectionGroupEntityVM);
		}
		else
		{
			VirtualList.ScrollController.ForceScrollToTop();
		}
	}

	protected void OnCollectionChanged()
	{
		List<TEntityVM> list = null;
		list = new List<TEntityVM>();
		foreach (KeyValuePair<INestedListSource, List<NestedSelectionGroupEntityVM>> nestedEntityCollection in base.ViewModel.NestedEntityCollections)
		{
			int num = list.IndexOf(nestedEntityCollection.Key as TEntityVM);
			List<TEntityVM> list2 = nestedEntityCollection.Value.Select((NestedSelectionGroupEntityVM ent) => (TEntityVM)ent).ToList();
			if (HasFilter)
			{
				list2 = list2.Where((TEntityVM entity) => IsVisible(entity)).ToList();
			}
			if (HasSorter)
			{
				list2.Sort(EntityComparer);
			}
			list.InsertRange(num + 1, list2);
		}
		List<TEntityVM> list3 = VisibleCollection.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			TEntityVM item = list[i];
			int num2 = VisibleCollection.IndexOf(item);
			if (num2 < 0)
			{
				VisibleCollection.Insert(i, item);
				continue;
			}
			list3.Remove(item);
			VisibleCollection.Move(num2, i);
		}
		foreach (TEntityVM item2 in list3)
		{
			VisibleCollection.Remove(item2);
		}
	}

	protected abstract bool IsVisible(TEntityVM entity);

	protected abstract int EntityComparer(TEntityVM a, TEntityVM b);

	public bool TryScrollToLastSelectedEntity()
	{
		return TryScrollToEntity(base.ViewModel.LastChangedEntity.Value);
	}

	private bool TryScrollToEntity(TEntityVM entityVM)
	{
		VirtualList.ScrollController.ForceScrollToElement(entityVM);
		return true;
	}

	protected override void DestroyViewImplementation()
	{
	}
}
