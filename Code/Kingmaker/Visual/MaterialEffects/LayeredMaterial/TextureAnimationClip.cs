using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class TextureAnimationClip : IAnimationClip
{
	private readonly PropertyIdentifier m_PropertyIdentifier;

	private readonly Texture m_Texture;

	public TextureAnimationClip(string propertyName, Texture texture)
	{
		m_PropertyIdentifier = new PropertyIdentifier(propertyName);
		m_Texture = texture;
	}

	public void Sample(in PropertyBlock properties, float time)
	{
		properties.SetTexture(m_PropertyIdentifier, m_Texture);
	}

	void IAnimationClip.Sample(in PropertyBlock properties, float time)
	{
		Sample(in properties, time);
	}
}
