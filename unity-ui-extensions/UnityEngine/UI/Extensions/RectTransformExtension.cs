namespace UnityEngine.UI.Extensions;

public static class RectTransformExtension
{
	public static Vector2 switchToRectTransform(this RectTransform from, RectTransform to)
	{
		Vector2 vector = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin, from.rect.height * from.pivot.y + from.rect.yMin);
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, from.position);
		screenPoint += vector;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenPoint, null, out var localPoint);
		Vector2 vector2 = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin, to.rect.height * to.pivot.y + to.rect.yMin);
		return to.anchoredPosition + localPoint - vector2;
	}
}
