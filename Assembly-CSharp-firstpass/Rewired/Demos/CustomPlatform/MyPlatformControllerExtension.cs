using Rewired.ControllerExtensions;
using Rewired.Interfaces;

namespace Rewired.Demos.CustomPlatform;

public sealed class MyPlatformControllerExtension : CustomControllerExtension, IControllerVibrator
{
	private class Source : IControllerExtensionSource
	{
		public readonly MyPlatformInputSource.Joystick sourceJoystick;

		public Source(MyPlatformInputSource.Joystick sourceJoystick)
		{
			this.sourceJoystick = sourceJoystick;
		}
	}

	public int vibrationMotorCount => 2;

	public MyPlatformControllerExtension(MyPlatformInputSource.Joystick sourceJoystick)
		: base(new Source(sourceJoystick))
	{
	}

	private MyPlatformControllerExtension(MyPlatformControllerExtension other)
		: base(other)
	{
	}

	public override Controller.Extension ShallowCopy()
	{
		return new MyPlatformControllerExtension(this);
	}

	public void SetVibration(int motorIndex, float motorLevel)
	{
		((Source)GetSource()).sourceJoystick.SetVibration(motorIndex, motorLevel);
	}

	public void SetVibration(int motorIndex, float motorLevel, float duration)
	{
		((Source)GetSource()).sourceJoystick.SetVibration(motorIndex, motorLevel, duration);
	}

	public void SetVibration(int motorIndex, float motorLevel, bool stopOtherMotors)
	{
		((Source)GetSource()).sourceJoystick.SetVibration(motorIndex, motorLevel, stopOtherMotors);
	}

	public void SetVibration(int motorIndex, float motorLevel, float duration, bool stopOtherMotors)
	{
		((Source)GetSource()).sourceJoystick.SetVibration(motorIndex, motorLevel, duration, stopOtherMotors);
	}

	public float GetVibration(int motorIndex)
	{
		return ((Source)GetSource()).sourceJoystick.GetVibration(motorIndex);
	}

	public void StopVibration()
	{
		((Source)GetSource()).sourceJoystick.StopVibration();
	}
}
