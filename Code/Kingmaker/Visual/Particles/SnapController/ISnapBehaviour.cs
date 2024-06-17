namespace Kingmaker.Visual.Particles.SnapController;

internal interface ISnapBehaviour
{
	void Setup();

	void Update(in CameraData cameraData, in AnimationSample animationSample);

	void OnParticleUpdateJobScheduled();

	void Recycle();
}
