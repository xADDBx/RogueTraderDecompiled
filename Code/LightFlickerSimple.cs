using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlickerSimple : MonoBehaviour
{
	[Header("Light Settings")]
	public float minIntensity = 0.4f;

	public float maxIntensity = 1.2f;

	public float flickerSpeed = 6f;

	[Header("Emissive Sync Settings")]
	public bool syncEmissive = true;

	public float maxEmissiveIntensity = 25f;

	[Header("Emissive Target IDs")]
	[Tooltip("Список ID — должны совпадать с flickerID на EmissiveFlickerReceiver в арт-сцене")]
	public string[] flickerIDs;

	private Light _light;

	private float _seed;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_seed = Random.Range(0f, 100f);
	}

	private void Update()
	{
		float num = Mathf.PerlinNoise(Time.time * flickerSpeed + _seed, 0f);
		_light.intensity = Mathf.Lerp(minIntensity, maxIntensity, num);
		if (syncEmissive && flickerIDs != null)
		{
			UpdateEmissive(num);
		}
	}

	private void UpdateEmissive(float normalizedIntensity)
	{
		float emissiveIntensity = normalizedIntensity * maxEmissiveIntensity;
		string[] array = flickerIDs;
		for (int i = 0; i < array.Length; i++)
		{
			List<EmissiveFlickerReceiver> list = EmissiveFlickerRegistry.Get(array[i]);
			if (list == null)
			{
				continue;
			}
			foreach (EmissiveFlickerReceiver item in list)
			{
				item.SetEmissiveIntensity(emissiveIntensity);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (flickerIDs == null)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		string[] array = flickerIDs;
		for (int i = 0; i < array.Length; i++)
		{
			List<EmissiveFlickerReceiver> list = EmissiveFlickerRegistry.Get(array[i]);
			if (list == null)
			{
				continue;
			}
			foreach (EmissiveFlickerReceiver item in list)
			{
				if (item != null)
				{
					Gizmos.DrawLine(base.transform.position, item.transform.position);
				}
			}
		}
	}
}
