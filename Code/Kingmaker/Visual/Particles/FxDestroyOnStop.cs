using System.Collections;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class FxDestroyOnStop : MonoBehaviour, IDeactivatableComponent
{
	public float Delay;

	public void Stop()
	{
		if (base.gameObject.activeInHierarchy && base.enabled)
		{
			StartCoroutine(DestroyCoroutine());
		}
	}

	private IEnumerator DestroyCoroutine()
	{
		if (Delay > 0f)
		{
			yield return new WaitForSeconds(Delay);
		}
		if (GetComponentInParent<PooledGameObject>() != null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
