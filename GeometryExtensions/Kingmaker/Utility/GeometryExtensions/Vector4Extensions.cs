using System;
using UnityEngine;

namespace Kingmaker.Utility.GeometryExtensions;

public static class Vector4Extensions
{
	public static Vector4 Round(this Vector4 value, int decimals)
	{
		return new Vector4((float)Math.Round(value.x, decimals), (float)Math.Round(value.y, decimals), (float)Math.Round(value.z, decimals), (float)Math.Round(value.w, decimals));
	}
}
