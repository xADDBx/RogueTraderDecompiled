using System;
using UnityEngine;

namespace Kingmaker.Visual;

public static class MathHelper
{
	public static void Vector3Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
	{
		result.x = value1.x + value2.x;
		result.y = value1.y + value2.y;
		result.z = value1.z + value2.z;
	}

	public static void Vector3Subtract(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
	{
		result.x = value1.x - value2.x;
		result.y = value1.y - value2.y;
		result.z = value1.z - value2.z;
	}

	public static void Vector3Multiply(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
	{
		result.x = value1.x * value2.x;
		result.y = value1.y * value2.y;
		result.z = value1.z * value2.z;
	}

	public static void Vector3Multiply(ref Vector3 value1, float scaleFactor, out Vector3 result)
	{
		result.x = value1.x * scaleFactor;
		result.y = value1.y * scaleFactor;
		result.z = value1.z * scaleFactor;
	}

	public static void Vector3Divide(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
	{
		result.x = value1.x / value2.x;
		result.y = value1.y / value2.y;
		result.z = value1.z / value2.z;
	}

	public static void Vector3Cross(ref Vector3 vector1, ref Vector3 vector2, out Vector3 result)
	{
		float x = (float)((double)vector1.y * (double)vector2.z - (double)vector1.z * (double)vector2.y);
		float y = (float)((double)vector1.z * (double)vector2.x - (double)vector1.x * (double)vector2.z);
		float z = (float)((double)vector1.x * (double)vector2.y - (double)vector1.y * (double)vector2.x);
		result.x = x;
		result.y = y;
		result.z = z;
	}

	public static void Dot(ref Vector3 vector1, ref Vector3 vector2, out float result)
	{
		result = (float)((double)vector1.x * (double)vector2.x + (double)vector1.y * (double)vector2.y + (double)vector1.z * (double)vector2.z);
	}

	public static void Vector3Normalize(ref Vector3 value, out Vector3 result)
	{
		float num = 1f / (float)Math.Sqrt((double)value.x * (double)value.x + (double)value.y * (double)value.y + (double)value.z * (double)value.z);
		result.x = value.x * num;
		result.y = value.y * num;
		result.z = value.z * num;
	}

	public static void Vector3Length(ref Vector3 value, out float result)
	{
		result = (float)Math.Sqrt((double)value.x * (double)value.x + (double)value.y * (double)value.y + (double)value.z * (double)value.z);
	}

	public static void Vector3LengthSquared(ref Vector3 value, out float result)
	{
		result = (float)((double)value.x * (double)value.x + (double)value.y * (double)value.y + (double)value.z * (double)value.z);
	}

	public static void Vector3Distance(ref Vector3 value1, ref Vector3 value2, out float result)
	{
		float num = value1.x - value2.x;
		float num2 = value1.y - value2.y;
		float num3 = value1.z - value2.z;
		float num4 = (float)((double)num * (double)num + (double)num2 * (double)num2 + (double)num3 * (double)num3);
		result = (float)Math.Sqrt(num4);
	}

	public static void Vector3DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
	{
		float num = value1.x - value2.x;
		float num2 = value1.y - value2.y;
		float num3 = value1.z - value2.z;
		result = (float)((double)num * (double)num + (double)num2 * (double)num2 + (double)num3 * (double)num3);
	}

	public static void Vector3CatmullRom(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, ref Vector3 value4, float amount, out Vector3 result)
	{
		float num = amount * amount;
		float num2 = amount * num;
		result.x = (float)(0.5 * (2.0 * (double)value2.x + (0.0 - (double)value1.x + (double)value3.x) * (double)amount + (2.0 * (double)value1.x - 5.0 * (double)value2.x + 4.0 * (double)value3.x - (double)value4.x) * (double)num + (0.0 - (double)value1.x + 3.0 * (double)value2.x - 3.0 * (double)value3.x + (double)value4.x) * (double)num2));
		result.y = (float)(0.5 * (2.0 * (double)value2.y + (0.0 - (double)value1.y + (double)value3.y) * (double)amount + (2.0 * (double)value1.y - 5.0 * (double)value2.y + 4.0 * (double)value3.y - (double)value4.y) * (double)num + (0.0 - (double)value1.y + 3.0 * (double)value2.y - 3.0 * (double)value3.y + (double)value4.y) * (double)num2));
		result.z = (float)(0.5 * (2.0 * (double)value2.z + (0.0 - (double)value1.z + (double)value3.z) * (double)amount + (2.0 * (double)value1.z - 5.0 * (double)value2.z + 4.0 * (double)value3.z - (double)value4.z) * (double)num + (0.0 - (double)value1.z + 3.0 * (double)value2.z - 3.0 * (double)value3.z + (double)value4.z) * (double)num2));
	}

	public static void Vector3Lerp(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
	{
		result.x = value1.x + (value2.x - value1.x) * amount;
		result.y = value1.y + (value2.y - value1.y) * amount;
		result.z = value1.z + (value2.z - value1.z) * amount;
	}

	public static void QuaternionSlerp(ref Quaternion quaternion1, ref Quaternion quaternion2, double amount, out Quaternion result)
	{
		double num = quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y + quaternion1.z * quaternion2.z + quaternion1.w * quaternion2.w;
		bool flag = false;
		if (num < 0.0)
		{
			flag = true;
			num = 0.0 - num;
		}
		double num2;
		double num3;
		if (num > 0.9999989867210388)
		{
			num2 = 1.0 - amount;
			num3 = (flag ? (0.0 - amount) : amount);
		}
		else
		{
			double num4 = Math.Acos(num);
			double num5 = 1.0 / Math.Sin(num4);
			num2 = Math.Sin((1.0 - amount) * num4) * num5;
			num3 = (flag ? ((0.0 - Math.Sin(amount * num4)) * num5) : (Math.Sin(amount * num4) * num5));
		}
		result.x = (float)num2 * quaternion1.x + (float)num3 * quaternion2.x;
		result.y = (float)num2 * quaternion1.y + (float)num3 * quaternion2.y;
		result.z = (float)num2 * quaternion1.z + (float)num3 * quaternion2.z;
		result.w = (float)num2 * quaternion1.w + (float)num3 * quaternion2.w;
	}

	public static Matrix4x4 WorldToCameraMatrix(Matrix4x4 cameraWorld)
	{
		Matrix4x4 inverse = cameraWorld.inverse;
		inverse.m20 *= -1f;
		inverse.m21 *= -1f;
		inverse.m22 *= -1f;
		inverse.m23 *= -1f;
		return inverse;
	}
}
