using System;
using System.Collections.Generic;

namespace Kingmaker.UnitLogic.Parts;

internal class VendorLootItemEqualityComparer : IEqualityComparer<VendorLootItem>
{
	public bool Equals(VendorLootItem x, VendorLootItem y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null)
		{
			return false;
		}
		if (y == null)
		{
			return false;
		}
		if (x.GetType() != y.GetType())
		{
			return false;
		}
		if (object.Equals(x.Item, y.Item) && x.Count == y.Count && x.ReputationToUnlock == y.ReputationToUnlock && x.ProfitFactorCosts == y.ProfitFactorCosts)
		{
			return x.Diversity == y.Diversity;
		}
		return false;
	}

	public int GetHashCode(VendorLootItem obj)
	{
		return HashCode.Combine(obj.Item, obj.ReputationToUnlock, obj.ProfitFactorCosts, obj.Diversity);
	}
}
