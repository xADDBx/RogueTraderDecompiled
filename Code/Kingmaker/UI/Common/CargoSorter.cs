using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.Items;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UI.Common;

public static class CargoSorter
{
	public static List<CargoEntity> SortCargo(List<CargoEntity> cargo, ItemsSorterType type)
	{
		switch (type)
		{
		case ItemsSorterType.TypeUp:
			cargo.Sort(CompareByType);
			break;
		case ItemsSorterType.TypeDown:
			cargo.Sort(CompareByType);
			cargo.Reverse();
			break;
		case ItemsSorterType.NameUp:
			cargo.Sort(CompareByName);
			break;
		case ItemsSorterType.NameDown:
			cargo.Sort(CompareByName);
			cargo.Reverse();
			break;
		case ItemsSorterType.DateUp:
			cargo.Sort(CompareByDate);
			break;
		case ItemsSorterType.DateDown:
			cargo.Sort(CompareByDate);
			cargo.Reverse();
			break;
		}
		return cargo;
	}

	public static int CompareBy(CargoEntity a, CargoEntity b, ItemsSorterType type)
	{
		return type switch
		{
			ItemsSorterType.TypeUp => CompareByType(a, b), 
			ItemsSorterType.TypeDown => -CompareByType(a, b), 
			ItemsSorterType.NameUp => CompareByName(a, b), 
			ItemsSorterType.NameDown => -CompareByName(a, b), 
			ItemsSorterType.DateUp => CompareByDate(a, b), 
			ItemsSorterType.DateDown => -CompareByDate(a, b), 
			ItemsSorterType.CargoValue => -CompareByValue(a, b), 
			_ => 0, 
		};
	}

	private static int CompareByType(CargoEntity a, CargoEntity b)
	{
		return (a?.Blueprint?.OriginType.CompareTo(b?.Blueprint?.OriginType)).GetValueOrDefault();
	}

	private static int CompareByName(CargoEntity a, CargoEntity b)
	{
		return string.CompareOrdinal(a?.Blueprint?.Name, b?.Blueprint?.Name);
	}

	private static int CompareByDate(CargoEntity a, CargoEntity b)
	{
		TimeSpan timeSpan = ((a?.Inventory?.Items).Empty<ItemEntity>() ? default(TimeSpan) : a.Inventory.Items.Max((ItemEntity i) => i.Time));
		TimeSpan value = ((b?.Inventory?.Items).Empty<ItemEntity>() ? default(TimeSpan) : b.Inventory.Items.Max((ItemEntity i) => i.Time));
		return timeSpan.CompareTo(value);
	}

	private static int CompareByValue(CargoEntity a, CargoEntity b)
	{
		int num = ((a != null && VendorHelper.Vendor.VendorFaction.CargoTypes.Contains(a.Blueprint.OriginType)) ? Game.Instance.BlueprintRoot.SystemMechanics.CargoRoot.GetTemplate(a.Blueprint.OriginType).ReputationPointsCost : 0);
		int value = ((b != null && VendorHelper.Vendor.VendorFaction.CargoTypes.Contains(b.Blueprint.OriginType)) ? Game.Instance.BlueprintRoot.SystemMechanics.CargoRoot.GetTemplate(b.Blueprint.OriginType).ReputationPointsCost : 0);
		if (num.CompareTo(value) != 0)
		{
			return num.CompareTo(value);
		}
		return string.Compare(a?.Name, b?.Name, StringComparison.Ordinal);
	}
}
