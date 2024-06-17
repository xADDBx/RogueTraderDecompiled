using UnityEngine;

namespace Kingmaker.Visual.Debug;

public class BoundingBoxRenderer : MonoBehaviour
{
	private void OnDrawGizmosSelected()
	{
		Renderer component = GetComponent<Renderer>();
		if (component != null)
		{
			Gizmos.DrawWireCube(component.bounds.center, component.bounds.size);
		}
	}
}
