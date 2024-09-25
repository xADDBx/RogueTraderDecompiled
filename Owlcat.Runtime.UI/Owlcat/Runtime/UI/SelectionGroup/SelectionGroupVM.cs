using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Owlcat.Runtime.UI.SelectionGroup;

public abstract class SelectionGroupVM<TViewModel> : BaseDisposable, IViewModel, IBaseDisposable, IDisposable where TViewModel : SelectionGroupEntityVM
{
	public ReactiveCollection<TViewModel> EntitiesCollection;

	public ReactiveProperty<TViewModel> LastChangedEntity = new ReactiveProperty<TViewModel>();

	public SelectionGroupVM(List<TViewModel> visibleCollection)
	{
		EntitiesCollection = new ReactiveCollection<TViewModel>(visibleCollection);
		SubscribeItems();
	}

	public SelectionGroupVM(ReactiveCollection<TViewModel> entitiesCollection)
	{
		EntitiesCollection = entitiesCollection;
		SubscribeItems();
	}

	private void SubscribeItems()
	{
		foreach (TViewModel item in EntitiesCollection)
		{
			SubscribeNewItem(item);
		}
		AddDisposable(EntitiesCollection.ObserveAdd().Subscribe(delegate(CollectionAddEvent<TViewModel> newItem)
		{
			SubscribeNewItem(newItem.Value);
		}));
		AddDisposable(EntitiesCollection.ObserveRemove().Subscribe(delegate(CollectionRemoveEvent<TViewModel> newItem)
		{
			DisposeRemovedItem(newItem.Value);
		}));
	}

	public void InsertEntityAtIndex(int index, TViewModel viewModel)
	{
		EntitiesCollection.Insert(index, viewModel);
	}

	public void RemoveEntity(TViewModel viewModel)
	{
		DisposeRemovedItem(viewModel);
		EntitiesCollection.Remove(viewModel);
	}

	public void ClearFromIndex(int index)
	{
		while (EntitiesCollection.Count > index)
		{
			EntitiesCollection.RemoveAt(EntitiesCollection.Count - 1);
		}
	}

	protected virtual void SubscribeNewItem(TViewModel viewModel)
	{
		AddDisposable(viewModel.IsSelected.Where((bool value) => value).Subscribe(delegate
		{
			TrySelectEntity(viewModel);
		}));
		AddDisposable(viewModel.IsSelected.Where((bool value) => !value).Subscribe(delegate
		{
			TryUnselectEntity(viewModel);
		}));
	}

	private void DisposeRemovedItem(TViewModel viewModel)
	{
		viewModel.Dispose();
	}

	public bool TrySelectEntity(TViewModel viewModel)
	{
		if (viewModel == null)
		{
			return false;
		}
		if (!viewModel.IsAvailable.Value && !viewModel.IsSelected.Value)
		{
			return false;
		}
		if (TryDoSelect(viewModel))
		{
			LastChangedEntity.Value = viewModel;
			return true;
		}
		return false;
	}

	protected abstract bool TryDoSelect(TViewModel viewModel);

	private bool TryUnselectEntity(TViewModel viewModel)
	{
		if (viewModel == null)
		{
			return false;
		}
		if (!viewModel.IsAvailable.Value && !viewModel.IsSelected.Value)
		{
			return false;
		}
		if (TryDoUnselectSelect(viewModel))
		{
			LastChangedEntity.Value = viewModel;
			return true;
		}
		return false;
	}

	protected abstract bool TryDoUnselectSelect(TViewModel viewModel);

	protected override void DisposeImplementation()
	{
		LastChangedEntity.Value = null;
	}

	public bool TrySelectFirstValidEntity()
	{
		TViewModel val = EntitiesCollection.FirstOrDefault((TViewModel ent) => ent.IsAvailable.Value);
		if (val == null)
		{
			return false;
		}
		if (val.IsSelected.Value)
		{
			return true;
		}
		return TrySelectEntity(val);
	}

	public bool TrySelectLastValidEntity()
	{
		TViewModel val = EntitiesCollection.LastOrDefault((TViewModel ent) => ent.IsAvailable.Value);
		if (val == null)
		{
			return false;
		}
		if (val.IsSelected.Value)
		{
			return true;
		}
		return TrySelectEntity(val);
	}

	public abstract void SetupSelectedState();
}
