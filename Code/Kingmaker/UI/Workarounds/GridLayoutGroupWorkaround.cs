using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Workarounds;

[Obsolete("Workaround disabled, use default component instead")]
public class GridLayoutGroupWorkaround : GridLayoutGroup
{
	public bool DoWorkaround;

	public override void CalculateLayoutInputHorizontal()
	{
		if (DoWorkaround)
		{
			int childCount = base.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				_ = ((RectTransform)base.transform.GetChild(i)).anchoredPosition;
			}
		}
		base.CalculateLayoutInputHorizontal();
	}
}
