using UnityEngine;

namespace Owlcat.Runtime.Core.Utility;

public static class LayersEx
{
	public static bool IsLayerMask(this GameObject go, Layers mask)
	{
		return ((uint)mask & (uint)(1 << go.layer)) != 0;
	}
}
