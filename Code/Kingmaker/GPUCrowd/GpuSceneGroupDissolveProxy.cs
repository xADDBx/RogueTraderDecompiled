using System.Collections.Generic;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Kingmaker.GPUCrowd;

public class GpuSceneGroupDissolveProxy : MonoBehaviour, IRendererProxy
{
	[SerializeField]
	private ExposedProperty m_OpacityProperty = "OcclusionGeometryClipOpacity";

	[SerializeField]
	private List<VisualEffect> m_LinkedEffects = new List<VisualEffect>();

	public List<VisualEffect> LinkedEffects => m_LinkedEffects;

	public void SetOpacity(float value)
	{
		foreach (VisualEffect linkedEffect in m_LinkedEffects)
		{
			if (linkedEffect != null && linkedEffect.HasFloat(m_OpacityProperty))
			{
				linkedEffect.SetFloat(m_OpacityProperty, value);
			}
		}
	}
}
