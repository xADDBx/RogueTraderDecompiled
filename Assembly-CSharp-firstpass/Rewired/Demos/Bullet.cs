using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class Bullet : MonoBehaviour
{
	public float lifeTime = 3f;

	private bool die;

	private float deathTime;

	private void Start()
	{
		if (lifeTime > 0f)
		{
			deathTime = Time.time + lifeTime;
			die = true;
		}
	}

	private void Update()
	{
		if (die && Time.time >= deathTime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
