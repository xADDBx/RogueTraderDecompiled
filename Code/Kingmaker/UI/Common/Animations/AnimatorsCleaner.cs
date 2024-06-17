using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.UI.Common.Animations;

public class AnimatorsCleaner : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> m_GameObjectsToDisable = new List<GameObject>();

	[SerializeField]
	private List<Animator> m_AnimatorsToDisable = new List<Animator>();

	[SerializeField]
	private List<Animation> m_AnimationsToDisable = new List<Animation>();

	public void SetActiveState(bool state)
	{
		if (m_GameObjectsToDisable != null)
		{
			foreach (GameObject item in m_GameObjectsToDisable)
			{
				if ((bool)item)
				{
					item.SetActive(state);
				}
			}
		}
		SetComponentsListState(m_AnimatorsToDisable, state);
		SetComponentsListState(m_AnimationsToDisable, state);
	}

	private void SetComponentsListState<T>(List<T> components, bool state) where T : Behaviour
	{
		if (components == null)
		{
			return;
		}
		foreach (T component in components)
		{
			if (!(component == null))
			{
				component.enabled = state;
			}
		}
	}
}
