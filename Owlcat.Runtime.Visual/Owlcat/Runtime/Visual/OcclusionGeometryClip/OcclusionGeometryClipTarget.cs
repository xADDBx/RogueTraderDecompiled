using System;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

public sealed class OcclusionGeometryClipTarget : MonoBehaviour, ITargetInfoProvider
{
	public enum ActivationModeType
	{
		Manual,
		Auto
	}

	[SerializeField]
	internal ActivationModeType m_ActivationMode;

	[SerializeField]
	internal Vector3 m_PositionLocalOffset = new Vector3(0f, 1f, 0f);

	[SerializeField]
	internal Vector3 m_PositionWorldOffset;

	[SerializeField]
	internal Vector2 m_Size;

	[SerializeField]
	internal Vector2 m_DynamicSize;

	[SerializeField]
	internal TargetInsideBoxOcclusionMode m_TargetInsideBoxOcclusionMode;

	private Transform m_CachedTransform;

	private bool m_Enabled;

	private bool m_ClippingEnabledManually;

	private bool m_Registered;

	public ActivationModeType ActivationMode
	{
		get
		{
			return m_ActivationMode;
		}
		set
		{
			if (m_ActivationMode != value)
			{
				m_ActivationMode = value;
				UpdateRegistration();
			}
		}
	}

	public bool ClippingEnabled
	{
		get
		{
			return m_ClippingEnabledManually;
		}
		set
		{
			if (m_ClippingEnabledManually != value)
			{
				m_ClippingEnabledManually = value;
				UpdateRegistration();
			}
		}
	}

	TargetInfo ITargetInfoProvider.TargetInfo
	{
		get
		{
			TargetInfo result = default(TargetInfo);
			if (math.lengthsq(m_PositionLocalOffset) < float.Epsilon)
			{
				result.position = m_CachedTransform.position + m_PositionWorldOffset;
			}
			else
			{
				result.position = m_CachedTransform.TransformPoint(m_PositionLocalOffset) + m_PositionWorldOffset;
			}
			result.size = m_Size;
			result.dynamicSize = m_DynamicSize;
			result.targetInsideBoxOcclusionMode = m_TargetInsideBoxOcclusionMode;
			return result;
		}
	}

	[UsedImplicitly]
	private void OnValidate()
	{
		m_DynamicSize = math.max(m_DynamicSize, default(float2));
		m_Size = math.max(m_DynamicSize, m_Size);
		if (!Enum.IsDefined(typeof(ActivationModeType), m_ActivationMode))
		{
			m_ActivationMode = ActivationModeType.Manual;
		}
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		m_CachedTransform = base.transform;
		m_Enabled = true;
		UpdateRegistration();
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		m_Enabled = false;
		UpdateRegistration();
	}

	private void UpdateRegistration()
	{
		bool flag = EvaluateDesiredClippingStatus();
		if (m_Registered != flag)
		{
			if (flag)
			{
				System.RegisterTarget(this);
			}
			else
			{
				System.UnregisterTarget(this);
			}
			m_Registered = flag;
		}
	}

	private bool EvaluateDesiredClippingStatus()
	{
		switch (m_ActivationMode)
		{
		case ActivationModeType.Manual:
			if (base.gameObject.activeSelf && m_Enabled)
			{
				return m_ClippingEnabledManually;
			}
			return false;
		case ActivationModeType.Auto:
			if (base.gameObject.activeSelf)
			{
				return m_Enabled;
			}
			return false;
		default:
			return false;
		}
	}
}
