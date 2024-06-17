using System;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.Code.Utility.Debug;

public static class VectorDebugExtensions
{
	public static string ToStrBit(this float v)
	{
		return v.Int().ToString("x8");
	}

	public static string ToStrBit(this double v)
	{
		return v.Int().ToString("x8");
	}

	public static string ToStrBit(this Vector2 v)
	{
		return $"({v.x.Int():x8},{v.y.Int():x8})";
	}

	public static string ToStrBit(this Vector3 v)
	{
		return $"({v.x.Int():x8},{v.y.Int():x8},{v.z.Int():x8})";
	}

	public static string ToStrBit(this Vector4 v)
	{
		return $"({v.x.Int():x8},{v.y.Int():x8},{v.z.Int():x8},{v.w.Int():x8})";
	}

	public static string ToStrBit(this Matrix4x4 m)
	{
		return "(" + m.GetRow(0).ToStrBit() + " " + m.GetRow(1).ToStrBit() + " " + m.GetRow(2).ToStrBit() + " " + m.GetRow(3).ToStrBit() + ")";
	}

	public static string ToStrBit(this Quaternion v)
	{
		return $"({v.x.Int():x8},{v.y.Int():x8},{v.z.Int():x8},{v.w.Int():x8})";
	}

	public static string ToStrBit(this float2 v)
	{
		return $"({v.x.Int():x8},{v.y.Int():x8})";
	}

	private static int Int(this float value)
	{
		return BitConverter.SingleToInt32Bits(value);
	}

	private static long Int(this double value)
	{
		return BitConverter.DoubleToInt64Bits(value);
	}
}
