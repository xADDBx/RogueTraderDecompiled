using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker;

public class SplineVertexPaintPrefab : MonoBehaviour
{
	[Serializable]
	public class ListItems
	{
		public GameObject itemGo;

		public float itemFloat;
	}

	[Serializable]
	public class ListMeshes
	{
		public MeshFilter itemMeshFilter;

		public Mesh itemMesh;

		public MeshRenderer itemMeshRenderer;

		public Material[] itemMat;
	}

	[Serializable]
	public class ListOriginalMeshes
	{
		public MeshFilter itemMeshFilter;

		public Mesh itemMesh;

		public MeshRenderer itemMeshRenderer;

		public Material[] itemMatOrig;
	}

	public bool InvertMode;

	[SerializeField]
	public List<ListItems> PointsList = new List<ListItems>();

	[SerializeField]
	public List<ListOriginalMeshes> OriginalMeshesList = new List<ListOriginalMeshes>();

	[SerializeField]
	public List<ListMeshes> MeshesList = new List<ListMeshes>();

	public List<MeshFilter> SplinesList = new List<MeshFilter>();

	public bool SearchPointsParent;

	[HideInInspector]
	public bool CollectMeshes = true;

	private void Start()
	{
		setMeshFilter();
	}

	public void setMeshFilter()
	{
		foreach (ListMeshes meshes in MeshesList)
		{
			meshes.itemMeshFilter.sharedMesh = meshes.itemMesh;
			meshes.itemMeshRenderer.sharedMaterials = meshes.itemMat;
		}
		CollectMeshes = false;
	}
}
