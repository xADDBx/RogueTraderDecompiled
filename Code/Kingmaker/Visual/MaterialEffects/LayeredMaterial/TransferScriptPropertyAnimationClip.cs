namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class TransferScriptPropertyAnimationClip : IAnimationClip
{
	private readonly ScriptProperty m_SrcProperty;

	private readonly PropertyIdentifier m_DstPropertyIdentifier;

	public TransferScriptPropertyAnimationClip(ScriptProperty srcProperty, string dstPropertyName)
	{
		m_SrcProperty = srcProperty;
		m_DstPropertyIdentifier = new PropertyIdentifier(dstPropertyName);
	}

	public void Sample(in PropertyBlock properties, float time)
	{
		properties.TransferPropertyFromScript(m_SrcProperty, m_DstPropertyIdentifier);
	}

	void IAnimationClip.Sample(in PropertyBlock properties, float time)
	{
		Sample(in properties, time);
	}
}
