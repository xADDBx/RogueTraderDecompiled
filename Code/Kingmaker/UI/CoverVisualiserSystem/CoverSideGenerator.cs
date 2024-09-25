using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.UI.CoverVisualiserSystem;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
[ExecuteInEditMode]
public class CoverSideGenerator : MonoBehaviour
{
	public Side side;

	private Mesh m_VisualMesh;

	private MeshFilter m_MeshFilter;

	private MeshRenderer m_MeshRenderer;

	private List<Vector3> m_Verts;

	private List<Vector3> m_Normals;

	private List<Vector2> m_Uvs;

	private List<int> m_Tris;

	private const float gridSize = 1.35f;

	private void Awake()
	{
		m_VisualMesh = new Mesh();
		m_Verts = new List<Vector3>();
		m_Normals = new List<Vector3>();
		m_Uvs = new List<Vector2>();
		m_Tris = new List<int>();
		m_MeshFilter = GetComponent<MeshFilter>();
		m_MeshRenderer = GetComponent<MeshRenderer>();
		BuildSide();
	}

	private void OnDestroy()
	{
		ClearMaterial();
	}

	public void ChangeMaterial(Material material, Color color)
	{
		ClearMaterial();
		m_MeshRenderer.sharedMaterial = new Material(material)
		{
			color = color
		};
	}

	private void ClearMaterial()
	{
		Material sharedMaterial = m_MeshRenderer.sharedMaterial;
		if (sharedMaterial != null)
		{
			Object.DestroyImmediate(sharedMaterial);
		}
	}

	private void BuildSide()
	{
		switch (side)
		{
		case Side.Back:
			BuildPlaneMesh(new Vector3(-0.5f, 0f, -0.5f) * 1.35f, Vector3.up * 1.35f, Vector3.right * 1.35f, reversed: false);
			break;
		case Side.Bottom:
			BuildPlaneMesh(new Vector3(-0.5f, 0f, -0.5f) * 1.35f, Vector3.forward * 1.35f, Vector3.right * 1.35f, reversed: true);
			break;
		case Side.Top:
			BuildPlaneMesh(new Vector3(-0.5f, 1f, -0.5f) * 1.35f, Vector3.forward * 1.35f, Vector3.right * 1.35f, reversed: true);
			break;
		case Side.Front:
			BuildPlaneMesh(new Vector3(-0.5f, 0f, 0.5f) * 1.35f, Vector3.up * 1.35f, Vector3.right * 1.35f, reversed: true);
			break;
		case Side.Left:
			BuildPlaneMesh(new Vector3(-0.5f, 0f, -0.5f) * 1.35f, Vector3.up * 1.35f, Vector3.forward * 1.35f, reversed: true);
			break;
		case Side.Right:
			BuildPlaneMesh(new Vector3(0.5f, 0f, -0.5f) * 1.35f, Vector3.up * 1.35f, Vector3.forward * 1.35f, reversed: false);
			break;
		}
		m_VisualMesh.vertices = m_Verts.ToArray();
		m_VisualMesh.uv = m_Uvs.ToArray();
		m_VisualMesh.triangles = m_Tris.ToArray();
		m_VisualMesh.RecalculateBounds();
		m_VisualMesh.normals = m_Normals.ToArray();
		m_MeshFilter.mesh = m_VisualMesh;
	}

	private void BuildPlaneMesh(Vector3 corner, Vector3 up, Vector3 right, bool reversed)
	{
		int count = m_Verts.Count;
		m_Verts.Add(corner);
		m_Verts.Add(corner + up);
		m_Verts.Add(corner + up + right);
		m_Verts.Add(corner + right);
		_ = new Vector3[2] { up, right };
		Vector3 item = Vector3.Cross(up, right);
		for (int i = 0; i < 4; i++)
		{
			m_Normals.Add(item);
		}
		m_Uvs.Add(new Vector2(0f, 0f));
		m_Uvs.Add(new Vector2(0f, 1f));
		m_Uvs.Add(new Vector2(1f, 1f));
		m_Uvs.Add(new Vector2(1f, 0f));
		if (reversed)
		{
			m_Tris.Add(count);
			m_Tris.Add(count + 1);
			m_Tris.Add(count + 2);
			m_Tris.Add(count + 2);
			m_Tris.Add(count + 3);
			m_Tris.Add(count);
		}
		else
		{
			m_Tris.Add(count + 1);
			m_Tris.Add(count);
			m_Tris.Add(count + 2);
			m_Tris.Add(count + 3);
			m_Tris.Add(count + 2);
			m_Tris.Add(count);
		}
	}
}
