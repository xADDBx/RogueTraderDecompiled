using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

public class SrollbarWithPlaceholder : Scrollbar
{
	public GameObject Placeholder;

	public override void GraphicUpdateComplete()
	{
		base.GraphicUpdateComplete();
		Placeholder.gameObject.SetActive(!base.gameObject.activeSelf);
		base.size = 0f;
		base.value = 1f;
	}
}
