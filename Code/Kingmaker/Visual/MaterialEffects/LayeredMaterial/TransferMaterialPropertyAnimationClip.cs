using System;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class TransferMaterialPropertyAnimationClip : IAnimationClip
{
	private delegate void TransferFunc(in PropertyBlock properties, in PropertyIdentifier src, in PropertyIdentifier dst);

	private static readonly TransferFunc s_FloatTransferFunc = delegate(in PropertyBlock properties, in PropertyIdentifier src, in PropertyIdentifier dst)
	{
		properties.TransferFloatFromBaseMaterial(src, dst);
	};

	private static readonly TransferFunc s_ColorTransferFunc = delegate(in PropertyBlock properties, in PropertyIdentifier src, in PropertyIdentifier dst)
	{
		properties.TransferColorFromBaseMaterial(src, dst);
	};

	private static readonly TransferFunc s_TextureTransferFunc = delegate(in PropertyBlock properties, in PropertyIdentifier src, in PropertyIdentifier dst)
	{
		properties.TransferTextureFromBaseMaterial(src, dst);
	};

	private readonly PropertyIdentifier m_SrcPropertyIdentifier;

	private readonly PropertyIdentifier m_DstPropertyIdentifier;

	private readonly TransferFunc m_TransferFunc;

	public TransferMaterialPropertyAnimationClip(string srcPropertyName, string dstPropertyName, MaterialPropertyType propertyType)
	{
		m_SrcPropertyIdentifier = new PropertyIdentifier(srcPropertyName);
		m_DstPropertyIdentifier = new PropertyIdentifier(dstPropertyName);
		switch (propertyType)
		{
		case MaterialPropertyType.Float:
			m_TransferFunc = s_FloatTransferFunc;
			break;
		case MaterialPropertyType.Color:
			m_TransferFunc = s_ColorTransferFunc;
			break;
		case MaterialPropertyType.Texture:
			m_TransferFunc = s_TextureTransferFunc;
			break;
		default:
			throw new ArgumentOutOfRangeException("propertyType", propertyType, null);
		}
	}

	public void Sample(in PropertyBlock properties, float time)
	{
		m_TransferFunc(in properties, in m_SrcPropertyIdentifier, in m_DstPropertyIdentifier);
	}

	void IAnimationClip.Sample(in PropertyBlock properties, float time)
	{
		Sample(in properties, time);
	}
}
