using UnityEngine;

namespace Kingmaker.Visual.Particles.SnapController;

internal readonly struct AnimationSample
{
	public readonly Vector3 offset;

	public readonly float cameraOffsetScale;

	public AnimationSample(Vector3 offset, float cameraOffsetScale)
	{
		this.offset = offset;
		this.cameraOffsetScale = cameraOffsetScale;
	}
}
