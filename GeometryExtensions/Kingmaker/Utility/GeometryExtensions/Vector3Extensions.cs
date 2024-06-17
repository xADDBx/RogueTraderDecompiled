using System;
using UnityEngine;

namespace Kingmaker.Utility.GeometryExtensions;

public static class Vector3Extensions
{
	public static Vector3 Round(this Vector3 value, int decimals)
	{
		return new Vector3((float)Math.Round(value.x, decimals), (float)Math.Round(value.y, decimals), (float)Math.Round(value.z, decimals));
	}
}
