using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class LightDependencyController : MonoBehaviour
{
	public List<GameObject> dependentObjects = new List<GameObject>();

	private Light m_Light;

	private void Start()
	{
		m_Light = GetComponent<Light>();
		SwitchActive();
	}

	private void Awake()
	{
		SwitchActive();
	}

	public void SwitchActive()
	{
		if (m_Light == null)
		{
			m_Light = GetComponent<Light>();
		}
		if (dependentObjects == null || dependentObjects.Count <= 0)
		{
			return;
		}
		foreach (GameObject dependentObject in dependentObjects)
		{
			if (dependentObject != null)
			{
				if (dependentObject.GetComponent<Light>() != null)
				{
					dependentObject.GetComponent<Light>().enabled = m_Light.enabled;
				}
				else
				{
					dependentObject.SetActive(m_Light.enabled);
				}
			}
		}
	}
}
