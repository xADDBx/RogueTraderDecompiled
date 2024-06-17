using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class ScriptPropertiesSnapshot
{
	private delegate void TransferFunc(ScriptPropertiesSnapshot snapshot, MaterialPropertyBlock properties, int dstPropertyId);

	private static TransferFunc[] s_TransferFuncMap;

	private readonly GameObject m_TargetGameObject;

	private readonly Transform m_TargetTransform;

	private float? m_ParticlesSnapMapSizeScale;

	private float? m_ParticlesSnapMapParticleSizeScale;

	private float? m_ParticlesSnapMapLifetimeScale;

	private float? m_ParticlesSnapMapRateOverTimeScale;

	private float? m_ParticlesSnapMapBurstScale;

	private Matrix4x4? m_TargetWorldToLocalMatrix;

	private Matrix4x4? m_TargetLocalToWorldMatrix;

	static ScriptPropertiesSnapshot()
	{
		s_TransferFuncMap = new TransferFunc[7];
		s_TransferFuncMap[0] = delegate(ScriptPropertiesSnapshot snapshot, MaterialPropertyBlock properties, int dstPropertyId)
		{
			TransferFloatProperty(properties, dstPropertyId, in snapshot.m_ParticlesSnapMapSizeScale);
		};
		s_TransferFuncMap[1] = delegate(ScriptPropertiesSnapshot snapshot, MaterialPropertyBlock properties, int dstPropertyId)
		{
			TransferFloatProperty(properties, dstPropertyId, in snapshot.m_ParticlesSnapMapParticleSizeScale);
		};
		s_TransferFuncMap[2] = delegate(ScriptPropertiesSnapshot snapshot, MaterialPropertyBlock properties, int dstPropertyId)
		{
			TransferFloatProperty(properties, dstPropertyId, in snapshot.m_ParticlesSnapMapLifetimeScale);
		};
		s_TransferFuncMap[3] = delegate(ScriptPropertiesSnapshot snapshot, MaterialPropertyBlock properties, int dstPropertyId)
		{
			TransferFloatProperty(properties, dstPropertyId, in snapshot.m_ParticlesSnapMapRateOverTimeScale);
		};
		s_TransferFuncMap[4] = delegate(ScriptPropertiesSnapshot snapshot, MaterialPropertyBlock properties, int dstPropertyId)
		{
			TransferFloatProperty(properties, dstPropertyId, in snapshot.m_ParticlesSnapMapBurstScale);
		};
		s_TransferFuncMap[5] = delegate(ScriptPropertiesSnapshot snapshot, MaterialPropertyBlock properties, int dstPropertyId)
		{
			TransferMatrixProperty(properties, dstPropertyId, in snapshot.m_TargetWorldToLocalMatrix);
		};
		s_TransferFuncMap[6] = delegate(ScriptPropertiesSnapshot snapshot, MaterialPropertyBlock properties, int dstPropertyId)
		{
			TransferMatrixProperty(properties, dstPropertyId, in snapshot.m_TargetLocalToWorldMatrix);
		};
	}

	private static void TransferFloatProperty(MaterialPropertyBlock properties, int dstPropertyId, in float? value)
	{
		if (value.HasValue)
		{
			properties.SetFloat(dstPropertyId, value.Value);
		}
	}

	private static void TransferMatrixProperty(MaterialPropertyBlock properties, int dstPropertyId, in Matrix4x4? value)
	{
		if (value.HasValue)
		{
			properties.SetMatrix(dstPropertyId, value.Value);
		}
	}

	public ScriptPropertiesSnapshot(GameObject targetGameObject)
	{
		m_TargetGameObject = targetGameObject;
		m_TargetTransform = targetGameObject.transform;
	}

	public void CaptureStaticProperties()
	{
		ParticlesSnapMap particlesSnapMap = ((m_TargetGameObject != null) ? m_TargetGameObject.GetComponentInChildren<ParticlesSnapMap>() : null);
		if (particlesSnapMap != null)
		{
			m_ParticlesSnapMapSizeScale = particlesSnapMap.SizeScale;
			m_ParticlesSnapMapParticleSizeScale = particlesSnapMap.ParticleSizeScale;
			m_ParticlesSnapMapLifetimeScale = particlesSnapMap.LifetimeScale;
			m_ParticlesSnapMapRateOverTimeScale = particlesSnapMap.RateOverTimeScale;
			m_ParticlesSnapMapBurstScale = particlesSnapMap.BurstScale;
		}
		else
		{
			m_ParticlesSnapMapSizeScale = null;
			m_ParticlesSnapMapParticleSizeScale = null;
			m_ParticlesSnapMapLifetimeScale = null;
			m_ParticlesSnapMapRateOverTimeScale = null;
			m_ParticlesSnapMapBurstScale = null;
		}
	}

	public void CaptureDynamicProperties()
	{
		if (m_TargetTransform != null)
		{
			m_TargetWorldToLocalMatrix = m_TargetTransform.worldToLocalMatrix;
			m_TargetLocalToWorldMatrix = m_TargetTransform.localToWorldMatrix;
		}
		else
		{
			m_TargetWorldToLocalMatrix = null;
			m_TargetLocalToWorldMatrix = null;
		}
	}

	public void Clear()
	{
		m_ParticlesSnapMapSizeScale = null;
		m_ParticlesSnapMapParticleSizeScale = null;
		m_ParticlesSnapMapLifetimeScale = null;
		m_ParticlesSnapMapRateOverTimeScale = null;
		m_ParticlesSnapMapBurstScale = null;
		m_TargetWorldToLocalMatrix = null;
		m_TargetLocalToWorldMatrix = null;
	}

	public void TransferProperty(ScriptProperty property, int dstPropertyId, MaterialPropertyBlock properties)
	{
		if (property >= ScriptProperty.ParticlesSnapMapSizeScale && (int)property < s_TransferFuncMap.Length)
		{
			s_TransferFuncMap[(int)property](this, properties, dstPropertyId);
		}
	}
}
