using UnityEngine;

namespace Kingmaker.UI.Selection;

public class SelectUtil
{
	public static Rect GetViewportBounds(Camera camera, Vector3 anchor, Vector3 outer)
	{
		Vector3 lhs = camera.ScreenToViewportPoint(anchor);
		Vector3 rhs = camera.ScreenToViewportPoint(outer);
		Vector3 vector = Vector3.Min(lhs, rhs);
		Vector3 vector2 = Vector3.Max(lhs, rhs);
		return new Rect(vector, vector2 - vector);
	}

	public static bool IsWithinBounds(Camera camera, Rect viewportBounds, Bounds bounds)
	{
		Vector3 center = bounds.center;
		Vector3 vector = bounds.size / 2f;
		Vector3 vector2 = camera.WorldToViewportPoint(center + new Vector3(vector.x, vector.y, vector.z));
		Vector3 vector3 = camera.WorldToViewportPoint(center + new Vector3(vector.x, 0f - vector.y, vector.z));
		Vector3 vector4 = camera.WorldToViewportPoint(center + new Vector3(vector.x, vector.y, 0f - vector.z));
		Vector3 vector5 = camera.WorldToViewportPoint(center + new Vector3(vector.x, 0f - vector.y, 0f - vector.z));
		Vector3 vector6 = camera.WorldToViewportPoint(center + new Vector3(0f - vector.x, 0f - vector.y, vector.z));
		Vector3 vector7 = camera.WorldToViewportPoint(center + new Vector3(0f - vector.x, vector.y, vector.z));
		Vector3 vector8 = camera.WorldToViewportPoint(center + new Vector3(0f - vector.x, 0f - vector.y, 0f - vector.z));
		Vector3 vector9 = camera.WorldToViewportPoint(center + new Vector3(0f - vector.x, vector.y, 0f - vector.z));
		Vector2 vector10 = Vector2.Min(vector2, Vector2.Min(vector3, Vector2.Min(vector4, Vector2.Min(vector5, Vector2.Min(vector6, Vector2.Min(vector7, Vector2.Min(vector8, vector9)))))));
		Vector2 vector11 = Vector2.Max(vector2, Vector2.Max(vector3, Vector2.Max(vector4, Vector2.Max(vector5, Vector2.Max(vector6, Vector2.Max(vector7, Vector2.Max(vector8, vector9)))))));
		return viewportBounds.Overlaps(new Rect(vector10, vector11 - vector10));
	}
}
