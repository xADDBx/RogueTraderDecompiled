using System;
using System.Collections.Generic;

namespace Kingmaker.UI.SurfaceCombatHUD;

public readonly struct HighlightData : IEqualityComparer<HighlightData>
{
	public readonly float startTime;

	public readonly float diration;

	public HighlightData(float startTime, float diration)
	{
		this.startTime = startTime;
		this.diration = diration;
	}

	public bool Equals(HighlightData x, HighlightData y)
	{
		float num = x.startTime;
		if (num.Equals(y.startTime))
		{
			num = x.diration;
			return num.Equals(y.diration);
		}
		return false;
	}

	public int GetHashCode(HighlightData obj)
	{
		return HashCode.Combine(obj.startTime, obj.diration);
	}
}
