namespace Kingmaker.Visual.Particles.SnapController;

internal readonly struct AnimationSampler
{
	private readonly OffsetAnimationSampler m_OffsetAnimationSampler;

	private readonly CameraOffsetScaleAnimationSampler m_CameraOffsetScaleAnimationSampler;

	public AnimationSampler(OffsetAnimationSampler offsetAnimationSampler, CameraOffsetScaleAnimationSampler cameraOffsetScaleAnimationSampler)
	{
		m_OffsetAnimationSampler = offsetAnimationSampler;
		m_CameraOffsetScaleAnimationSampler = cameraOffsetScaleAnimationSampler;
	}

	public AnimationSample Sample(float particleSystemTime)
	{
		return new AnimationSample(m_OffsetAnimationSampler.Sample(particleSystemTime), m_CameraOffsetScaleAnimationSampler.Sample(particleSystemTime));
	}
}
