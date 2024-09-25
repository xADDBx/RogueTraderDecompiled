using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Workarounds;

public class HorizontalLayoutGroupWorkaround : HorizontalLayoutGroup
{
	public bool DoWorkaround = true;

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
