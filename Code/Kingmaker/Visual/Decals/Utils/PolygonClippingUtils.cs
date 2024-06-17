using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Decals.Utils;

public static class PolygonClippingUtils
{
	private static readonly Plane s_Right = new Plane(Vector3.right, 0.5f);

	private static readonly Plane s_Left = new Plane(Vector3.left, 0.5f);

	private static readonly Plane s_Top = new Plane(Vector3.up, 0.5f);

	private static readonly Plane s_Bottom = new Plane(Vector3.down, 0.5f);

	private static readonly Plane s_Front = new Plane(Vector3.forward, 0.5f);

	private static readonly Plane s_Back = new Plane(Vector3.back, 0.5f);

	private static readonly List<Vector3> s_TempVerts = new List<Vector3>();

	private static readonly List<Vector3> s_TempNormals = new List<Vector3>();

	private static readonly List<Vector2> s_TempLmapUv = new List<Vector2>();

	public static void Clip(List<Vector3> poly, List<Vector3> normals, List<Vector2> lmapUv, Vector3 pushNormal)
	{
		s_TempVerts.Clear();
		s_TempNormals.Clear();
		s_TempLmapUv.Clear();
		List<Vector3> inV = poly;
		List<Vector3> inN = normals;
		List<Vector2> inL = lmapUv;
		List<Vector3> outV = s_TempVerts;
		List<Vector3> outN = s_TempNormals;
		List<Vector2> outL = s_TempLmapUv;
		Clip(inV, inN, inL, outV, outN, outL, s_Right);
		Swap(ref inV, ref inN, ref inL, ref outV, ref outN, ref outL);
		Clip(inV, inN, inL, outV, outN, outL, s_Left);
		Swap(ref inV, ref inN, ref inL, ref outV, ref outN, ref outL);
		Clip(inV, inN, inL, outV, outN, outL, s_Top);
		Swap(ref inV, ref inN, ref inL, ref outV, ref outN, ref outL);
		Clip(inV, inN, inL, outV, outN, outL, s_Bottom);
		Swap(ref inV, ref inN, ref inL, ref outV, ref outN, ref outL);
		Clip(inV, inN, inL, outV, outN, outL, s_Front);
		Swap(ref inV, ref inN, ref inL, ref outV, ref outN, ref outL);
		Clip(inV, inN, inL, outV, outN, outL, s_Back);
		Swap(ref inV, ref inN, ref inL, ref outV, ref outN, ref outL);
	}

	private static void Swap(ref List<Vector3> inV, ref List<Vector3> inN, ref List<Vector2> inL, ref List<Vector3> outV, ref List<Vector3> outN, ref List<Vector2> outL)
	{
		List<Vector3> list = inV;
		List<Vector3> list2 = inN;
		List<Vector2> list3 = inL;
		inV = outV;
		inN = outN;
		inL = outL;
		outV = list;
		outN = list2;
		outL = list3;
		outV.Clear();
		outN.Clear();
		outL.Clear();
	}

	private static void Clip(List<Vector3> poly, List<Vector3> normals, List<Vector2> lmapUv, List<Vector3> outPoly, List<Vector3> outNormals, List<Vector2> outLmapUv, Plane plane)
	{
		for (int i = 0; i < poly.Count; i++)
		{
			int index = (i + 1) % poly.Count;
			Vector3 vector = poly[i];
			Vector3 vector2 = poly[index];
			bool side = plane.GetSide(vector);
			bool side2 = plane.GetSide(vector2);
			if (side)
			{
				outPoly.Add(vector);
				outNormals.Add(normals[i]);
				if (lmapUv.Count > 0)
				{
					outLmapUv.Add(lmapUv[i]);
				}
			}
			if (side != side2)
			{
				float dist;
				Vector3 item = PlaneLineCast(plane, vector, vector2, out dist);
				outPoly.Add(item);
				float t = dist / Vector3.Distance(vector, vector2);
				outNormals.Add(Vector3.Lerp(normals[i], normals[index], t));
				if (lmapUv.Count > 0)
				{
					Vector2 a = lmapUv[i];
					Vector2 b = lmapUv[index];
					Vector2 item2 = Vector2.Lerp(a, b, t);
					outLmapUv.Add(item2);
				}
			}
		}
	}

	private static Vector3 PlaneLineCast(Plane plane, Vector3 a, Vector3 b, out float dist)
	{
		Ray ray = new Ray(a, b - a);
		plane.Raycast(ray, out dist);
		return ray.GetPoint(dist);
	}
}
