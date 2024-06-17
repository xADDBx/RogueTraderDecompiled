using System;
using UnityEngine;

namespace Kingmaker.Utility.GeometryExtensions;

public static class Matrix4x4Extensions
{
	public static Matrix4x4 Round(this Matrix4x4 value, int decimals)
	{
		for (int i = 0; i < 16; i++)
		{
			value[i] = (float)Math.Round(value[i], decimals);
		}
		return value;
	}
}
