using System.Collections.Generic;
using UniRx;

namespace Owlcat.Runtime.UI.SelectionGroup;

public class SelectionGroupCheckVM<TViewModel> : SelectionGroupVM<TViewModel> where TViewModel : SelectionGroupEntityVM
{
	public ReactiveCollection<TViewModel> SelectedEntitiesCollection;

	public SelectionGroupCheckVM(List<TViewModel> visibleCollection, ReactiveCollection<TViewModel> selectedEntitiesCollection)
		: base(visibleCollection)
	{
		SelectedEntitiesCollection = selectedEntitiesCollection;
		AddDisposable(SelectedEntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			SetupSelectedState();
		}));
	}

	public SelectionGroupCheckVM(ReactiveCollection<TViewModel> entitiesCollection, ReactiveCollection<TViewModel> selectedEntitiesCollection)
		: base(entitiesCollection)
	{
		SelectedEntitiesCollection = selectedEntitiesCollection;
		AddDisposable(SelectedEntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			SetupSelectedState();
		}));
	}

	protected override bool TryDoSelect(TViewModel viewModel)
	{
		if (viewModel == null)
		{
			return false;
		}
		if (SelectedEntitiesCollection == null)
		{
			return false;
		}
		if (!SelectedEntitiesCollection.Contains(viewModel))
		{
			SelectedEntitiesCollection.Add(viewModel);
			return true;
		}
		return false;
	}

	protected override bool TryDoUnselectSelect(TViewModel viewModel)
	{
		if (viewModel == null)
		{
			return false;
		}
		if (SelectedEntitiesCollection == null)
		{
			return false;
		}
		if (SelectedEntitiesCollection.Contains(viewModel))
		{
			SelectedEntitiesCollection.Remove(viewModel);
			return true;
		}
		return false;
	}

	public override void SetupSelectedState()
	{
		foreach (TViewModel item in EntitiesCollection)
		{
			item.SetSelected(SelectedEntitiesCollection.Contains(item));
		}
	}
}
