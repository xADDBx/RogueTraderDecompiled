using System;
using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public static class PhysicUtils
{
	private static Vector3[] s_NormalizedDiscPoints;

	static PhysicUtils()
	{
		s_NormalizedDiscPoints = new Vector3[8];
		FillPointsForYAngle(Mathf.Sin(MathF.PI * 3f / 4f), 0f, 0);
		FillPointsForYAngle(Mathf.Sin(MathF.PI / 2f), 0f, 4);
		static void FillPointsForYAngle(float sinYAngle, float y, int startIndex)
		{
			float num = 0f;
			for (int i = startIndex; i < startIndex + 4; i++)
			{
				float num2 = Mathf.Cos(num);
				float num3 = Mathf.Sin(num);
				float x = sinYAngle * num2;
				float z = sinYAngle * num3;
				s_NormalizedDiscPoints[i] = new Vector3(x, y, z);
				num += MathF.PI / 4f;
			}
		}
	}

	public static bool HeuristicDiscCastDown(Vector3 origin, float radius, out RaycastHit hitInfo, float maxDistance, int layerMask)
	{
		RaycastHit? raycastHit = null;
		Vector3[] array = s_NormalizedDiscPoints;
		foreach (Vector3 vector in array)
		{
			if (Physics.Raycast(origin + vector * radius, Vector3.down, out var hitInfo2, maxDistance, layerMask) && (!raycastHit.HasValue || raycastHit.Value.distance > hitInfo2.distance))
			{
				raycastHit = hitInfo2;
			}
		}
		hitInfo = raycastHit.GetValueOrDefault();
		return raycastHit.HasValue;
	}
}
