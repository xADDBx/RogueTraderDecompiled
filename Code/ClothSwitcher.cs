using UnityEngine;

public class ClothSwitcher : MonoBehaviour
{
	private void OnEnable()
	{
		if ((bool)GetComponent<Cloth>())
		{
			Cloth component = GetComponent<Cloth>();
			component.enabled = false;
			component.SetEnabledFading(enabled: true, 0.01f);
		}
	}
}
