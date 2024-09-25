using UnityEngine;

namespace Kingmaker.Visual;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class CurvedMeshRenderer : MonoBehaviour
{
	[SerializeField]
	private MeshFilter m_MeshFilter;

	[SerializeField]
	private MeshRenderer m_MeshRenderer;

	private LineRenderer m_LineRenderer;

	private Mesh m_Mesh;

	private void OnEnable()
	{
		m_LineRenderer = GetComponent<LineRenderer>();
		m_Mesh = new Mesh();
		m_MeshFilter.mesh = m_Mesh;
		UpdateMesh();
	}

	public void Update()
	{
	}

	private void UpdateMesh()
	{
		if (!(m_MeshFilter == null) && !(m_LineRenderer == null) && !(m_MeshRenderer == null))
		{
			Vector3[] array = new Vector3[m_LineRenderer.positionCount];
			m_LineRenderer.GetPositions(array);
			int num = array.Length;
			Vector3[] array2 = new Vector3[num + 1];
			array2[0] = m_LineRenderer.bounds.center;
			int[] array3 = new int[num * 3];
			Vector2[] array4 = new Vector2[array2.Length];
			int num2 = 0;
			int num3 = 1;
			while (num2 < array.Length)
			{
				array2[num2] = base.transform.InverseTransformPoint(array[num2]);
				array3[num3] = num2;
				array3[num3 + 1] = num2 + 1;
				num2++;
				num3 += 3;
			}
			array3[^1] = 1;
			m_Mesh.name = "CurvedMesh";
			m_Mesh.vertices = array2;
			m_Mesh.triangles = array3;
			for (int i = 0; i < array4.Length; i++)
			{
				array4[i] = new Vector2(array2[i].x, array2[i].y);
			}
			m_Mesh.uv = array4;
			m_Mesh.Optimize();
			m_Mesh.RecalculateNormals();
			m_Mesh.RecalculateBounds();
		}
	}
}
