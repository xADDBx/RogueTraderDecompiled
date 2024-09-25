using System;
using UnityEngine;

namespace Owlcat.Runtime.Core.Math;

public struct Matrix3x3
{
	public float m00;

	public float m01;

	public float m02;

	public float m10;

	public float m11;

	public float m12;

	public float m20;

	public float m21;

	public float m22;

	public static Matrix3x3 identity
	{
		get
		{
			Matrix3x3 result = default(Matrix3x3);
			result.m00 = 1f;
			result.m11 = 1f;
			result.m22 = 1f;
			return result;
		}
	}

	public Matrix3x3(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
	{
		this.m00 = m00;
		this.m01 = m01;
		this.m02 = m02;
		this.m10 = m10;
		this.m11 = m11;
		this.m12 = m12;
		this.m20 = m20;
		this.m21 = m21;
		this.m22 = m22;
	}

	public Matrix3x3(Matrix3x3 m)
	{
		m00 = m.m00;
		m10 = m.m10;
		m20 = m.m20;
		m01 = m.m01;
		m11 = m.m11;
		m21 = m.m21;
		m02 = m.m02;
		m12 = m.m12;
		m22 = m.m22;
	}

	public float GetDeterminant()
	{
		return m00 * (m11 * m22 - m12 * m21) - m01 * (m10 * m22 - m12 * m20) + m02 * (m10 * m21 - m11 * m20);
	}

	public Matrix3x3 Invert()
	{
		float determinant = GetDeterminant();
		if (determinant == 0f)
		{
			throw new UnityException("Can't invert matrix with determinant 0");
		}
		float num = 1f / determinant;
		Matrix3x3 result = default(Matrix3x3);
		result.m00 = (m11 * m22 - m21 * m12) * num;
		result.m10 = (m20 * m12 - m10 * m22) * num;
		result.m20 = (m10 * m21 - m20 * m11) * num;
		result.m01 = (m21 * m02 - m01 * m22) * num;
		result.m11 = (m00 * m22 - m20 * m02) * num;
		result.m21 = (m20 * m01 - m00 * m21) * num;
		result.m02 = (m01 * m12 - m11 * m02) * num;
		result.m12 = (m10 * m02 - m00 * m12) * num;
		result.m22 = (m00 * m11 - m10 * m01) * num;
		return result;
	}

	public Matrix3x3 Transpose()
	{
		Matrix3x3 result = default(Matrix3x3);
		result.m00 = m00;
		result.m01 = m10;
		result.m02 = m20;
		result.m10 = m01;
		result.m11 = m11;
		result.m12 = m21;
		result.m20 = m02;
		result.m21 = m12;
		result.m22 = m22;
		return result;
	}

	public static Vector2 MultiplyVector2(Matrix3x3 m1, Vector2 inVector)
	{
		Vector2 result = default(Vector2);
		result.x = m1.m00 * inVector.x + m1.m01 * inVector.y + m1.m02;
		result.y = m1.m10 * inVector.x + m1.m11 * inVector.y + m1.m12;
		return result;
	}

	public static Vector3 MultiplyVector3(Matrix3x3 m1, Vector3 inVector)
	{
		Vector3 result = default(Vector3);
		result.x = m1.m00 * inVector.x + m1.m01 * inVector.y + m1.m02;
		result.y = m1.m10 * inVector.x + m1.m11 * inVector.y + m1.m12;
		result.z = inVector.z;
		return result;
	}

	public static Matrix3x3 MultiplyMatrix3x3(Matrix3x3 m1, Matrix3x3 m2)
	{
		Matrix3x3 result = default(Matrix3x3);
		result.m00 = m1.m00 * m2.m00 + m1.m10 * m2.m01 + m1.m20 * m2.m02;
		result.m10 = m1.m00 * m2.m10 + m1.m10 * m2.m11 + m1.m20 * m2.m12;
		result.m20 = m1.m00 * m2.m20 + m1.m10 * m2.m21 + m1.m20 * m2.m22;
		result.m01 = m1.m01 * m2.m00 + m1.m11 * m2.m01 + m1.m21 * m2.m02;
		result.m11 = m1.m01 * m2.m10 + m1.m11 * m2.m11 + m1.m21 * m2.m12;
		result.m21 = m1.m01 * m2.m20 + m1.m11 * m2.m21 + m1.m21 * m2.m22;
		result.m02 = m1.m02 * m2.m00 + m1.m12 * m2.m01 + m1.m22 * m2.m02;
		result.m12 = m1.m02 * m2.m10 + m1.m12 * m2.m11 + m1.m22 * m2.m12;
		result.m22 = m1.m02 * m2.m20 + m1.m12 * m2.m21 + m1.m22 * m2.m22;
		return result;
	}

	public static Matrix3x3 operator *(Matrix3x3 m1, Matrix3x3 m2)
	{
		return MultiplyMatrix3x3(m1, m2);
	}

	public static Vector3 operator *(Matrix3x3 m, Vector3 v)
	{
		return MultiplyVector3(m, v);
	}

	public static Vector2 operator *(Matrix3x3 m, Vector2 v)
	{
		return MultiplyVector2(m, v);
	}

	public override string ToString()
	{
		float determinant = GetDeterminant();
		return string.Format("[Matrix3x3: det={9} m00={0}, m01={1}, m02={2}, m10={3}, m11={4}, m12={5}, m20={6}, m21={7}, m22={8}]", m00, m10, m20, m01, m11, m21, m02, m12, m22, determinant);
	}

	public static Matrix3x3 CreateTranslation(Vector2 translation)
	{
		Matrix3x3 result = default(Matrix3x3);
		result.m00 = 1f;
		result.m11 = 1f;
		result.m22 = 1f;
		result.m02 = translation.x;
		result.m12 = translation.y;
		return result;
	}

	public static Matrix3x3 CreateRotation(float rotation)
	{
		float num = Mathf.Cos(rotation * (MathF.PI / 180f));
		float num2 = Mathf.Sin(rotation * (MathF.PI / 180f));
		Matrix3x3 result = default(Matrix3x3);
		result.m00 = num;
		result.m01 = 0f - num2;
		result.m10 = num2;
		result.m11 = num;
		result.m22 = 1f;
		return result;
	}

	public static Matrix3x3 CreateScale(Vector2 scale)
	{
		Matrix3x3 result = default(Matrix3x3);
		result.m00 = scale.x;
		result.m11 = scale.y;
		result.m22 = 1f;
		return result;
	}

	public static Matrix3x3 CreateTRS(Vector2 translation, float rotation, Vector2 scale)
	{
		float num = Mathf.Cos(rotation * (MathF.PI / 180f));
		float num2 = Mathf.Sin(rotation * (MathF.PI / 180f));
		Matrix3x3 result = default(Matrix3x3);
		result.m00 = scale.x * num;
		result.m01 = scale.x * (0f - num2);
		result.m02 = translation.x;
		result.m10 = scale.y * num2;
		result.m11 = scale.y * num;
		result.m12 = translation.y;
		result.m20 = 0f;
		result.m21 = 0f;
		result.m22 = 1f;
		return result;
	}
}
