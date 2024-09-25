using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class PBDForceVolumeCylinder : PBDForceVolumeBase
{
	[Header("Volume")]
	public float Radius = 0.5f;

	public float Height = 2f;

	protected override ForceVolumeType GetVolumeType()
	{
		return ForceVolumeType.Cylinder;
	}

	protected override void UpdateForceVolume()
	{
		base.UpdateForceVolume();
		m_ForceVolume.VolumeParameters = new float4(Radius, Height, 0f, 0f);
	}
}
