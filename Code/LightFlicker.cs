using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
	public float minIntensity = 0.4f;

	public float maxIntensity = 1.2f;

	public float flickerSpeed = 6f;

	private Light lightSource;

	private float seed;

	private void Start()
	{
		lightSource = GetComponent<Light>();
		seed = Random.Range(0f, 100f);
	}

	private void Update()
	{
		float t = Mathf.PerlinNoise(Time.time * flickerSpeed + seed, 0f);
		float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
		lightSource.intensity = intensity;
	}
}
