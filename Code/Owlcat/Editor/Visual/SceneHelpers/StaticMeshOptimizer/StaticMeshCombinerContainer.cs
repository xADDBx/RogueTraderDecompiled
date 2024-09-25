using UnityEngine;

namespace Owlcat.Editor.Visual.SceneHelpers.StaticMeshOptimizer;

[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class StaticMeshCombinerContainer : MonoBehaviour
{
	public BoxCollider BoxColliderContainer;

	private void Start()
	{
		if ((bool)BoxColliderContainer)
		{
			BoxColliderContainer.enabled = false;
		}
	}
}
