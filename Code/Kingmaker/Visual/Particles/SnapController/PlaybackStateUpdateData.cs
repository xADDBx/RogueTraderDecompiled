namespace Kingmaker.Visual.Particles.SnapController;

internal readonly struct PlaybackStateUpdateData
{
	public readonly bool particleSystemVisible;

	public readonly CameraData cameraData;

	public PlaybackStateUpdateData(bool particleSystemVisible, CameraData cameraData)
	{
		this.particleSystemVisible = particleSystemVisible;
		this.cameraData = cameraData;
	}
}
