using UnityEngine;

namespace UnityModManagerNet;

public static class Extensions
{
	public static void Draw<T>(this T instance, UnityModManager.ModEntry mod) where T : class, IDrawable, new()
	{
		UnityModManager.UI.DrawFields(ref instance, mod, DrawFieldMask.OnlyDrawAttr, instance.OnChange);
	}

	public static void Draw<T>(this T instance, UnityModManager.ModEntry mod, int unique) where T : class, IDrawable, new()
	{
		UnityModManager.UI.DrawFields(ref instance, mod, unique, DrawFieldMask.OnlyDrawAttr, instance.OnChange);
	}

	public static void ResizeToIfLess(this Texture2D tex, int size)
	{
		float num = size;
		float num2 = Mathf.Max((float)((Texture)tex).width / num, (float)((Texture)tex).height / num);
		if (num2 < 1f)
		{
			TextureScale.Bilinear(tex, (int)((float)((Texture)tex).width / num2), (int)((float)((Texture)tex).height / num2));
		}
	}

	public static void CopyFieldsTo<T1, T2>(this T1 from, ref T2 to) where T1 : ICopyable, new() where T2 : new()
	{
		object obj = to;
		Utils.CopyFields<T1, T2>(from, obj, CopyFieldMask.OnlyCopyAttr);
		to = (T2)obj;
	}
}
