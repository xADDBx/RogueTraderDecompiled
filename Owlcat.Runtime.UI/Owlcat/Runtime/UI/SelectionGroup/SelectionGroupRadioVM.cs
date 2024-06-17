using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Owlcat.Runtime.UI.SelectionGroup;

public class SelectionGroupRadioVM<TViewModel> : SelectionGroupVM<TViewModel> where TViewModel : SelectionGroupEntityVM
{
	public ReactiveProperty<TViewModel> SelectedEntity;

	private bool m_Cyclical;

	public SelectionGroupRadioVM(List<TViewModel> visibleCollection, ReactiveProperty<TViewModel> entity = null, bool cyclical = false)
		: base(visibleCollection)
	{
		SelectedEntity = entity ?? new ReactiveProperty<TViewModel>();
		AddDisposable(SelectedEntity.Subscribe(delegate
		{
			SetupSelectedState();
		}));
		m_Cyclical = cyclical;
	}

	public SelectionGroupRadioVM(ReactiveCollection<TViewModel> entitiesCollection, ReactiveProperty<TViewModel> entity = null, bool cyclical = false)
		: base(entitiesCollection)
	{
		SelectedEntity = entity ?? new ReactiveProperty<TViewModel>();
		AddDisposable(SelectedEntity.Subscribe(delegate
		{
			SetupSelectedState();
		}));
		m_Cyclical = cyclical;
	}

	protected override bool TryDoSelect(TViewModel viewModel)
	{
		if (SelectedEntity == null)
		{
			return false;
		}
		if (SelectedEntity.Value != viewModel)
		{
			SelectedEntity.Value = viewModel;
			return true;
		}
		return false;
	}

	protected override bool TryDoUnselectSelect(TViewModel viewModel)
	{
		if (SelectedEntity != null && SelectedEntity.Value == viewModel)
		{
			SelectedEntity.Value = null;
			return true;
		}
		return false;
	}

	public override void SetupSelectedState()
	{
		foreach (TViewModel item in EntitiesCollection)
		{
			item.SetSelected(SelectedEntity.Value == item);
		}
	}

	public bool SelectPrevValidEntity()
	{
		int count = EntitiesCollection.IndexOf(SelectedEntity.Value);
		TViewModel val = EntitiesCollection.ToList().GetRange(0, count).LastOrDefault((TViewModel entity) => entity.IsAvailable.Value);
		if (m_Cyclical && val == null)
		{
			val = EntitiesCollection.LastOrDefault((TViewModel entity) => entity.IsAvailable.Value);
		}
		if (val == null)
		{
			return false;
		}
		SelectedEntity.Value = val;
		return true;
	}

	public bool SelectNextValidEntity()
	{
		int num = EntitiesCollection.IndexOf(SelectedEntity.Value);
		TViewModel val = EntitiesCollection.ToList().GetRange(num + 1, EntitiesCollection.Count - num - 1).FirstOrDefault((TViewModel entity) => entity.IsAvailable.Value);
		if (m_Cyclical && val == null)
		{
			val = EntitiesCollection.FirstOrDefault((TViewModel entity) => entity.IsAvailable.Value);
		}
		if (val == null)
		{
			return false;
		}
		SelectedEntity.Value = val;
		return true;
	}

	public void MoveIndex(TViewModel selectionItemVM, int index)
	{
		int num = EntitiesCollection.IndexOf(selectionItemVM);
		if (num >= 0)
		{
			EntitiesCollection.Move(num, index);
		}
	}
}
