using System;
using UnityEngine;

namespace Kingmaker.Utility.GeometryExtensions;

public static class QuaternionExtensions
{
	public static Quaternion Round(this Quaternion value, int decimals)
	{
		return new Quaternion((float)Math.Round(value.x, decimals), (float)Math.Round(value.y, decimals), (float)Math.Round(value.z, decimals), (float)Math.Round(value.w, decimals));
	}
}
