using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

internal sealed class ShadowSettingsOverride : MonoBehaviour, OverridableValue<float>.ISource
{
	private WaaaghPipelineAsset m_Asset;

	[SerializeField]
	private float m_ShadowDistance = 1000f;

	[SerializeField]
	private int m_OverridePriority = 1000;

	float OverridableValue<float>.ISource.Value => m_ShadowDistance;

	private void OnEnable()
	{
		m_Asset = WaaaghPipeline.Asset;
		if (m_Asset != null)
		{
			m_Asset.ShadowSettings.ShadowDistance.AddOverride(this, m_OverridePriority);
		}
	}

	private void OnDisable()
	{
		if (m_Asset != null)
		{
			m_Asset.ShadowSettings.ShadowDistance.RemoveOverride(this);
			m_Asset = null;
		}
	}

	private void OnValidate()
	{
		if (m_Asset != null)
		{
			m_Asset.ShadowSettings.ShadowDistance.RemoveOverride(this);
			m_Asset.ShadowSettings.ShadowDistance.AddOverride(this, m_OverridePriority);
		}
	}
}
