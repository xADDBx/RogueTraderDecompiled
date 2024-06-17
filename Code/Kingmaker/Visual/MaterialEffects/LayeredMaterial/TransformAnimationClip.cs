using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class TransformAnimationClip : IAnimationClip
{
	private readonly PropertyIdentifier? m_WorldToLocalPropertyIdentifier;

	private readonly PropertyIdentifier? m_LocalToWorldPropertyIdentifier;

	private readonly Transform m_Transform;

	public TransformAnimationClip(string worldToLocalPropertyName, string localToWorldPropertyName, Transform transform)
	{
		m_WorldToLocalPropertyIdentifier = (string.IsNullOrWhiteSpace(worldToLocalPropertyName) ? null : new PropertyIdentifier?(new PropertyIdentifier(worldToLocalPropertyName)));
		m_LocalToWorldPropertyIdentifier = (string.IsNullOrWhiteSpace(localToWorldPropertyName) ? null : new PropertyIdentifier?(new PropertyIdentifier(localToWorldPropertyName)));
		m_Transform = transform;
	}

	public void Sample(in PropertyBlock properties, float time)
	{
		if (!(m_Transform == null))
		{
			if (m_WorldToLocalPropertyIdentifier.HasValue)
			{
				PropertyIdentifier value = m_WorldToLocalPropertyIdentifier.Value;
				Matrix4x4 value2 = m_Transform.worldToLocalMatrix;
				properties.SetMatrix(value, in value2);
			}
			if (m_LocalToWorldPropertyIdentifier.HasValue)
			{
				PropertyIdentifier value3 = m_LocalToWorldPropertyIdentifier.Value;
				Matrix4x4 value2 = m_Transform.localToWorldMatrix;
				properties.SetMatrix(value3, in value2);
			}
		}
	}

	void IAnimationClip.Sample(in PropertyBlock properties, float time)
	{
		Sample(in properties, time);
	}
}
