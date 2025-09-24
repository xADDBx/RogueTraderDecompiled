using System;
using System.Collections.Generic;
using Rewired.Interfaces;
using Rewired.Platforms.Custom;

namespace Rewired.Demos.CustomPlatform;

public sealed class MyPlatformInputSource : CustomPlatformInputSource
{
	public new sealed class Joystick : CustomPlatformInputSource.Joystick, IControllerVibrator
	{
		private UnityInputJoystickSource.Joystick _sourceJoystick;

		public UnityInputJoystickSource.Joystick sourceJoystick => _sourceJoystick;

		public int vibrationMotorCount => _sourceJoystick.vibrationMotorCount;

		public Joystick(UnityInputJoystickSource.Joystick sourceJoystick)
			: base(sourceJoystick.deviceName, sourceJoystick.systemId, sourceJoystick.axisCount, sourceJoystick.buttonCount)
		{
			if (sourceJoystick == null)
			{
				throw new ArgumentNullException("sourceJoystick");
			}
			_sourceJoystick = sourceJoystick;
			base.customIdentifier = _sourceJoystick.identifier;
			base.deviceInstanceGuid = sourceJoystick.deviceInstanceGuid;
		}

		public override void Update()
		{
			for (int i = 0; i < base.buttonCount; i++)
			{
				SetButtonValue(i, sourceJoystick.GetButtonValue(i));
			}
			for (int j = 0; j < base.axisCount; j++)
			{
				SetAxisValue(j, sourceJoystick.GetAxisValue(j));
			}
		}

		public void SetVibration(int motorIndex, float motorLevel)
		{
			_sourceJoystick.SetVibration(motorIndex, motorLevel);
		}

		public void SetVibration(int motorIndex, float motorLevel, float duration)
		{
			_sourceJoystick.SetVibration(motorIndex, motorLevel, duration);
		}

		public void SetVibration(int motorIndex, float motorLevel, bool stopOtherMotors)
		{
			_sourceJoystick.SetVibration(motorIndex, motorLevel, stopOtherMotors);
		}

		public void SetVibration(int motorIndex, float motorLevel, float duration, bool stopOtherMotors)
		{
			_sourceJoystick.SetVibration(motorIndex, motorLevel, duration, stopOtherMotors);
		}

		public float GetVibration(int motorIndex)
		{
			return _sourceJoystick.GetVibration(motorIndex);
		}

		public void StopVibration()
		{
			_sourceJoystick.StopVibration();
		}
	}

	private UnityInputJoystickSource _joystickInputSource = new UnityInputJoystickSource();

	private bool _initialized;

	private bool _disposed;

	public override bool isReady => _initialized;

	public MyPlatformInputSource(CustomPlatformConfigVars configVars)
		: base(configVars, new InitOptions
		{
			unifiedKeyboardSource = new MyPlatformUnifiedKeyboardSource(),
			unifiedMouseSource = new MyPlatformUnifiedMouseSource()
		})
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		_initialized = true;
		MonitorDeviceChanges();
	}

	public override void Update()
	{
		_joystickInputSource.Update();
		MonitorDeviceChanges();
	}

	private void MonitorDeviceChanges()
	{
		IList<CustomInputSource.Joystick> joysticks = GetJoysticks();
		IList<UnityInputJoystickSource.Joystick> joysticks2 = _joystickInputSource.GetJoysticks();
		for (int num = joysticks.Count - 1; num >= 0; num--)
		{
			Joystick joystick = joysticks[num] as Joystick;
			if (!ContainsSystemJoystickBySystemId(joysticks2, joystick.sourceJoystick.systemId))
			{
				RemoveJoystick(joystick);
			}
		}
		for (int i = 0; i < joysticks2.Count; i++)
		{
			UnityInputJoystickSource.Joystick joystick2 = joysticks2[i];
			if (!ContainsJoystickBySystemId(joysticks, joystick2.systemId))
			{
				Joystick joystick3 = new Joystick(joystick2);
				if (joystick2.vibrationMotorCount > 0)
				{
					joystick3.extension = new MyPlatformControllerExtension(joystick3);
				}
				AddJoystick(joystick3);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			_disposed = true;
			base.Dispose(disposing);
		}
	}

	private static bool ContainsJoystickBySystemId(IList<CustomInputSource.Joystick> joysticks, long systemId)
	{
		for (int i = 0; i < joysticks.Count; i++)
		{
			if (joysticks[i].systemId == systemId)
			{
				return true;
			}
		}
		return false;
	}

	private static bool ContainsSystemJoystickBySystemId(IList<UnityInputJoystickSource.Joystick> systemJoysticks, long systemId)
	{
		for (int i = 0; i < systemJoysticks.Count; i++)
		{
			if (systemJoysticks[i].systemId == systemId)
			{
				return true;
			}
		}
		return false;
	}
}
