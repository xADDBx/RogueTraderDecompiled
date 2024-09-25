using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.InputSystems;

public class SetInitialFocus : MonoBehaviour
{
	public GameObject initialOption;

	private void Start()
	{
		EventSystem.current.SetSelectedGameObject(initialOption);
	}

	private void Update()
	{
	}
}
