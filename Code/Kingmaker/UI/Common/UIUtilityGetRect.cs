using Kingmaker.GameModes;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UI.Common;

public static class UIUtilityGetRect
{
	public static Rect GetRect(float percentWidth, float percentHeight)
	{
		float num = percentWidth / 100f;
		float num2 = percentHeight / 100f;
		float num3 = Screen.width;
		float num4 = Screen.height;
		float num5 = num3 * num / num3;
		float num6 = num4 * num2 / num4;
		float x = 0.5f - num5 / 2f;
		float y = 0.5f - num6 / 2f;
		return new Rect(x, y, num5, num6);
	}

	public static bool CheckObjectInRect(Vector3 point, float percentWidth, float percentHeight)
	{
		Vector3 vector = CameraRig.Instance.Camera.Or(null)?.WorldToViewportPoint(point) ?? Vector3.positiveInfinity;
		Vector3 point2 = new Vector3(vector.x, vector.y);
		return GetRect(percentWidth, percentHeight).Contains(point2);
	}

	public static Vector3 GetObjectPositionInCamera(Vector3 objectPosition)
	{
		if (Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			return CameraRig.WorldToViewportMainCamera(objectPosition);
		}
		return CameraRig.Instance.WorldToViewport(objectPosition);
	}

	public static Vector2 PixelPositionInRect(Rect parentRect, Vector3 canvasPosition, Transform parent)
	{
		Vector2 result = default(Vector2);
		result.x = canvasPosition.x * parentRect.width;
		result.y = canvasPosition.y * parentRect.height;
		return result;
	}

	public static Vector2 ObjectPixelPositionInRect(Vector3 objectPosition, Transform parent)
	{
		Vector3 objectPositionInCamera = GetObjectPositionInCamera(objectPosition);
		Rect rect = ((RectTransform)parent.parent).rect;
		Vector2 result = default(Vector2);
		result.x = objectPositionInCamera.x * rect.width;
		result.y = objectPositionInCamera.y * rect.height;
		return result;
	}
}
