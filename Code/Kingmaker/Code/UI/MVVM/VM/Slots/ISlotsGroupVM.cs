using System;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface ISlotsGroupVM : IDisposable
{
	ItemsCollection MechanicCollection { get; }

	ReactiveProperty<ItemsFilterType> FilterType { get; }

	ReactiveProperty<ItemsSorterType> SorterType { get; }

	ReactiveProperty<string> SearchString { get; }

	ItemSlotsGroupType Type { get; }

	void UpdateVisibleCollection();
}
