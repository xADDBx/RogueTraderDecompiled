using UnityEngine;

namespace Kingmaker.Visual.Particles.SnapController;

internal readonly struct CameraData
{
	public readonly Vector3 position;

	public CameraData(Vector3 position)
	{
		this.position = position;
	}
}
