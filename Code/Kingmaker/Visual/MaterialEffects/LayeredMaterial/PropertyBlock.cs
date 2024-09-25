using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal readonly struct PropertyBlock
{
	private readonly ScriptPropertiesSnapshot m_ScriptPropertiesSnapshot;

	private readonly MaterialPropertiesSnapshot m_MaterialPropertiesSnapshot;

	private readonly MaterialPropertyBlock m_MaterialPropertyBlock;

	public PropertyBlock(ScriptPropertiesSnapshot scriptPropertiesSnapshot, MaterialPropertiesSnapshot materialPropertiesSnapshot, MaterialPropertyBlock materialPropertyBlock)
	{
		m_ScriptPropertiesSnapshot = scriptPropertiesSnapshot;
		m_MaterialPropertiesSnapshot = materialPropertiesSnapshot;
		m_MaterialPropertyBlock = materialPropertyBlock;
	}

	public void SetFloat(PropertyIdentifier propertyId, float value)
	{
		m_MaterialPropertyBlock.SetFloat(propertyId.id, value);
	}

	public void SetColor(PropertyIdentifier propertyId, Color value)
	{
		m_MaterialPropertyBlock.SetColor(propertyId.id, value);
	}

	public void SetTexture(PropertyIdentifier propertyId, Texture value)
	{
		if (value != null)
		{
			m_MaterialPropertyBlock.SetTexture(propertyId.id, value);
		}
	}

	public void SetMatrix(PropertyIdentifier propertyId, in Matrix4x4 value)
	{
		m_MaterialPropertyBlock.SetMatrix(propertyId.id, value);
	}

	public void TransferFloatFromBaseMaterial(PropertyIdentifier srcPropertyId, PropertyIdentifier dstPropertyId)
	{
		if (m_MaterialPropertiesSnapshot.TryGetFloat(srcPropertyId.id, out var value))
		{
			m_MaterialPropertyBlock.SetFloat(dstPropertyId.id, value);
		}
	}

	public void TransferColorFromBaseMaterial(PropertyIdentifier srcPropertyId, PropertyIdentifier dstPropertyId)
	{
		if (m_MaterialPropertiesSnapshot.TryGetColor(srcPropertyId.id, out var value))
		{
			m_MaterialPropertyBlock.SetColor(dstPropertyId.id, value);
		}
	}

	public void TransferTextureFromBaseMaterial(PropertyIdentifier srcPropertyId, PropertyIdentifier dstPropertyId)
	{
		if (m_MaterialPropertiesSnapshot.TryGetTexture(srcPropertyId.id, out var value) && value != null)
		{
			m_MaterialPropertyBlock.SetTexture(dstPropertyId.id, value);
		}
	}

	public void TransferPropertyFromScript(ScriptProperty srcProperty, PropertyIdentifier dstPropertyId)
	{
		m_ScriptPropertiesSnapshot.TransferProperty(srcProperty, dstPropertyId.id, m_MaterialPropertyBlock);
	}
}
