using System;
using UnityEngine;

namespace Kingmaker.Visual.OcclusionGeometryClip;

internal static class OcclusionGeometryClipEntitySystem
{
	private const int kInitialCapacity = 16;

	private static OcclusionGeometryClipEntityAreaProxy[] s_Areas = new OcclusionGeometryClipEntityAreaProxy[16];

	private static PlaneBox[] s_AreaBounds = new PlaneBox[16];

	private static int s_AreaCount = 0;

	public static void AddArea(OcclusionGeometryClipEntityAreaProxy area)
	{
		if (s_AreaCount == s_Areas.Length)
		{
			int num = s_AreaCount * 2;
			OcclusionGeometryClipEntityAreaProxy[] destinationArray = new OcclusionGeometryClipEntityAreaProxy[num];
			PlaneBox[] destinationArray2 = new PlaneBox[num];
			Array.Copy(s_Areas, destinationArray, s_AreaCount);
			Array.Copy(s_AreaBounds, destinationArray2, s_AreaCount);
			s_Areas = destinationArray;
			s_AreaBounds = destinationArray2;
		}
		int num2 = (area.RegistryIndex = s_AreaCount);
		s_AreaCount++;
		s_Areas[num2] = area;
		s_AreaBounds[num2] = area.Bounds;
	}

	public static void RemoveArea(OcclusionGeometryClipEntityAreaProxy area)
	{
		int registryIndex = area.RegistryIndex;
		int num = s_AreaCount;
		if (registryIndex != num)
		{
			s_Areas[registryIndex] = s_Areas[num];
			s_AreaBounds[registryIndex] = s_AreaBounds[num];
			s_Areas[registryIndex].RegistryIndex = registryIndex;
		}
		s_Areas[num] = null;
		s_AreaCount--;
		area.RegistryIndex = -1;
	}

	public static void UpdateArea(OcclusionGeometryClipEntityAreaProxy area)
	{
		if (area.RegistryIndex >= 0 && area.RegistryIndex < s_AreaCount)
		{
			s_AreaBounds[area.RegistryIndex] = area.Bounds;
		}
	}

	public static void AddEntity(OcclusionGeometryClipEntityProxy entity)
	{
		if (TryGetAreaAtPoint(entity.transform.position, out var result))
		{
			result.AddEntity(entity);
			entity.LinkedArea = result;
		}
	}

	public static void RemoveEntity(OcclusionGeometryClipEntityProxy entity)
	{
		if ((object)entity.LinkedArea != null)
		{
			entity.LinkedArea.RemoveEntity(entity);
			entity.LinkedArea = null;
		}
	}

	private static bool TryGetAreaAtPoint(Vector3 point, out OcclusionGeometryClipEntityAreaProxy result)
	{
		Vector4 point2 = new Vector4(point.x, point.y, point.z, 1f);
		for (int i = 0; i < s_AreaCount; i++)
		{
			if (s_AreaBounds[i].ContainsPoint(in point2))
			{
				result = s_Areas[i];
				return true;
			}
		}
		result = null;
		return false;
	}
}
