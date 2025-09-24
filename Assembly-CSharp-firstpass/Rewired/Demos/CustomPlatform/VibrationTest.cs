using System;
using UnityEngine;

namespace Rewired.Demos.CustomPlatform;

public class VibrationTest : MonoBehaviour
{
	public int playerId;

	public float vibrationIncrement = 0.1f;

	private float[] motors = new float[2];

	private static readonly string[] action_motors = new string[2] { "VibrationMotor0", "VibrationMotor1" };

	private static readonly string action_stop = "StopVibration";

	private Player player => ReInput.players.GetPlayer(playerId);

	private void Update()
	{
		for (int i = 0; i < action_motors.Length; i++)
		{
			if (player.GetButtonDown(action_motors[i]))
			{
				SetVibration(i, Mathf.Clamp01(motors[i] + vibrationIncrement));
			}
			if (player.GetNegativeButtonDown(action_motors[i]))
			{
				SetVibration(i, Mathf.Clamp01(motors[i] - vibrationIncrement));
			}
		}
		if (player.GetButtonDown(action_stop))
		{
			StopVibration();
		}
	}

	private void StopVibration()
	{
		player.StopVibration();
		Array.Clear(motors, 0, motors.Length);
	}

	private void SetVibration(int motorIndex, float value)
	{
		motors[motorIndex] = value;
		player.SetVibration(motorIndex, motors[motorIndex]);
	}
}
