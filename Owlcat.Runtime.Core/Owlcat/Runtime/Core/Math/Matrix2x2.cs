using System;
using UnityEngine;

namespace Owlcat.Runtime.Core.Math;

public struct Matrix2x2
{
	public float m00;

	public float m01;

	public float m10;

	public float m11;

	public Matrix2x2(float m00, float m01, float m10, float m11)
	{
		this.m00 = m00;
		this.m01 = m01;
		this.m10 = m10;
		this.m11 = m11;
	}

	public Matrix2x2(Vector4 v)
	{
		m00 = v.x;
		m01 = v.y;
		m10 = v.z;
		m11 = v.w;
	}

	public Matrix2x2(Matrix2x2 m)
	{
		m00 = m.m00;
		m01 = m.m01;
		m10 = m.m10;
		m11 = m.m11;
	}

	public static Matrix2x2 MultiplyMatrix2x2(Matrix2x2 m1, Matrix2x2 m2)
	{
		Matrix2x2 result = default(Matrix2x2);
		result.m00 = m1.m00 * m2.m00 + m1.m10 * m2.m01;
		result.m10 = m1.m00 * m2.m10 + m1.m10 * m2.m11;
		result.m01 = m1.m01 * m2.m00 + m1.m11 * m2.m01;
		result.m11 = m1.m01 * m2.m10 + m1.m11 * m2.m11;
		return result;
	}

	public static Matrix2x2 operator *(Matrix2x2 m1, Matrix2x2 m2)
	{
		return MultiplyMatrix2x2(m1, m2);
	}

	public static Matrix2x2 CreateRotation(float rotation)
	{
		float num = Mathf.Cos(rotation * (MathF.PI / 180f));
		float num2 = Mathf.Sin(rotation * (MathF.PI / 180f));
		Matrix2x2 result = default(Matrix2x2);
		result.m00 = num;
		result.m01 = 0f - num2;
		result.m10 = num2;
		result.m11 = num;
		return result;
	}

	public static Matrix2x2 CreateScale(Vector2 scale)
	{
		Matrix2x2 result = default(Matrix2x2);
		result.m00 = scale.x;
		result.m11 = scale.y;
		return result;
	}

	public static Matrix2x2 CreateRS(float rotation, Vector2 scale)
	{
		float num = Mathf.Cos(rotation * (MathF.PI / 180f));
		float num2 = Mathf.Sin(rotation * (MathF.PI / 180f));
		Matrix2x2 result = default(Matrix2x2);
		result.m00 = scale.x * num;
		result.m01 = scale.x * (0f - num2);
		result.m10 = scale.y * num2;
		result.m11 = scale.y * num;
		return result;
	}

	public override string ToString()
	{
		return $"[Matrix2x2: m00={m00}, m01={m01}, m10={m10}, m11={m11}]";
	}

	public Vector4 ToVector()
	{
		return new Vector4(m00, m01, m10, m11);
	}
}
