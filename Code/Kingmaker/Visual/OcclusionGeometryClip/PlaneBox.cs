using System;
using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

[Serializable]
public struct PlaneBox
{
	public Vector4 PlaneX;

	public Vector4 PlaneY;

	public Vector4 PlaneZ;

	public Vector3 Extents;

	public PlaneBox(in Vector3 center, in Quaternion rotation, in Vector3 size)
	{
		Vector3 lhs = rotation * new Vector3(1f, 0f, 0f);
		Vector3 lhs2 = rotation * new Vector3(0f, 1f, 0f);
		Vector3 lhs3 = rotation * new Vector3(0f, 0f, 1f);
		PlaneX = new Vector4(lhs.x, lhs.y, lhs.z, 0f - Vector3.Dot(lhs, center));
		PlaneY = new Vector4(lhs2.x, lhs2.y, lhs2.z, 0f - Vector3.Dot(lhs2, center));
		PlaneZ = new Vector4(lhs3.x, lhs3.y, lhs3.z, 0f - Vector3.Dot(lhs3, center));
		Extents = size / 2f;
	}

	public bool ContainsPoint(in Vector4 point)
	{
		if (Mathf.Abs(Vector4.Dot(PlaneX, point)) <= Extents.x && Mathf.Abs(Vector4.Dot(PlaneY, point)) <= Extents.y)
		{
			return Mathf.Abs(Vector4.Dot(PlaneZ, point)) <= Extents.z;
		}
		return false;
	}

	public Matrix4x4 GetLocalToWorldMatrix()
	{
		Vector3 pos = (Vector3)PlaneX * (0f - PlaneX.w) + (Vector3)PlaneY * (0f - PlaneY.w) + (Vector3)PlaneZ * (0f - PlaneZ.w);
		Quaternion q = Quaternion.LookRotation(PlaneZ, PlaneY);
		Vector3 s = Extents * 2f;
		return Matrix4x4.TRS(pos, q, s);
	}

	public override string ToString()
	{
		return $"(x:{PlaneX}, y:{PlaneY}, z:{PlaneZ}, e:{Extents}";
	}
}
