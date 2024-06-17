using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class PBDForceVolumeSphere : PBDForceVolumeBase
{
	[Header("Volume")]
	public float3 Position;

	public float Radius = 0.5f;

	protected override ForceVolumeType GetVolumeType()
	{
		return ForceVolumeType.Sphere;
	}

	protected override void UpdateForceVolume()
	{
		base.UpdateForceVolume();
		m_ForceVolume.VolumeParameters = new float4(Position, Radius);
	}
}
