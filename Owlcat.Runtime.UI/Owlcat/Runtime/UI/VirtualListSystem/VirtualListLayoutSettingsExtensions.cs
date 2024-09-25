using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public static class VirtualListLayoutSettingsExtensions
{
	public static float GetFloatFromVector2(this IVirtualListLayoutSettings settings, Vector2 value)
	{
		if (!settings.IsVertical)
		{
			return value.x;
		}
		return value.y;
	}

	public static Vector2 GetVector2FromFloat(this IVirtualListLayoutSettings settings, float value)
	{
		if (!settings.IsVertical)
		{
			return new Vector2(value, 0f);
		}
		return new Vector2(0f, value);
	}
}
