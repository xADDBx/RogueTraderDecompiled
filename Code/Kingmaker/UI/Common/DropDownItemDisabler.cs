using UnityEngine;

namespace Kingmaker.UI.Common;

public class DropDownItemDisabler : MonoBehaviour
{
	public void Start()
	{
		base.gameObject.SetActive(base.transform.GetSiblingIndex() > 1);
	}
}
