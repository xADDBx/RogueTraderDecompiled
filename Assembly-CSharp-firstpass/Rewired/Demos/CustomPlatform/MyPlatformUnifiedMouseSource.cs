using Rewired.Platforms.Custom;
using UnityEngine;

namespace Rewired.Demos.CustomPlatform;

public class MyPlatformUnifiedMouseSource : CustomPlatformUnifiedMouseSource
{
	public override Vector2 mousePosition => Input.mousePosition;

	protected override void Update()
	{
		SetAxisValue(0, Input.GetAxis("MouseAxis1"));
		SetAxisValue(1, Input.GetAxis("MouseAxis2"));
		SetAxisValue(2, Input.GetAxis("MouseAxis3"));
		SetButtonValue(0, Input.GetButton("MouseButton0"));
		SetButtonValue(1, Input.GetButton("MouseButton1"));
		SetButtonValue(2, Input.GetButton("MouseButton2"));
		SetButtonValue(3, Input.GetButton("MouseButton3"));
		SetButtonValue(4, Input.GetButton("MouseButton4"));
		SetButtonValue(5, Input.GetButton("MouseButton5"));
		SetButtonValue(6, Input.GetButton("MouseButton6"));
	}
}
