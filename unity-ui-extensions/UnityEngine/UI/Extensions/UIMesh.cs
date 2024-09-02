using System.Collections.Generic;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Primitives/UI Mesh")]
[RequireComponent(typeof(CanvasRenderer))]
[ExecuteInEditMode]
public class UIMesh : MaskableGraphic
{
	[SerializeField]
	private Mesh m_mesh;

	[SerializeField]
	private List<Material> m_materials;

	public Mesh Mesh
	{
		get
		{
			return m_mesh;
		}
		set
		{
			if (!(m_mesh == value))
			{
				m_mesh = value;
				SetVerticesDirty();
				SetMaterialDirty();
			}
		}
	}

	public List<Material> Materials
	{
		get
		{
			return m_materials;
		}
		set
		{
			if (m_materials != value)
			{
				m_materials = value;
				SetVerticesDirty();
				SetMaterialDirty();
			}
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		Debug.Log("Populate Mesh Data");
		vh.Clear();
		vh.FillMesh(m_mesh);
		base.OnPopulateMesh(vh);
	}
}
