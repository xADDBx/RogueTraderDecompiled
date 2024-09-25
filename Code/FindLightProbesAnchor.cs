using UnityEngine;

public class FindLightProbesAnchor : MonoBehaviour
{
	private const string ANCHOR_NAME = "AmbientFxAnchor";

	private void Start()
	{
		ParticleSystem component = GetComponent<ParticleSystem>();
		if ((bool)component)
		{
			GameObject gameObject = GameObject.Find("AmbientFxAnchor");
			if ((bool)gameObject)
			{
				component.GetComponent<ParticleSystemRenderer>().probeAnchor = gameObject.transform;
			}
		}
	}
}
