using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Other.NestedSelectionGroup;

public abstract class NestedSelectionGroupVM<TViewModel> : BaseDisposable, IViewModel, IBaseDisposable, IDisposable where TViewModel : NestedSelectionGroupEntityVM
{
	public ReactiveProperty<TViewModel> LastChangedEntity = new ReactiveProperty<TViewModel>();

	public ReactiveDictionary<INestedListSource, List<NestedSelectionGroupEntityVM>> NestedEntityCollections = new ReactiveDictionary<INestedListSource, List<NestedSelectionGroupEntityVM>>();

	public NestedSelectionGroupVM(INestedListSource nestedListSource)
	{
		TryAddNestedList(nestedListSource);
	}

	protected virtual void SubscribeNewItem(TViewModel entityViewModel)
	{
		AddDisposable(entityViewModel.IsSelected.Where((bool value) => value).Subscribe(delegate
		{
			TrySelectEntity(entityViewModel);
		}));
		AddDisposable(entityViewModel.IsSelected.Where((bool value) => !value).Subscribe(delegate
		{
			TryUnselectEntity(entityViewModel);
		}));
		if (entityViewModel.HasNesting)
		{
			AddDisposable(entityViewModel.IsExpanded.Where((bool value) => value).Subscribe(delegate
			{
				TryAddNestedList(entityViewModel.NextSource);
			}));
			AddDisposable(entityViewModel.IsExpanded.Where((bool value) => !value).Subscribe(delegate
			{
				TryRemoveNestedList(entityViewModel.NextSource);
			}));
		}
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

	private bool TryAddNestedList(INestedListSource source)
	{
		if (!source.HasNesting)
		{
			return false;
		}
		if (source is NestedSelectionGroupEntityVM nestedSelectionGroupEntityVM && !nestedSelectionGroupEntityVM.IsAvailable.Value)
		{
			return false;
		}
		List<NestedSelectionGroupEntityVM> list = source.ExtractNestedEntities();
		if (list == null || !list.Any())
		{
			return false;
		}
		foreach (NestedSelectionGroupEntityVM item in list)
		{
			SubscribeNewItem(item as TViewModel);
		}
		NestedEntityCollections[source] = list.Select((NestedSelectionGroupEntityVM entity) => entity).ToList();
		DoAddNestedList(source);
		return true;
	}

	protected abstract void DoAddNestedList(INestedListSource source);

	private bool TryRemoveNestedList(INestedListSource source)
	{
		if (source is NestedSelectionGroupEntityVM nestedSelectionGroupEntityVM && !nestedSelectionGroupEntityVM.IsAvailable.Value)
		{
			return false;
		}
		if (!NestedEntityCollections.ContainsKey(source))
		{
			return false;
		}
		foreach (NestedSelectionGroupEntityVM item in NestedEntityCollections[source])
		{
			if (item.HasNesting && item.IsExpanded.Value)
			{
				item.SetExpanded(state: false);
			}
			item.Dispose();
		}
		NestedEntityCollections.Remove(source);
		DoRemoveNestedList(source);
		return true;
	}

	protected abstract void DoRemoveNestedList(INestedListSource viewModel);
}
