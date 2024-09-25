using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public abstract class PBDForceVolumeBase : MonoBehaviour
{
	private int m_FrameId;

	protected ForceVolume m_ForceVolume = new ForceVolume();

	private Vector3 m_PrevPosition;

	public Transform Target;

	public ForceEmissionType EmissionType = ForceEmissionType.Directional;

	public Transform Emitter;

	[Header("Emission")]
	public AxisDirection Axis;

	public DirectionType DirectionType;

	public float3 Direction;

	public float Intensity;

	[Range(0f, 1f)]
	public float DirectionLerp;

	public AnimationCurve IntensityOverSpeedMultiplier = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	protected abstract ForceVolumeType GetVolumeType();

	private void OnEnable()
	{
		m_PrevPosition = base.transform.position;
		PBD.RegisterForceVolume(m_ForceVolume);
	}

	private void OnDisable()
	{
		PBD.UnregisterForceVolume(m_ForceVolume);
	}

	private void Update()
	{
		if (Target != null)
		{
			base.transform.position = Target.position;
			base.transform.rotation = Target.rotation;
		}
		UpdateForceVolume();
	}

	protected virtual void UpdateForceVolume()
	{
		if (m_FrameId == Time.frameCount)
		{
			return;
		}
		m_ForceVolume.LocalToWorldVolume = base.transform.localToWorldMatrix;
		if (Emitter == null)
		{
			m_ForceVolume.LocalToWorldEmitter = m_ForceVolume.LocalToWorldVolume;
		}
		else
		{
			m_ForceVolume.LocalToWorldEmitter = Emitter.localToWorldMatrix;
		}
		m_ForceVolume.Axis = Axis;
		m_ForceVolume.DirectionType = DirectionType;
		m_ForceVolume.EmissionType = EmissionType;
		m_ForceVolume.VolumeType = GetVolumeType();
		m_ForceVolume.Intensity = Intensity;
		m_ForceVolume.DirectionLerp = DirectionLerp;
		switch (DirectionType)
		{
		case DirectionType.EmitterDirection:
			if (Emitter == null)
			{
				m_ForceVolume.Direction = base.transform.forward;
			}
			else
			{
				m_ForceVolume.Direction = Emitter.forward;
			}
			break;
		case DirectionType.MovementDirection:
		{
			m_ForceVolume.Direction = base.transform.position - m_PrevPosition;
			float time = math.length(m_ForceVolume.Direction);
			m_ForceVolume.Intensity *= IntensityOverSpeedMultiplier.Evaluate(time);
			if (m_ForceVolume.Intensity > 0f)
			{
				m_ForceVolume.Direction = math.normalize(m_ForceVolume.Direction);
			}
			else if (Emitter == null)
			{
				m_ForceVolume.Direction = base.transform.forward;
			}
			else
			{
				m_ForceVolume.Direction = Emitter.forward;
			}
			break;
		}
		case DirectionType.Custom:
			m_ForceVolume.Direction = math.normalize(Direction);
			break;
		}
		m_PrevPosition = base.transform.position;
		m_FrameId = Time.frameCount;
	}
}
