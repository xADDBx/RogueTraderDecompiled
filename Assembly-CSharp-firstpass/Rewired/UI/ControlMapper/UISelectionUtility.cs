using System.Collections.Generic;
using Rewired.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

public static class UISelectionUtility
{
	public static Selectable FindNextSelectable(Selectable selectable, Transform transform, Vector3 direction)
	{
		RectTransform rectTransform = transform as RectTransform;
		if (rectTransform == null)
		{
			return null;
		}
		IList<Selectable> allSelectables = Selectable.allSelectables;
		int count = allSelectables.Count;
		direction.Normalize();
		Vector2 vector = direction;
		Vector2 vector2 = UITools.GetPointOnRectEdge(rectTransform, vector);
		bool flag = vector == Vector2.right * -1f || vector == Vector2.right;
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		Selectable selectable2 = null;
		Selectable selectable3 = null;
		Vector2 point = vector2 + vector * 999999f;
		for (int i = 0; i < count; i++)
		{
			Selectable selectable4 = allSelectables[i];
			if (selectable4 == selectable || selectable4 == null || selectable4.navigation.mode == Navigation.Mode.None || (!selectable4.IsInteractable() && !ReflectionTools.GetPrivateField<Selectable, bool>(selectable4, "m_GroupsAllowInteraction")))
			{
				continue;
			}
			RectTransform rectTransform2 = selectable4.transform as RectTransform;
			if (rectTransform2 == null)
			{
				continue;
			}
			Rect rect = UITools.InvertY(UITools.TransformRectTo(rectTransform2, transform, rectTransform2.rect));
			if (MathTools.LineIntersectsRect(vector2, point, rect, out var sqrMagnitude))
			{
				if (flag)
				{
					sqrMagnitude *= 0.25f;
				}
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					selectable3 = selectable4;
				}
			}
			Vector2 to = (Vector2)UnityTools.TransformPoint(rectTransform2, transform, rectTransform2.rect.center) - vector2;
			if (!(Mathf.Abs(Vector2.Angle(vector, to)) > 75f))
			{
				float sqrMagnitude2 = to.sqrMagnitude;
				if (sqrMagnitude2 < num)
				{
					num = sqrMagnitude2;
					selectable2 = selectable4;
				}
			}
		}
		if (selectable3 != null && selectable2 != null)
		{
			if (num2 > num)
			{
				return selectable2;
			}
			return selectable3;
		}
		return selectable3 ?? selectable2;
	}
}
