using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker;

public class CharacterStudioLightButtonTemplate : MonoBehaviour
{
	private int index;

	public Button button;

	public void LinkButton(int index, CharacterStudioLightManager manager)
	{
		this.index = index;
		base.gameObject.SetActive(value: true);
		button.onClick.AddListener(delegate
		{
			manager.SelectPreset(this.index);
		});
	}
}
