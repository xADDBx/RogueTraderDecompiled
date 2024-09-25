using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class FallbackJoystickIdentificationDemo : MonoBehaviour
{
	private const float windowWidth = 250f;

	private const float windowHeight = 250f;

	private const float inputDelay = 1f;

	private bool identifyRequired;

	private Queue<Joystick> joysticksToIdentify;

	private float nextInputAllowedTime;

	private GUIStyle style;

	private void Awake()
	{
		if (ReInput.unityJoystickIdentificationRequired)
		{
			ReInput.ControllerConnectedEvent += JoystickConnected;
			ReInput.ControllerDisconnectedEvent += JoystickDisconnected;
			IdentifyAllJoysticks();
		}
	}

	private void JoystickConnected(ControllerStatusChangedEventArgs args)
	{
		IdentifyAllJoysticks();
	}

	private void JoystickDisconnected(ControllerStatusChangedEventArgs args)
	{
		IdentifyAllJoysticks();
	}

	public void IdentifyAllJoysticks()
	{
		Reset();
		if (ReInput.controllers.joystickCount != 0)
		{
			Joystick[] joysticks = ReInput.controllers.GetJoysticks();
			if (joysticks != null)
			{
				identifyRequired = true;
				joysticksToIdentify = new Queue<Joystick>(joysticks);
				SetInputDelay();
			}
		}
	}

	private void SetInputDelay()
	{
		nextInputAllowedTime = Time.time + 1f;
	}

	private void OnGUI()
	{
		if (!identifyRequired)
		{
			return;
		}
		if (joysticksToIdentify == null || joysticksToIdentify.Count == 0)
		{
			Reset();
			return;
		}
		Rect screenRect = new Rect((float)Screen.width * 0.5f - 125f, (float)Screen.height * 0.5f - 125f, 250f, 250f);
		GUILayout.Window(0, screenRect, DrawDialogWindow, "Joystick Identification Required");
		GUI.FocusWindow(0);
		if (!(Time.time < nextInputAllowedTime) && ReInput.controllers.SetUnityJoystickIdFromAnyButtonOrAxisPress(joysticksToIdentify.Peek().id, 0.8f, positiveAxesOnly: false))
		{
			joysticksToIdentify.Dequeue();
			SetInputDelay();
			if (joysticksToIdentify.Count == 0)
			{
				Reset();
			}
		}
	}

	private void DrawDialogWindow(int windowId)
	{
		if (identifyRequired)
		{
			if (style == null)
			{
				style = new GUIStyle(GUI.skin.label);
				style.wordWrap = true;
			}
			GUILayout.Space(15f);
			GUILayout.Label("A joystick has been attached or removed. You will need to identify each joystick by pressing a button on the controller listed below:", style);
			Joystick joystick = joysticksToIdentify.Peek();
			GUILayout.Label("Press any button on \"" + joystick.name + "\" now.", style);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Skip"))
			{
				joysticksToIdentify.Dequeue();
			}
		}
	}

	private void Reset()
	{
		joysticksToIdentify = null;
		identifyRequired = false;
	}
}
