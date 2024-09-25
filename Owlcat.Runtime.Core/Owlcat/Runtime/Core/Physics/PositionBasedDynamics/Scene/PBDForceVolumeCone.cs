using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class PBDForceVolumeCone : PBDForceVolumeBase
{
	[Header("Volume")]
	public float Radius = 0.5f;

	[Range(1f, 179f)]
	public float Angle = 30f;

	protected override ForceVolumeType GetVolumeType()
	{
		return ForceVolumeType.Cone;
	}

	protected override void UpdateForceVolume()
	{
		base.UpdateForceVolume();
		m_ForceVolume.VolumeParameters = new float4(Radius, Angle, 0f, 0f);
	}
}
