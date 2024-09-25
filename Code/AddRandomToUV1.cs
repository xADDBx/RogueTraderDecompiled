using System;
using UnityEngine;
using UnityEngine.UI;

public class AddRandomToUV1 : BaseMeshEffect
{
	private enum Channel
	{
		U,
		V,
		Z,
		W
	}

	[SerializeField]
	private Channel m_Channel = Channel.Z;

	[SerializeField]
	private bool m_RandomizeOnEnableOnly = true;

	[SerializeField]
	private bool m_SetValue;

	[SerializeField]
	private float m_SeedValue = 0.7f;

	private float m_Seed;

	private UIVertex m_Vert;

	protected override void OnEnable()
	{
		m_Seed = m_SeedValue;
		if (!m_SetValue && m_RandomizeOnEnableOnly)
		{
			m_Seed = UnityEngine.Random.value;
		}
		base.graphic.canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (!IsActive())
		{
			return;
		}
		if (!m_SetValue && !m_RandomizeOnEnableOnly)
		{
			m_Seed = UnityEngine.Random.value;
		}
		for (int i = 0; i < vh.currentVertCount; i++)
		{
			vh.PopulateUIVertex(ref m_Vert, i);
			switch (m_Channel)
			{
			case Channel.U:
				m_Vert.uv1.x = m_Seed;
				break;
			case Channel.V:
				m_Vert.uv1.y = m_Seed;
				break;
			case Channel.Z:
				m_Vert.uv1.z = m_Seed;
				break;
			case Channel.W:
				m_Vert.uv1.w = m_Seed;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			vh.SetUIVertex(m_Vert, i);
		}
	}
}
