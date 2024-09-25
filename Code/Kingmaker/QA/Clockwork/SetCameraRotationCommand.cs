using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.QA.Arbiter.Attributes;

namespace Kingmaker.QA.Clockwork;

[ComponentName("ClockworkRules/MoveCommand")]
[TypeId("c7fdec0c5f3e482ca875c52bf4c2d3db")]
public class SetCameraRotationCommand : ClockworkCommand
{
	[CameraRotationButtons]
	public float Rotation;

	public float WaitTime = 5f;

	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		TaskRotateCamera taskRotateCamera = new TaskRotateCamera(runner, Rotation, WaitTime);
		taskRotateCamera.SetSourceCommand(this);
		return taskRotateCamera;
	}

	public override string GetCaption()
	{
		return $"{GetStatusString()}Rotate Camera to position {Rotation} and wait {WaitTime} seconds";
	}
}
