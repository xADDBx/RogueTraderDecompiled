using UnityEngine;

namespace Kingmaker.Utility.GameConst;

public static class UIGlobalMapConsts
{
	public static readonly Vector2[] Pivots = new Vector2[8]
	{
		new Vector2(0.5f, 1f),
		new Vector2(0f, 0.5f),
		new Vector2(0.5f, 0f),
		new Vector2(1f, 0.5f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0f, 1f)
	};
}
