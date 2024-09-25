using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Utilities;

public static class RendererExtensions
{
	public static int GetExpectedMaterialsCount(this Renderer renderer)
	{
		if (renderer is MeshRenderer)
		{
			if (renderer.TryGetComponent<MeshFilter>(out var component))
			{
				Mesh sharedMesh = component.sharedMesh;
				if (sharedMesh != null)
				{
					return sharedMesh.subMeshCount;
				}
			}
			return 0;
		}
		if (renderer is SkinnedMeshRenderer { sharedMesh: var sharedMesh2 })
		{
			if (sharedMesh2 != null)
			{
				return sharedMesh2.subMeshCount;
			}
			return 0;
		}
		return -1;
	}
}
