using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddZWToUV0 : BaseMeshEffect
{
	[SerializeField]
	private List<Vector2> ZWValues = new List<Vector2>
	{
		Vector2.zero,
		Vector2.up,
		Vector2.one,
		Vector2.right
	};

	private UIVertex m_Vert;

	public override void ModifyMesh(VertexHelper vh)
	{
		if (IsActive())
		{
			for (int i = 0; i < vh.currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref m_Vert, i);
				m_Vert.uv0.z = ZWValues[i].x;
				m_Vert.uv0.w = ZWValues[i].y;
				vh.SetUIVertex(m_Vert, i);
			}
		}
	}
}
