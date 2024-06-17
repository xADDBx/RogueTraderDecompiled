using System;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class CampPositionMarker : MonoBehaviour, IComparable<CampPositionMarker>
{
	public CampPositionType Type;

	public int Order;

	public int CompareTo(CampPositionMarker other)
	{
		if ((object)this == other)
		{
			return 0;
		}
		if ((object)other == null)
		{
			return 1;
		}
		int num = Type.CompareTo(other.Type);
		if (num != 0)
		{
			return num;
		}
		return Order.CompareTo(other.Order);
	}
}
