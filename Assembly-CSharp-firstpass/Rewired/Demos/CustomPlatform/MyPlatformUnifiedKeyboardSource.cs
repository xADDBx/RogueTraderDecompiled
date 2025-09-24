using System;
using Rewired.Platforms.Custom;
using UnityEngine;

namespace Rewired.Demos.CustomPlatform;

public class MyPlatformUnifiedKeyboardSource : CustomPlatformUnifiedKeyboardSource
{
	private static readonly KeyboardKeyCode[] keyCodes = (KeyboardKeyCode[])Enum.GetValues(typeof(KeyboardKeyCode));

	protected override void OnInitialize()
	{
		base.OnInitialize();
		KeyPropertyMap keyPropertyMap = new KeyPropertyMap();
		keyPropertyMap.Set(new KeyPropertyMap.Key
		{
			keyCode = KeyboardKeyCode.A,
			label = "[A]"
		});
		keyPropertyMap.Set(new KeyPropertyMap.Key[3]
		{
			new KeyPropertyMap.Key
			{
				keyCode = KeyboardKeyCode.B,
				label = "[B]"
			},
			new KeyPropertyMap.Key
			{
				keyCode = KeyboardKeyCode.C,
				label = "[C]"
			},
			new KeyPropertyMap.Key
			{
				keyCode = KeyboardKeyCode.D,
				label = "[D]"
			}
		});
		base.keyPropertyMap = keyPropertyMap;
	}

	protected override void Update()
	{
		for (int i = 0; i < keyCodes.Length; i++)
		{
			SetKeyValue(keyCodes[i], Input.GetKey((KeyCode)keyCodes[i]));
		}
	}
}
