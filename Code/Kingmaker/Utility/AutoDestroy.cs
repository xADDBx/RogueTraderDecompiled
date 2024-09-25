using System.Collections;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using UnityEngine;

namespace Kingmaker.Utility;

public class AutoDestroy : MonoBehaviour, IDeactivatableComponent
{
	public float Lifetime = 1f;

	private void OnEnable()
	{
		StartCoroutine(DelayedDestroy());
	}

	private IEnumerator DelayedDestroy()
	{
		yield return null;
		yield return new WaitForSeconds(Lifetime);
		PooledGameObject componentInParent = GetComponentInParent<PooledGameObject>();
		if (componentInParent != null && componentInParent.gameObject != base.gameObject)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			FxCoreHelper.Destroy(base.gameObject);
		}
	}
}
