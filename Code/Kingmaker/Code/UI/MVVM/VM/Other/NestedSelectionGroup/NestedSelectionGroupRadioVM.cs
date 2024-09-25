using System.Collections.Generic;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Other.NestedSelectionGroup;

public class NestedSelectionGroupRadioVM<TViewModel> : NestedSelectionGroupVM<TViewModel> where TViewModel : NestedSelectionGroupEntityVM
{
	public ReactiveDictionary<INestedListSource, ReactiveProperty<NestedSelectionGroupEntityVM>> NestedSelectedEntities = new ReactiveDictionary<INestedListSource, ReactiveProperty<NestedSelectionGroupEntityVM>>();

	public NestedSelectionGroupRadioVM(INestedListSource nestedListSource)
		: base(nestedListSource)
	{
	}

	protected override void SubscribeNewItem(TViewModel entityViewModel)
	{
		base.SubscribeNewItem(entityViewModel);
		AddDisposable(entityViewModel.IsSelected.Subscribe(entityViewModel.SetExpanded));
		AddDisposable(entityViewModel.IsExpanded.Subscribe(entityViewModel.SetSelected));
	}

	protected override bool TryDoSelect(TViewModel viewModel)
	{
		if (NestedSelectedEntities.ContainsKey(viewModel.Source))
		{
			ReactiveProperty<NestedSelectionGroupEntityVM> reactiveProperty = NestedSelectedEntities[viewModel.Source];
			if (reactiveProperty.Value != viewModel)
			{
				reactiveProperty.Value = viewModel;
				return true;
			}
			return false;
		}
		return false;
	}

	protected override bool TryDoUnselectSelect(TViewModel viewModel)
	{
		if (NestedSelectedEntities.ContainsKey(viewModel.Source))
		{
			ReactiveProperty<NestedSelectionGroupEntityVM> reactiveProperty = NestedSelectedEntities[viewModel.Source];
			if (reactiveProperty.Value == viewModel)
			{
				reactiveProperty.Value = null;
				return true;
			}
			return false;
		}
		return false;
	}

	protected override void DoAddNestedList(INestedListSource source)
	{
		ReactiveProperty<NestedSelectionGroupEntityVM> selectedReactiveProperty = source.GetSelectedReactiveProperty();
		NestedSelectedEntities[source] = selectedReactiveProperty;
		AddDisposable(selectedReactiveProperty.Subscribe(delegate
		{
			SetupNestedSelectedState(source);
		}));
	}

	private void SetupNestedSelectedState(INestedListSource viewModel)
	{
		if (!NestedEntityCollections.ContainsKey(viewModel) || !NestedSelectedEntities.ContainsKey(viewModel))
		{
			return;
		}
		List<NestedSelectionGroupEntityVM> list = NestedEntityCollections[viewModel];
		ReactiveProperty<NestedSelectionGroupEntityVM> reactiveProperty = NestedSelectedEntities[viewModel];
		foreach (NestedSelectionGroupEntityVM item in list)
		{
			item.SetSelected(reactiveProperty.Value == item);
		}
	}

	protected override void DoRemoveNestedList(INestedListSource viewModel)
	{
		if (NestedSelectedEntities.ContainsKey(viewModel))
		{
			NestedSelectedEntities[viewModel].Dispose();
			NestedSelectedEntities.Remove(viewModel);
		}
	}
}
