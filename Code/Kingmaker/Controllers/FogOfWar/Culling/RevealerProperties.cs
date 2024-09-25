using System;
using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.Controllers.FogOfWar.Culling;

[BurstCompile]
public struct RevealerProperties
{
	public float2 Center;

	public float Radius;

	public float HeightMin;

	public float HeightMax;

	public static readonly Comparison<RevealerProperties> Comparison = ComparisonImpl;

	private static int ComparisonImpl(RevealerProperties a, RevealerProperties b)
	{
		int num = a.Center.x.CompareTo(b.Center.x);
		if (num != 0)
		{
			return num;
		}
		num = a.Center.y.CompareTo(b.Center.y);
		if (num != 0)
		{
			return num;
		}
		num = a.Radius.CompareTo(b.Radius);
		if (num != 0)
		{
			return num;
		}
		num = a.HeightMin.CompareTo(b.HeightMin);
		if (num != 0)
		{
			return num;
		}
		return a.HeightMax.CompareTo(b.HeightMax);
	}
}
