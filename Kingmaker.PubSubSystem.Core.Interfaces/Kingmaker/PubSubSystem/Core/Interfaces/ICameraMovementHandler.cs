namespace Kingmaker.PubSubSystem.Core.Interfaces;

public interface ICameraMovementHandler : ISubscriber
{
	void HandleCameraRotated(float angle);

	void HandleCameraTransformed(float distance);
}
