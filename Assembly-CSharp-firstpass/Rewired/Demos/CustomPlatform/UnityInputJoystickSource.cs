using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Rewired.Interfaces;
using UnityEngine;

namespace Rewired.Demos.CustomPlatform;

public class UnityInputJoystickSource
{
	public class Joystick : IControllerVibrator
	{
		private const int maxJoysticks = 8;

		private const int maxAxes = 10;

		private const int maxButtons = 20;

		public readonly long systemId;

		public readonly string deviceName;

		public Guid deviceInstanceGuid;

		public readonly int axisCount;

		public readonly int buttonCount;

		public MyPlatformControllerIdentifier identifier;

		public readonly bool[] buttonValues;

		public readonly float[] axisValues;

		public int unityIndex;

		public int vibrationMotorCount { get; set; }

		public Joystick(long systemId, string deviceName, int axisCount, int buttonCount)
		{
			this.systemId = systemId;
			this.deviceName = deviceName;
			this.axisCount = axisCount;
			this.buttonCount = buttonCount;
			axisValues = new float[axisCount];
			buttonValues = new bool[buttonCount];
		}

		public bool GetButtonValue(int index)
		{
			if (index >= 20)
			{
				return false;
			}
			if (systemId >= 8)
			{
				return false;
			}
			return Input.GetKey((KeyCode)(350 + unityIndex * 20 + index));
		}

		public float GetAxisValue(int index)
		{
			if (index >= 10)
			{
				return 0f;
			}
			if (systemId >= 8)
			{
				return 0f;
			}
			return Input.GetAxis("Joy" + (unityIndex + 1) + "Axis" + (index + 1));
		}

		public void SetVibration(int motorIndex, float motorLevel)
		{
			Debug.Log("Vibrate " + deviceName + ": motorIndex: " + motorIndex + ", motorLevel: " + motorLevel);
		}

		public void SetVibration(int motorIndex, float motorLevel, float duration)
		{
			Debug.Log("Vibrate " + deviceName + ": motorIndex: " + motorIndex + ", motorLevel: " + motorLevel + ", duration: " + duration);
		}

		public void SetVibration(int motorIndex, float motorLevel, bool stopOtherMotors)
		{
			Debug.Log("Vibrate " + deviceName + ": motorIndex: " + motorIndex + ", motorLevel: " + motorLevel + ", stopOtherMotors: " + stopOtherMotors);
		}

		public void SetVibration(int motorIndex, float motorLevel, float duration, bool stopOtherMotors)
		{
			Debug.Log("Vibrate " + deviceName + ": motorIndex: " + motorIndex + ", motorLevel: " + motorLevel + ", duration: " + duration + ", stopOtherMotors: " + stopOtherMotors);
		}

		public float GetVibration(int motorIndex)
		{
			return 0f;
		}

		public void StopVibration()
		{
			Debug.Log("Stop vibration " + deviceName);
		}
	}

	private const float joystickCheckInterval = 1f;

	private static int systemIdCounter;

	private string[] _unityJoysticks = new string[0];

	private double _nextJoystickCheckTime;

	private List<Joystick> _joysticks;

	private ReadOnlyCollection<Joystick> _joysticks_readOnly;

	public UnityInputJoystickSource()
	{
		_joysticks = new List<Joystick>();
		_joysticks_readOnly = new ReadOnlyCollection<Joystick>(_joysticks);
		RefreshJoysticks();
	}

	public void Update()
	{
		CheckForJoystickChanges();
	}

	public IList<Joystick> GetJoysticks()
	{
		return _joysticks_readOnly;
	}

	private void CheckForJoystickChanges()
	{
		double unscaledTime = ReInput.time.unscaledTime;
		if (unscaledTime >= _nextJoystickCheckTime)
		{
			_nextJoystickCheckTime = unscaledTime + 1.0;
			if (DidJoysticksChange())
			{
				RefreshJoysticks();
			}
		}
	}

	private bool DidJoysticksChange()
	{
		string[] joystickNames = Input.GetJoystickNames();
		string[] unityJoysticks = _unityJoysticks;
		_unityJoysticks = joystickNames;
		if (unityJoysticks.Length != joystickNames.Length)
		{
			return true;
		}
		for (int i = 0; i < joystickNames.Length; i++)
		{
			if (!string.Equals(unityJoysticks[i], joystickNames[i], StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	private void RefreshJoysticks()
	{
		bool[] array = new bool[_unityJoysticks.Length];
		for (int num = _joysticks.Count - 1; num >= 0; num--)
		{
			int unityIndex = _joysticks[num].unityIndex;
			if (unityIndex >= _unityJoysticks.Length || !string.Equals(_joysticks[num].deviceName, _unityJoysticks[unityIndex]))
			{
				bool flag = false;
				for (int num2 = _unityJoysticks.Length - 1; num2 >= 0; num2--)
				{
					if (!array[num2] && string.Equals(_unityJoysticks[num2], _joysticks[num].deviceName))
					{
						_joysticks[num].unityIndex = num2;
						array[num2] = true;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Debug.Log(_joysticks[num].deviceName + " was disconnected.");
					_joysticks.RemoveAt(num);
				}
			}
			else
			{
				array[unityIndex] = true;
			}
		}
		for (int i = 0; i < _unityJoysticks.Length; i++)
		{
			if (!array[i] && !string.IsNullOrEmpty(_unityJoysticks[i]))
			{
				Joystick joystick;
				if (_unityJoysticks[i].ToLower().Contains("xbox one") || _unityJoysticks[i].ToLower().Contains("xbox bluetooth"))
				{
					joystick = new Joystick(systemIdCounter++, _unityJoysticks[i], 7, 16);
					joystick.identifier = new MyPlatformControllerIdentifier
					{
						vendorId = 1118,
						productId = 721
					};
					joystick.vibrationMotorCount = 2;
				}
				else
				{
					joystick = new Joystick(systemIdCounter++, _unityJoysticks[i], 10, 20);
				}
				joystick.unityIndex = i;
				Debug.Log(_unityJoysticks[i] + " was connected.");
				_joysticks.Add(joystick);
			}
		}
	}
}
