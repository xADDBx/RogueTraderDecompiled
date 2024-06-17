using Owlcat.Runtime.Visual.Terrain;
using UnityEngine;

namespace Kingmaker.Visual;

public class BakedTerrain : MonoBehaviour
{
	public Terrain Terrain;

	public MeshRenderer[] Meshes;

	[HideInInspector]
	public long BakeTime;

	[HideInInspector]
	public int TerrainNamePrefix = -1;

	private void OnEnable()
	{
		MeshRenderer[] meshes;
		if (Meshes.Length == 0 || !Meshes[0] || Application.isEditor)
		{
			Object.Destroy(this);
			meshes = Meshes;
			for (int i = 0; i < meshes.Length; i++)
			{
				meshes[i].gameObject.SetActive(value: false);
			}
			Terrain.enabled = true;
			return;
		}
		OwlcatTerrain component = GetComponent<OwlcatTerrain>();
		component.Init();
		meshes = Meshes;
		foreach (MeshRenderer obj in meshes)
		{
			obj.gameObject.SetActive(value: true);
			obj.sharedMaterial = Terrain.materialTemplate;
			obj.SetPropertyBlock(component.MaterialPropertyBlock);
			obj.GetComponent<MeshFilter>().sharedMesh.UploadMeshData(markNoLongerReadable: true);
		}
		Terrain.enabled = false;
	}
}
