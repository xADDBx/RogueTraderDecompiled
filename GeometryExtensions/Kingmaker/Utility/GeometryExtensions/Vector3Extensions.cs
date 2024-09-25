using System;
using UnityEngine;

namespace Kingmaker.Utility.GeometryExtensions;

public static class Vector3Extensions
{
	public static Vector3 Round(this Vector3 value, int decimals)
	{
		return new Vector3((float)Math.Round(value.x, decimals), (float)Math.Round(value.y, decimals), (float)Math.Round(value.z, decimals));
	}

	public static bool Approximately(this Vector3 a, Vector3 b)
	{
		if (Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y))
		{
			return Mathf.Approximately(a.z, b.z);
		}
		return false;
	}
}
