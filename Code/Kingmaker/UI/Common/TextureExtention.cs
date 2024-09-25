using UnityEngine;

namespace Kingmaker.UI.Common;

public static class TextureExtention
{
	public static float GetAspect(this Texture texture)
	{
		return (float)texture.width / (float)texture.height;
	}
}
