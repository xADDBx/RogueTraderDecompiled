using System;
using UnityEngine;

namespace Kingmaker.Visual.Lighting;

[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
public class LightHalo : MonoBehaviour
{
	private Mesh m_QuadMesh;

	private Color[] m_Colors = new Color[4];

	public Color Color = Color.white;

	public float Size;

	public Material Material;

	private void Start()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCamera));
		CreateQuadMesh();
	}

	private void OnDestroy()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnPostRenderCamera));
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(m_QuadMesh);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(m_QuadMesh);
		}
	}

	private void OnPostRenderCamera(Camera cam)
	{
		Vector3 normalized = (cam.transform.position - base.transform.position).normalized;
		Vector3 up = cam.transform.up;
		normalized = Vector3.Cross(Vector3.Cross(up, normalized), up);
		Quaternion q = Quaternion.LookRotation(normalized, up);
		Matrix4x4 matrix = Matrix4x4.TRS(base.transform.position, q, Vector3.one * Size);
		for (int i = 0; i < m_Colors.Length; i++)
		{
			m_Colors[i] = Color;
		}
		m_QuadMesh.colors = m_Colors;
		if (Material != null)
		{
			Material.SetPass(0);
		}
		Graphics.DrawMeshNow(m_QuadMesh, matrix);
	}

	private void CreateQuadMesh()
	{
		m_QuadMesh = new Mesh
		{
			name = "LightHalo"
		};
		m_QuadMesh.MarkDynamic();
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, -1f, -0f),
			new Vector3(-1f, 1f, 0f),
			new Vector3(-1f, -1f, -0f)
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(1f, 1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(0f, 0f)
		};
		int[] triangles = new int[6] { 1, 0, 2, 1, 2, 3 };
		m_QuadMesh.vertices = vertices;
		m_QuadMesh.uv = uv;
		m_QuadMesh.triangles = triangles;
	}

	private void OnValidate()
	{
		Size = Mathf.Max(0.001f, Size);
	}
}
