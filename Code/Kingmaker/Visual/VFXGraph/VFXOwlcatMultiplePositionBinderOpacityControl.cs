using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using UnityEngine;

namespace Kingmaker.Visual.VFXGraph;

public class VFXOwlcatMultiplePositionBinderOpacityControl : MonoBehaviour, IRendererProxy
{
	public VFXOwlcatMultiplePositionBinder m_Binder;

	public int m_IndexBegin;

	public int m_IndexEnd;

	private bool m_ComponentEnabled;

	private float m_Value = 1f;

	private void OnEnable()
	{
		m_ComponentEnabled = true;
		if (m_Binder != null)
		{
			m_Binder.SetOpacity(m_IndexBegin, m_IndexEnd, m_Value);
		}
	}

	private void OnDisable()
	{
		m_ComponentEnabled = false;
		if (m_Binder != null)
		{
			m_Binder.SetOpacity(m_IndexBegin, m_IndexEnd, 0f);
		}
	}

	public void SetOpacity(float value)
	{
		m_Value = value;
		m_Binder.SetOpacity(m_IndexBegin, m_IndexEnd, m_ComponentEnabled ? value : 0f);
	}
}
