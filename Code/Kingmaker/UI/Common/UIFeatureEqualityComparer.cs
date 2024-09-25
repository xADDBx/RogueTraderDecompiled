using System.Collections.Generic;

namespace Kingmaker.UI.Common;

public class UIFeatureEqualityComparer : IEqualityComparer<UIFeature>
{
	public bool Equals(UIFeature x, UIFeature y)
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
		if (x.Feature == y.Feature && x.Param == y.Param && x.Level == y.Level && x.Rank == y.Rank)
		{
			return x.Type == y.Type;
		}
		return false;
	}

	public int GetHashCode(UIFeature obj)
	{
		return (((obj.Level * 397) ^ obj.Rank) * 397) ^ (int)obj.Type;
	}
}
