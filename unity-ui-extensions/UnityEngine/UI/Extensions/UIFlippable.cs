namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(RectTransform), typeof(Graphic))]
[DisallowMultipleComponent]
[AddComponentMenu("UI/Effects/Extensions/Flippable")]
public class UIFlippable : MonoBehaviour, IMeshModifier
{
	[SerializeField]
	private bool m_Horizontal;

	[SerializeField]
	private bool m_Veritical;

	public bool horizontal
	{
		get
		{
			return m_Horizontal;
		}
		set
		{
			m_Horizontal = value;
		}
	}

	public bool vertical
	{
		get
		{
			return m_Veritical;
		}
		set
		{
			m_Veritical = value;
		}
	}

	protected void OnValidate()
	{
		GetComponent<Graphic>().SetVerticesDirty();
	}

	public void ModifyMesh(VertexHelper verts)
	{
		RectTransform rectTransform = base.transform as RectTransform;
		for (int i = 0; i < verts.currentVertCount; i++)
		{
			UIVertex vertex = default(UIVertex);
			verts.PopulateUIVertex(ref vertex, i);
			vertex.position = new Vector3(m_Horizontal ? (vertex.position.x + (rectTransform.rect.center.x - vertex.position.x) * 2f) : vertex.position.x, m_Veritical ? (vertex.position.y + (rectTransform.rect.center.y - vertex.position.y) * 2f) : vertex.position.y, vertex.position.z);
			verts.SetUIVertex(vertex, i);
		}
	}

	public void ModifyMesh(Mesh mesh)
	{
	}
}
