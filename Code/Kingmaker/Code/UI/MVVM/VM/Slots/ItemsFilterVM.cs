using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public class ItemsFilterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private List<ISlotsGroupVM> m_SlotsGroupVms;

	public readonly ReactiveProperty<ItemsFilterType> CurrentFilter;

	public readonly ReactiveProperty<ItemsSorterType> CurrentSorter;

	public readonly ItemsFilterSearchVM ItemsFilterSearchVM;

	public OwlcatDropdownVM SorterDropdownVM;

	public readonly ReactiveCommand OnFilterReset = new ReactiveCommand();

	public ItemsFilterVM(ISlotsGroupVM slotsGroupVM)
		: this(new List<ISlotsGroupVM> { slotsGroupVM })
	{
	}

	public ItemsFilterVM(List<ISlotsGroupVM> slotsGroupVms)
	{
		ISlotsGroupVM slotsGroupVM = slotsGroupVms[0];
		CurrentFilter = slotsGroupVM.FilterType;
		CurrentSorter = slotsGroupVM.SorterType;
		AddDisposable(ItemsFilterSearchVM = new ItemsFilterSearchVM(slotsGroupVM.SearchString));
		AddDisposable(CurrentFilter.Subscribe(delegate(ItemsFilterType value)
		{
			slotsGroupVms.ForEach(delegate(ISlotsGroupVM s)
			{
				s.FilterType.Value = value;
			});
		}));
		AddDisposable(CurrentSorter.Subscribe(delegate(ItemsSorterType value)
		{
			slotsGroupVms.ForEach(delegate(ISlotsGroupVM s)
			{
				s.SorterType.Value = value;
			});
		}));
		SetSorterDropDownVM();
	}

	protected override void DisposeImplementation()
	{
		SorterDropdownVM?.Dispose();
	}

	public void SetCurrentFilter(ItemsFilterType newFilter)
	{
		CurrentFilter.Value = newFilter;
	}

	public void SetCurrentSorter(ItemsSorterType newSorter)
	{
		CurrentSorter.Value = newSorter;
	}

	public void ResetFilter()
	{
		OnFilterReset?.Execute();
	}

	private void SetSorterDropDownVM()
	{
		List<DropdownItemVM> list = new List<DropdownItemVM>();
		int index = 0;
		Array values = Enum.GetValues(typeof(ItemsSorterType));
		for (int i = 0; i < values.Length; i++)
		{
			ItemsSorterType itemsSorterType = (ItemsSorterType)values.GetValue(i);
			list.Add(new DropdownItemVM(LocalizedTexts.Instance.ItemsFilter.GetText(itemsSorterType)));
			if (itemsSorterType == CurrentSorter.Value)
			{
				index = i;
			}
		}
		AddDisposable(SorterDropdownVM = new OwlcatDropdownVM(list, index));
	}
}
